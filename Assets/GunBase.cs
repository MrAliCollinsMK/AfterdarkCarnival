using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Stats")]
    public float fireRate = 8f;      // shots per second
    public float range = 80f;
    public float damage = 1f;

    [Header("Refs")]
    public Transform muzzle;         // optional
    public GunRecoil recoil;         // optional

    public abstract void Fire(Transform shootOrigin);
}
