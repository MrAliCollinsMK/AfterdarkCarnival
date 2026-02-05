using UnityEngine;

namespace AfterdarkFPS
{
    public class TargetDummy : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float movementAmplitude = 2f;
        [SerializeField] private float movementSpeed = 1.5f;

        private float health;
        private Vector3 startPosition;
        private float randomPhase;

        private void Awake()
        {
            health = maxHealth;
            startPosition = transform.position;
            randomPhase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            var bobOffset = Mathf.Sin((Time.time * movementSpeed) + randomPhase) * movementAmplitude;
            transform.position = startPosition + new Vector3(0f, 0f, bobOffset);
        }

        public bool ApplyDamage(float damage)
        {
            health -= damage;
            if (health > 0f)
            {
                return false;
            }

            Destroy(gameObject);
            return true;
        }
    }
}
