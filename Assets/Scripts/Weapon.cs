using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Claims;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

[RequireComponent(typeof (Rigidbody))]
public class Weapon : VRTK_InteractableObject
{
    [SerializeField]
    protected float shootingForce;

    [SerializeField]
    protected Transform bulletSpawn;

    [SerializeField]
    private float recoilForce;

    [SerializeField]
    private float damage;

    [SerializeField]
    private AudioSource shootAudioSource;

    [SerializeField]
    private AudioSource mainAudioSource;

    [System.Serializable]
    public class soundClips
    {
        public AudioClip shootSound;

        public AudioClip reloadSoundOutOfAmmo;

        public AudioClip reloadSoundAmmoLeft;
    }

    [SerializeField]
    private soundClips SoundClips;

    [SerializeField]
    protected int bulletNumber;

    [SerializeField]
    private VRTK_ControllerEvents _controller;

    protected int currentBulletNumber;

    private bool reloading = false;

    private Random rd = new Random();

    [SerializeField]
    private GameObject casingPrefab;

    [SerializeField]
    private Transform casingSpawn;

    [SerializeField]
    private Text bulletNumberText;

    [SerializeField]
    private Image iconImage;

    private Rigidbody _rigbody;

    protected void Start()
    {
        shootAudioSource.clip = SoundClips.shootSound;
        currentBulletNumber = bulletNumber;
        _rigbody = GetComponent<Rigidbody>();
    }

    protected override void Update()
    {
        base.Update();
        Express();
        reloadBullets();
    }

    private void Collect()
    {
    }

    protected void Express()
    {
        if (IsGrabbed())
        {
            bulletNumberText.gameObject.SetActive(true);
            iconImage.gameObject.SetActive(true);
            bulletNumberText.text = currentBulletNumber + "/" + bulletNumber;
        }
        else
        {
            bulletNumberText.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);
        }
    }

    public override void StartUsing(VRTK_InteractUse currentUsingObject)
    {
        base.StartUsing(currentUsingObject);

        StartShooting();
    }

    public override void Ungrabbed(
        VRTK_InteractGrab currentGrabbingObject = null
    )
    {
        base.Ungrabbed(currentGrabbingObject);
        _rigbody.useGravity = true;
    }

    protected void reloadBullets()
    {
        if (
            currentBulletNumber != bulletNumber &&
            (_controller.buttonOnePressed || Input.GetKeyDown(KeyCode.R)) &&
            !reloading //左手按钮1或R键
        )
        {
            mainAudioSource.clip =
                currentBulletNumber == 0
                    ? SoundClips.reloadSoundOutOfAmmo
                    : SoundClips.reloadSoundAmmoLeft;

            mainAudioSource.Play();
            reloading = true;
            StartCoroutine("reloadingBullets");
        }
    }

    private IEnumerator reloadingBullets()
    {
        yield return new WaitForSeconds(mainAudioSource.clip.length);

        reloading = false;
        currentBulletNumber = bulletNumber;
    }

    // private void SetupInteractableWeaponEvents()
    // {
    //     // interactableObject.Grabbed.AddListener
    //     interactableFacade.Grabbed.AddListener (PickUpWeapon);
    //     interactableFacade.Ungrabbed.AddListener (DropWeapon);
    //     shotAction.ValueChanged.AddListener (ShootingController);
    // }
    protected virtual void StartShooting()
    {
        if (!reloading && currentBulletNumber > 0) Shoot();
    }

    protected virtual void StopShooting()
    {
    }

    protected virtual void Shoot()
    {
        currentBulletNumber--;

        // Debug.Log ("Shooting");
        shootAudioSource.Play();
        ApplyRecoil();
        ApplyCasing();
    }

    private void ApplyRecoil()
    {
        // base.interactableRigidbody
        //     .AddRelativeForce((Vector3.back+Vector3.up) * recoilForce, ForceMode.Impulse);
        transform.localEulerAngles +=
            (new Vector3(Random.Range(0f, 2f), 0, Random.Range(0f, 1f))) *
            -1 *
            recoilForce;
    }

    private void ApplyCasing()
    {
        //Spawn casing prefab at spawnpoint
        GameObject casing =
            Instantiate(casingPrefab,
            casingSpawn.position,
            casingSpawn.rotation);
    }

    public float GetShootForce()
    {
        return shootingForce;
    }

    public float GetDamage()
    {
        return damage;
    }
}
