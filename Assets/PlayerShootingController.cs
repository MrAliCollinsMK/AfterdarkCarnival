using UnityEngine;

public class PlayerShootingController : MonoBehaviour
{
    public Transform shootOrigin;          // usually Main Camera
    public WeaponBase currentWeapon;

    private FirstPersonPlayerController movement;
    private float nextFireTime;

    void Awake()
    {
        movement = GetComponent<FirstPersonPlayerController>();

        if(shootOrigin == null && Camera.main != null)
            shootOrigin = Camera.main.transform;
    }

    void Update()
    {
        if(movement == null || currentWeapon == null)
            return;

        var input = movement.GetInput();

        // Reload should work even when you're not firing
        if(input.ReloadPressed)
            currentWeapon.Reload();

        // Decide if we WANT to attempt a shot this frame
        bool wantsShotThisFrame;

        if(currentWeapon.isAutomatic)
            wantsShotThisFrame = input.FireHeld || input.FirePressed; // held drives auto
        else
            wantsShotThisFrame = input.FirePressed; // semi-auto ignores held

        if(!wantsShotThisFrame)
        {
            currentWeapon.OnFireReleased(); // stops loop audio if a future weapon uses it
            return;
        }

        // Rate limit
        float rate = Mathf.Max(0.01f, currentWeapon.fireRate);
        float interval = 1f / rate;

        if(Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + interval;
        currentWeapon.Fire(shootOrigin);
    }
}
