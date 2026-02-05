using UnityEngine;

public class PlayerShootingController : MonoBehaviour
{
    public Transform shootOrigin;          // usually Main Camera
    public WeaponBase currentWeapon;

    private FirstPersonPlayerController movement; // for input access
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

        var input = movement.GetInput(); // we’ll expose this cleanly

        bool wantsFire =
            input.FirePressed ||
            (input.FireHeld && currentWeapon.fireRate > 0f);

        if(!wantsFire) return;

        float interval = 1f / currentWeapon.fireRate;
        if(Time.time < nextFireTime) return;

        nextFireTime = Time.time + interval;
        currentWeapon.Fire(shootOrigin);
    }
}
