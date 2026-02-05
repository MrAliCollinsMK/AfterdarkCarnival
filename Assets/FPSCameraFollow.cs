using UnityEngine;

public class FPSCameraFollow : MonoBehaviour
{
    public Transform player;          // drag Player here
    public Vector3 offset = new Vector3(0f, 1.6f, 0f); // head height

    void LateUpdate()
    {
        if(!player) return;

        transform.position = player.position + offset;
    }
}
