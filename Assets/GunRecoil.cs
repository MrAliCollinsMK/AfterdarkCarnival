using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Kick")]
    public float kickBack = 0.06f;   // local z
    public float kickUp = 1.6f;      // degrees

    [Header("Return")]
    public float returnSpeed = 18f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Awake()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void LateUpdate()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, returnSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, startRot, returnSpeed * Time.deltaTime);
    }

    public void Fire()
    {
        transform.localPosition += Vector3.back * kickBack;
        transform.localRotation *= Quaternion.Euler(-kickUp, 0f, 0f);
    }
}
