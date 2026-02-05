using UnityEngine;

namespace AfterdarkFPS
{
    public class WeaponShooter : MonoBehaviour
    {
        [SerializeField] private float damage = 40f;
        [SerializeField] private float range = 140f;
        [SerializeField] private float fireRate = 8f;

        private Camera playerCamera;
        private float nextFireTime;
        private int score;

        private void Awake()
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + (1f / fireRate);
                Shoot();
            }
        }

        private void Shoot()
        {
            if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hit, range))
            {
                return;
            }

            var target = hit.collider.GetComponent<TargetDummy>();
            if (target == null)
            {
                return;
            }

            var wasDestroyed = target.ApplyDamage(damage);
            if (wasDestroyed)
            {
                score++;
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20, 20, 250, 30), $"Score: {score}");
            GUI.Label(new Rect(Screen.width * 0.5f - 70f, Screen.height - 36f, 220, 30), "WASD Move | Shift Sprint | LMB Fire");

            var reticleRect = new Rect((Screen.width * 0.5f) - 5f, (Screen.height * 0.5f) - 5f, 10f, 10f);
            GUI.Label(reticleRect, "+");
        }
    }
}
