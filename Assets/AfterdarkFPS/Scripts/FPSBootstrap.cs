using UnityEngine;

namespace AfterdarkFPS
{
    /// <summary>
    /// Builds a playable FPS sandbox at runtime so the game works in any scene.
    /// Attach this to an empty GameObject and press play.
    /// </summary>
    public class FPSBootstrap : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private Vector3 arenaSize = new Vector3(60f, 8f, 60f);
        [SerializeField] private int targetCount = 12;

        [Header("Player")]
        [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 2f, -20f);

        private void Start()
        {
            BuildArena();
            SpawnPlayer();
            SpawnTargets();
        }

        private void BuildArena()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(arenaSize.x / 10f, 1f, arenaSize.z / 10f);

            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.08f, 0.1f, 0.11f);
            ground.GetComponent<Renderer>().material = material;

            CreateWall(new Vector3(0f, arenaSize.y * 0.5f, arenaSize.z * 0.5f), new Vector3(arenaSize.x, arenaSize.y, 1f));
            CreateWall(new Vector3(0f, arenaSize.y * 0.5f, -arenaSize.z * 0.5f), new Vector3(arenaSize.x, arenaSize.y, 1f));
            CreateWall(new Vector3(arenaSize.x * 0.5f, arenaSize.y * 0.5f, 0f), new Vector3(1f, arenaSize.y, arenaSize.z));
            CreateWall(new Vector3(-arenaSize.x * 0.5f, arenaSize.y * 0.5f, 0f), new Vector3(1f, arenaSize.y, arenaSize.z));

            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.03f, 0.03f, 0.06f);
            RenderSettings.fogDensity = 0.02f;

            var directionalLight = new GameObject("Directional Light");
            var light = directionalLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.7f;
            directionalLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private static void CreateWall(Vector3 position, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "ArenaWall";
            wall.transform.position = position;
            wall.transform.localScale = scale;

            var wallMaterial = new Material(Shader.Find("Standard"));
            wallMaterial.color = new Color(0.2f, 0.05f, 0.08f);
            wall.GetComponent<Renderer>().material = wallMaterial;
        }

        private void SpawnPlayer()
        {
            var player = new GameObject("FPSPlayer");
            player.transform.position = spawnPosition;

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;

            player.AddComponent<FPSPlayerController>();
            player.AddComponent<WeaponShooter>();

            var bodyMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyMesh.name = "BodyMesh";
            bodyMesh.transform.SetParent(player.transform);
            bodyMesh.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            bodyMesh.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            Destroy(bodyMesh.GetComponent<Collider>());

            var cameraObject = new GameObject("PlayerCamera");
            cameraObject.transform.SetParent(player.transform);
            cameraObject.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        private void SpawnTargets()
        {
            for (var i = 0; i < targetCount; i++)
            {
                var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                target.name = $"Target_{i:00}";

                var x = Random.Range(-arenaSize.x * 0.4f, arenaSize.x * 0.4f);
                var z = Random.Range(-arenaSize.z * 0.25f, arenaSize.z * 0.45f);
                target.transform.position = new Vector3(x, 1f, z);

                target.AddComponent<Rigidbody>().isKinematic = true;
                target.AddComponent<TargetDummy>();

                var color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.95f, 0.95f, 0.2f), i / (float)Mathf.Max(1, targetCount - 1));
                var targetMaterial = new Material(Shader.Find("Standard"));
                targetMaterial.color = color;
                target.GetComponent<Renderer>().material = targetMaterial;
            }
        }
    }
}
