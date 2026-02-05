using UnityEngine;

#if UNITY_SWITCH && !UNITY_EDITOR
using nn.hid;
#endif

[DisallowMultipleComponent]
public class FirstPersonPlayerController : MonoBehaviour
{
    // -------- Input abstraction --------
    public interface IPlayerInput
    {
        void Initialize();
        void Tick();                     // call once per frame
        Vector2 Move { get; }            // WASD / Left stick
        Vector2 Look { get; }            // Mouse / Right stick
        bool JumpPressed { get; }        // edge
        bool JumpHeld { get; }           // hold
        bool SprintHeld { get; }         // hold

        bool FirePressed { get; }
        bool FireHeld { get; }
    }

    // -------- PC / Editor input (fast + reliable) --------
    private class UnityEditorInput : IPlayerInput
    {
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool SprintHeld { get; private set; }

       public bool FirePressed { get; private set; }
       public bool FireHeld { get; private set; }

        public void Initialize()
        {
            // Optional: lock cursor for FPS testing in editor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Tick()
        {
            // Old Input Manager axes (works instantly in most projects)
            float mx = Input.GetAxisRaw("Horizontal");
            float my = Input.GetAxisRaw("Vertical");
            Move = new Vector2(mx, my);

            float lx = Input.GetAxis("Mouse X");
            float ly = Input.GetAxis("Mouse Y");
            Look = new Vector2(lx, ly);

            JumpPressed = Input.GetButtonDown("Jump");
            JumpHeld = Input.GetButton("Jump");
            SprintHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            FirePressed = Input.GetMouseButtonDown(0);
            FireHeld = Input.GetMouseButton(0);
        }
    }

#if UNITY_SWITCH && !UNITY_EDITOR
    // -------- Switch Npad input --------
    private class SwitchNpadInput : IPlayerInput
    {
        private NpadId npadId = NpadId.Invalid;
        private NpadStyle npadStyle = NpadStyle.Invalid;
        private NpadState npadState = new NpadState();
        private bool updatePadState;

        // cached values we expose
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool SprintHeld { get; private set; }
         public bool FirePressed { get; private set; }
       public bool FireHeld { get; private set; }

        public void Initialize()
        {
            Npad.Initialize();
            Npad.SetSupportedIdType(new NpadId[] { NpadId.Handheld, NpadId.No1 });
            Npad.SetSupportedStyleSet(NpadStyle.FullKey | NpadStyle.Handheld | NpadStyle.JoyDual);
        }

        public void Tick()
        {
            updatePadState = UpdatePadState();

            if(!updatePadState)
            {
                Move = Vector2.zero;
                Look = Vector2.zero;
                JumpPressed = false;
                JumpHeld = false;
                SprintHeld = false;
                FirePressed = false;
                FireHeld = false;
                return;
            }

            // Sticks
            AnalogStickState lStick = npadState.analogStickL;
            AnalogStickState rStick = npadState.analogStickR;
            Move = new Vector2(lStick.fx, lStick.fy);
            Look = new Vector2(rStick.fx, rStick.fy);

            // Buttons (defaults: A=jump, B=sprint; you can change mappings easily)
            JumpPressed = npadState.GetButtonDown(NpadButton.A);
            JumpHeld = npadState.GetButton(NpadButton.A);

            // Sprint on B (or click LStick if you prefer: NpadButton.StickL)
            SprintHeld = npadState.GetButton(NpadButton.B);

            FirePressed = GetButtonDown(NpadButton.ZR);
            FireHeld = npadState.GetButton(NpadButton.ZR);
        }

        private bool UpdatePadState()
        {
            // Prefer handheld if active
            NpadStyle handheldStyle = Npad.GetStyleSet(NpadId.Handheld);
            NpadState handheldState = npadState;

            if(handheldStyle != NpadStyle.None)
            {
                Npad.GetState(ref handheldState, NpadId.Handheld, handheldStyle);
                if(handheldState.buttons != NpadButton.None)
                {
                    npadId = NpadId.Handheld;
                    npadStyle = handheldStyle;
                    npadState = handheldState;
                    return true;
                }
            }

            // Otherwise controller 1
            NpadStyle no1Style = Npad.GetStyleSet(NpadId.No1);
            NpadState no1State = npadState;

            if(no1Style != NpadStyle.None)
            {
                Npad.GetState(ref no1State, NpadId.No1, no1Style);
                if(no1State.buttons != NpadButton.None)
                {
                    npadId = NpadId.No1;
                    npadStyle = no1Style;
                    npadState = no1State;
                    return true;
                }
            }

            // Fall back to previous active id/style if still present
            if((npadId == NpadId.Handheld) && (handheldStyle != NpadStyle.None))
            {
                npadId = NpadId.Handheld;
                npadStyle = handheldStyle;
                npadState = handheldState;
            }
            else if((npadId == NpadId.No1) && (no1Style != NpadStyle.None))
            {
                npadId = NpadId.No1;
                npadStyle = no1Style;
                npadState = no1State;
            }
            else
            {
                npadId = NpadId.Invalid;
                npadStyle = NpadStyle.Invalid;
                npadState.Clear();
                return false;
            }

            return true;
        }

        public bool GetButtonDown(NpadButton npadButton)
	{
	
			// Use the cached state
			if (updatePadState)
			{
				return npadState.GetButtonDown(npadButton);
			}
			else
			{
				return false;
			}
		
	
	}

	public bool GetButtonUp(NpadButton npadButton)
	{
		
			if (updatePadState)
			{
				return npadState.GetButtonUp(npadButton);
			}
			else
			{
				return false;
	}
    }
#endif

    // -------- Controller refs --------
    [Header("Refs")]
    public Transform cameraPivot;              // assign your Camera transform
    private CharacterController cc;

    // -------- Look --------
    [Header("Look")]
    public float mouseSensitivity = 2.0f;      // PC feel
    public float stickSensitivity = 140.0f;    // degrees/sec feel (Switch)
    public bool invertY = false;
    [Range(0f, 0.35f)] public float stickDeadzone = 0.12f;
    public bool smoothLook = false;
    [Range(0f, 30f)] public float lookSmoothing = 14f;

    // -------- Movement --------
    [Header("Movement")]
    public float walkSpeed = 5.5f;
    public float sprintSpeed = 7.5f;
    public float acceleration = 18f;
    public float airAcceleration = 6f;
    public float gravity = -24f;
    public float jumpHeight = 1.25f;

    [Header("Jump Tuning")]
    [Range(0f, 0.25f)] public float coyoteTime = 0.12f;
    [Range(0f, 0.25f)] public float jumpBuffer = 0.12f;

    [Header("Movement Feel")]
    public float deceleration = 50f;     // higher = snappier stop
    public float airDeceleration = 2f;   // low = keep momentum in air
    public float stopSpeed = 0.08f;      // kills micro drift
    public float inputCutoff = 0.12f;    // treats tiny stick drift as zero

    // -------- Internals --------
    private IPlayerInput input;
    private float pitch;
    private Vector2 smoothedLook;
    private Vector3 velocity;                  // includes vertical
    private Vector3 planarVel;                 // horizontal smoothing
    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;

    public IPlayerInput GetInput() => input;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if(cc == null) cc = gameObject.AddComponent<CharacterController>();

        if(cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;

#if UNITY_SWITCH && !UNITY_EDITOR
        input = new SwitchNpadInput();
#else
        input = new UnityEditorInput();
#endif
        input.Initialize();
    }

    void Update()
    {
        input.Tick();

        // Track jump buffer
        if(input.JumpPressed)
            lastJumpPressedTime = Time.time;

        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        Vector2 raw = input.Look;

        // deadzone for sticks (mouse is usually tiny values but we won’t deadzone it aggressively)
        raw = ApplyRadialDeadzone(raw, stickDeadzone);

#if UNITY_SWITCH && !UNITY_EDITOR
        // stick is already normalized-ish; scale to degrees per second
        Vector2 lookDelta = raw * stickSensitivity * Time.deltaTime;
#else
        // mouse is already "per frame"; scale with sensitivity
        Vector2 lookDelta = raw * mouseSensitivity;
#endif

        if(invertY) lookDelta.y = -lookDelta.y;

        if(smoothLook)
        {
            smoothedLook = Vector2.Lerp(smoothedLook, lookDelta, 1f - Mathf.Exp(-lookSmoothing * Time.deltaTime));
            lookDelta = smoothedLook;
        }

        // yaw rotates body
        transform.Rotate(0f, lookDelta.x, 0f);

        // pitch rotates camera
        pitch -= lookDelta.y;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        if(cameraPivot != null)
            // IMPORTANT: camera is not parented, so set WORLD rotation.
            // Yaw comes from the player, pitch from the camera.
            cameraPivot.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
    }

    private void HandleMovement()
    {
        bool grounded = cc.isGrounded;

        if(grounded)
            lastGroundedTime = Time.time;

        // Desired move in local space
        Vector2 move = input.Move;

        // Kill tiny drift (especially on Switch sticks, but helps everywhere)
        if(move.magnitude < inputCutoff)
            move = Vector2.zero;

        // Optional: apply same radial deadzone style as look for a nicer stick feel
        // (safe for keyboard too; keyboard is already -1/0/1)
        move = ApplyRadialDeadzone(move, Mathf.Min(inputCutoff, 0.35f));

        if(move.sqrMagnitude > 1f)
            move.Normalize();

        Vector3 wishDir = (transform.right * move.x + transform.forward * move.y);
        float targetSpeed = input.SprintHeld ? sprintSpeed : walkSpeed;
        Vector3 wishVel = wishDir * targetSpeed;

        float accelRate = grounded ? acceleration : airAcceleration;
        float decelRate = grounded ? deceleration : airDeceleration;

        bool hasInput = move.sqrMagnitude > 0.0001f;

        if(hasInput)
        {
            planarVel = Vector3.MoveTowards(planarVel, wishVel, accelRate * Time.deltaTime);
        }
        else
        {
            planarVel = Vector3.MoveTowards(planarVel, Vector3.zero, decelRate * Time.deltaTime);
            if(planarVel.magnitude < stopSpeed)
                planarVel = Vector3.zero;
        }

        // Jump: coyote + buffer
        bool canCoyote = (Time.time - lastGroundedTime) <= coyoteTime;
        bool buffered = (Time.time - lastJumpPressedTime) <= jumpBuffer;

        if(buffered && (grounded || canCoyote))
        {
            // consume buffer
            lastJumpPressedTime = -999f;
            lastGroundedTime = -999f;

            // v = sqrt(2gh) (gravity is negative)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        if(grounded && velocity.y < 0f)
            velocity.y = -2f; // keeps you stuck to ground, avoids tiny “float”

        velocity.y += gravity * Time.deltaTime;

        Vector3 final = planarVel + Vector3.up * velocity.y;
        cc.Move(final * Time.deltaTime);
    }

    private static Vector2 ApplyRadialDeadzone(Vector2 v, float dz)
    {
        float m = v.magnitude;
        if(m <= dz) return Vector2.zero;
        float scaled = (m - dz) / (1f - dz);
        return v / m * Mathf.Clamp01(scaled);
    }
}
