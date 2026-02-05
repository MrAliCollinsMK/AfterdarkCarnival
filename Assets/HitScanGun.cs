using UnityEngine;

public class HitscanGun : WeaponBase
{
    public LayerMask hitMask = ~0;

    public override void Fire(Transform shootOrigin)
    {
        if(recoil != null)
            recoil.Fire();

        Ray ray = new Ray(shootOrigin.position, shootOrigin.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 11f);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 11f);
        }
        // Placeholder – you’ll swap this later
        Debug.Log($"Hit {hit.collider.name}");

            // Example damage hook
            // var dmg = hit.collider.GetComponentInParent<IDamageable>();
            // if (dmg != null) dmg.ApplyDamage(damage);
        }
    }

