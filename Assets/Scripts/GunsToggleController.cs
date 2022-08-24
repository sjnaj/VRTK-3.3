using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class GunsToggleController : MonoBehaviour
{
    [SerializeField]
    private VRTK_ControllerEvents rightControllerEvents;

    [SerializeField]
    private VRTK_ControllerEvents leftControllerEvents;

    [SerializeField]
    private VRTK_InteractGrab leftGrab;

    [SerializeField]
    private VRTK_InteractGrab rightGrab;

    [SerializeField]
    private byte catchGunNumber = 2;

    [SerializeField]
    private AudioClip takeOutSound;

    [SerializeField]
    private AudioClip holsterSound;

    private List<GameObject> gunList;

    private GameObject currentGun = null;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        gunList = new List<GameObject>(catchGunNumber);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Toggle();
    }

    void Toggle()
    {
        if (
            (
            leftControllerEvents.buttonTwoPressed || Input.GetKeyDown(KeyCode.K) //收枪,在此之前枪不能松手
            ) &&
            (currentGun = GetGrippedGun()) != null
        )
        {
            Put();
        }
        else if (
            (
            rightControllerEvents.buttonTwoPressed ||
            Input.GetKeyDown(KeyCode.L) //取枪，放在面前，还需手动拾取
            ) &&
            GetGrippedGun() == null
        )
        {
            Get();
        }
    }

    void Put() //收枪
    {
        if (gunList.Count == catchGunNumber)
        {
            gunList.RemoveAt(0); //超过数量则把最先的枪扔掉
        }

        gunList.Add (currentGun);

        currentGun
            .GetComponent<VRTK_InteractableObject>()
            .ForceStopInteracting(); //强制松手
        currentGun.transform.position = Vector3.up * 100; //  并隐藏

        audioSource.clip = holsterSound;
        audioSource.Play();
    }

    void Get() //取枪（已拾取过）
    {
        if (gunList.Count > 0)
        {
            audioSource.clip = takeOutSound;
            audioSource.Play();
            GameObject newGun =
                gunList[(gunList.IndexOf(currentGun) + 1) % gunList.Count];
            newGun.SetActive(true);
            newGun.GetComponent<Rigidbody>().useGravity = false; //保持悬浮容易取
            newGun.GetComponent<Rigidbody>().velocity = Vector3.zero;
            newGun.transform.position =
                transform.position + Vector3.up * 1.5f + Vector3.forward * 0.5f; // 放在人物前方
            newGun.transform.rotation = transform.rotation;
            newGun.transform.Rotate(0, 90, 0); //旋转方便拾取
        }
    }

    private GameObject GetGrippedGun()
    {
        return rightGrab.GetGrabbedObject() == null
            ? leftGrab.GetGrabbedObject()
            : rightGrab.GetGrabbedObject();
    }
}
