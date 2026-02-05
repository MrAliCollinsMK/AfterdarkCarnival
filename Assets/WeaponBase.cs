using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Stats")]
    public float fireRate = 8f;      // shots per second
    public float range = 80f;
    public float damage = 1f;

    [Header("Ammo")]
    public int magazineSize = 12;
    public int ammoInMag = 12;
    public float reloadTime = 1.2f;

    [Header("Firing")]
    public bool isAutomatic = false;

    protected bool isReloading;

    [Header("Refs")]
    public GameObject muzzleFlash;         // optional (for VFX)
    public float muzzleFlashDuration = 0.05f;
    public GunRecoil recoil;         // optional

    public enum FireAudioMode
    {
        OneShot,
        Loop
    }

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip emptyClip;
    public AudioClip reloadClip;
    public FireAudioMode fireAudioMode = FireAudioMode.OneShot;

    // ---------- Public API ----------

    // Called by PlayerShootingController
    public abstract void Fire(Transform shootOrigin);


    public virtual void Reload()
    {
        if(isReloading || ammoInMag >= magazineSize)
            return;

        StartCoroutine(ReloadRoutine());
    }

    // ---------- Protected helpers (for derived weapons only) ----------

    protected bool ConsumeAmmo()
    {
        if(isReloading)
            return false;

        if(ammoInMag <= 0)
        {
            PlayEmptySound();
            return false;
        }

        ammoInMag--;
        return true;
    }

    protected void PlayFireSound()
    {
        if(!audioSource || !fireClip)
            return;

        switch(fireAudioMode)
        {
            case FireAudioMode.OneShot:
                audioSource.PlayOneShot(fireClip);
                break;

            case FireAudioMode.Loop:
                if(!audioSource.isPlaying)
                {
                    audioSource.clip = fireClip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                break;
        }
    }

    protected void StopFireSound()
    {
        if(!audioSource)
            return;

        if(fireAudioMode == FireAudioMode.Loop && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    protected void PlayEmptySound()
    {
        if(audioSource && emptyClip)
            audioSource.PlayOneShot(emptyClip);
    }

    protected void PlayMuzzleFlash()
    {
        if(!muzzleFlash)
            return;

        muzzleFlash.SetActive(true);
        CancelInvoke(nameof(HideMuzzleFlash));
        Invoke(nameof(HideMuzzleFlash), muzzleFlashDuration);
    }

    void HideMuzzleFlash()
    {
        if(muzzleFlash)
            muzzleFlash.SetActive(false);
    }
    public void OnFireReleased()
    {
        StopFireSound();
    }

    // ---------- Internals ----------

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        StopFireSound(); // important for automatic weapons

        if(audioSource && reloadClip)
            audioSource.PlayOneShot(reloadClip);
        yield return new WaitForSeconds(reloadTime);
        ammoInMag = magazineSize;
        isReloading = false;
    }
}
