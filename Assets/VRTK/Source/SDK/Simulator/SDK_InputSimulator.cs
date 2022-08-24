using System.Numerics;

namespace VRTK
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The `[VRSimulator_CameraRig]` prefab is a mock Camera Rig set up that can be used to develop with VRTK without the need for VR Hardware.
    /// </summary>
    /// <remarks>
    /// Use the mouse and keyboard to move around both play area and hands and interacting with objects without the need of a hmd or VR controls.
    /// </remarks>
    public class SDK_InputSimulator : MonoBehaviour
    {
        /// <summary>
        /// Mouse input mode types
        /// </summary>
        public enum MouseInputMode
        {
            /// <summary>
            /// Mouse movement is always treated as mouse input.
            /// </summary>
            Always,
            /// <summary>
            /// Mouse movement is only treated as movement when a button is pressed.
            /// </summary>
            RequiresButtonPress
        }


#region Public fields

        [Header("General Settings")]
        [
            Tooltip(
                "Show control information in the upper left corner of the screen.")
        ]
        public bool showControlHints = true;

        [Tooltip("Hide hands when disabling them.")]
        public bool hideHandsAtSwitch = false;

        [Tooltip("Reset hand position and rotation when enabling them.")]
        public bool resetHandsAtSwitch = true;

        [
            Tooltip(
                "Displays an axis helper to show which axis the hands will be moved through.")
        ]
        public bool showHandAxisHelpers = true;

        [Header("Mouse Cursor Lock Settings")]
        [Tooltip("Lock the mouse cursor to the game window.")]
        public bool lockMouseToView = true;

        [
            Tooltip(
                "Whether the mouse movement always acts as input or requires a button press.")
        ]
        public MouseInputMode mouseMovementInput = MouseInputMode.Always;

        [Header("Manual Adjustment Settings")]
        [Tooltip("Adjust hand movement speed.")]
        public float handMoveMultiplier = 0.002f;

        [Tooltip("Adjust hand rotation speed.")]
        public float handRotationMultiplier = 0.5f;

        [Tooltip("Adjust player movement speed.")]
        public float playerMoveMultiplier = 5f;

        [Tooltip("Adjust player rotation speed.")]
        public float playerRotationMultiplier = 0.5f;

        [Tooltip("Adjust player sprint speed.")]
        public float playerSprintMultiplier = 2f;

        [Tooltip("Adjust the speed of the cursor movement in locked mode.")]
        public float lockedCursorMultiplier = 5f;

        [Tooltip("The Colour of the GameObject representing the left hand.")]
        public Color leftHandColor = Color.red;

        [Tooltip("The Colour of the GameObject representing the right hand.")]
        public Color rightHandColor = Color.green;

        [Header("Operation Key Binding Settings")]
        [
            Tooltip(
                "Key used to enable mouse input if a button press is required.")
        ]
        public KeyCode mouseMovementKey = KeyCode.Mouse1;

        [Tooltip("Key used to toggle control hints on/off.")]
        public KeyCode toggleControlHints = KeyCode.F1;

        [Tooltip("Key used to toggle control hints on/off.")]
        public KeyCode toggleMouseLock = KeyCode.F4;

        [Tooltip("Key used to switch between left and righ hand.")]
        public KeyCode changeHands = KeyCode.Tab;

        [Tooltip("Key used to switch hands On/Off.")]
        public KeyCode handsOnOff = KeyCode.LeftAlt;

        [
            Tooltip(
                "Key used to switch between positional and rotational movement.")
        ]
        public KeyCode rotationPosition = KeyCode.LeftShift;

        [Tooltip("Key used to switch between X/Y and X/Z axis.")]
        public KeyCode changeAxis = KeyCode.LeftControl;

        [Tooltip("Key used to distance pickup with left hand.")]
        public KeyCode distancePickupLeft = KeyCode.Mouse0;

        [Tooltip("Key used to distance pickup with right hand.")]
        public KeyCode distancePickupRight = KeyCode.Mouse1;

        [Tooltip("Key used to enable distance pickup.")]
        public KeyCode distancePickupModifier = KeyCode.LeftControl;

        [Header("Movement Key Binding Settings")]
        [Tooltip("Key used to move forward.")]
        public KeyCode moveForward = KeyCode.W;

        [Tooltip("Key used to move to the left.")]
        public KeyCode moveLeft = KeyCode.A;

        [Tooltip("Key used to move backwards.")]
        public KeyCode moveBackward = KeyCode.S;

        [Tooltip("Key used to move to the right.")]
        public KeyCode moveRight = KeyCode.D;

        [Tooltip("Key used to sprint.")]
        public KeyCode sprint = KeyCode.LeftShift;

        [Header("Controller Key Binding Settings")]
        [Tooltip("Key used to simulate trigger button.")]
        public KeyCode triggerAlias = KeyCode.Mouse1;

        [Tooltip("Key used to simulate grip button.")]
        public KeyCode gripAlias = KeyCode.Mouse0;

        [Tooltip("Key used to simulate touchpad button.")]
        public KeyCode touchpadAlias = KeyCode.Q;

        [Tooltip("Key used to simulate button one.")]
        public KeyCode buttonOneAlias = KeyCode.E;

        [Tooltip("Key used to simulate button two.")]
        public KeyCode buttonTwoAlias = KeyCode.R;

        [Tooltip("Key used to simulate start menu button.")]
        public KeyCode startMenuAlias = KeyCode.F;

        [
            Tooltip(
                "Key used to switch between button touch and button press mode.")
        ]
        public KeyCode touchModifier = KeyCode.T;

        [Tooltip("Key used to switch between hair touch mode.")]
        public KeyCode hairTouchModifier = KeyCode.H;


#endregion



#region Protected fields

        protected bool isHand = false;

        protected GameObject hintCanvas;

        protected Text hintText;

        protected Transform rightHand;

        protected Transform leftHand;

        protected Transform currentHand;

        protected Vector3 oldPos;

        protected Transform neck;

        protected SDK_ControllerSim rightController;

        protected SDK_ControllerSim leftController;

        protected static GameObject cachedCameraRig;

        protected static bool destroyed = false;

        protected float sprintMultiplier = 1;

        protected GameObject crossHairPanel;

        protected Transform leftHandHorizontalAxisGuide;

        protected Transform leftHandVerticalAxisGuide;

        protected Transform rightHandHorizontalAxisGuide;

        protected Transform rightHandVerticalAxisGuide;

        private CapsuleCollider _collider;

        private Rigidbody _rigidbody;

        private Rigidbody _neckRigidbody;

        private class SmoothVelocity
        {
            private float _current;

            private float _currentVelocity;

            /// Returns the smoothed velocity.
            public float Update(float target, float smoothTime)
            {
                return _current =
                    Mathf
                        .SmoothDamp(_current,
                        target,
                        ref _currentVelocity,
                        smoothTime);
            }

            public float Current
            {
                set
                {
                    _current = value;
                }
            }
        }

        private SmoothVelocity _velocityX = new SmoothVelocity();

        private SmoothVelocity _velocityZ = new SmoothVelocity();

        private float movementSmoothness = 0.125f;

        private readonly RaycastHit[] _wallCastResults = new RaycastHit[8];

        private readonly RaycastHit[] _groundCastResults = new RaycastHit[8];

        private Vector3 neckPosition;

        private Vector3 rightHandPosition;

        private Vector3 leftHandPosition;

        [SerializeField]
        private VRTK_ControllerEvents rightHandEventController;


#endregion


        /// <summary>
        /// The FindInScene method is used to find the `[VRSimulator_CameraRig]` GameObject within the current scene.
        /// </summary>
        /// <returns>Returns the found `[VRSimulator_CameraRig]` GameObject if it is found. If it is not found then it prints a debug log error.</returns>
        public static GameObject FindInScene()
        {
            if (cachedCameraRig == null && !destroyed)
            {
                cachedCameraRig =
                    VRTK_SharedMethods
                        .FindEvenInactiveGameObject<SDK_InputSimulator>(null,
                        true);
                if (!cachedCameraRig)
                {
                    VRTK_Logger
                        .Error(VRTK_Logger
                            .GetCommonMessage(VRTK_Logger
                                .CommonMessageKeys
                                .REQUIRED_COMPONENT_MISSING_FROM_SCENE,
                            "[VRSimulator_CameraRig]",
                            "SDK_InputSimulator",
                            ". check that the `VRTK/Prefabs/CameraRigs/[VRSimulator_CameraRig]` prefab been added to the scene."));
                }
            }
            return cachedCameraRig;
        }

        protected virtual void Awake()
        {
            VRTK_SDKManager
                .AttemptAddBehaviourToToggleOnLoadedSetupChange(this);
        }

        protected virtual void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            hintCanvas = transform.Find("Canvas/Control Hints").gameObject;
            crossHairPanel = transform.Find("Canvas/CrosshairPanel").gameObject;
            hintText = hintCanvas.GetComponentInChildren<Text>();
            hintCanvas.SetActive (showControlHints);
            rightHand = transform.Find("RightHand");
            rightHand.gameObject.SetActive(false);
            leftHand = transform.Find("LeftHand");
            leftHand.gameObject.SetActive(false);
            leftHandHorizontalAxisGuide =
                leftHand.Find("Guides/HorizontalPlane");
            leftHandVerticalAxisGuide = leftHand.Find("Guides/VerticalPlane");
            rightHandHorizontalAxisGuide =
                rightHand.Find("Guides/HorizontalPlane");
            rightHandVerticalAxisGuide = rightHand.Find("Guides/VerticalPlane");
            currentHand = rightHand;
            oldPos = Input.mousePosition;
            neck = transform.Find("Neck");
            SetHandColor (leftHand, leftHandColor);
            SetHandColor (rightHand, rightHandColor);
            rightController = rightHand.GetComponent<SDK_ControllerSim>();
            leftController = leftHand.GetComponent<SDK_ControllerSim>();
            rightController.selected = true;
            leftController.selected = false;
            destroyed = false;

            SDK_SimController controllerSDK =
                VRTK_SDK_Bridge.GetControllerSDK() as SDK_SimController;
            if (controllerSDK != null)
            {
                Dictionary<string, KeyCode> keyMappings =
                    new Dictionary<string, KeyCode>()
                    {
                        { "Trigger", triggerAlias },
                        { "Grip", gripAlias },
                        { "TouchpadPress", touchpadAlias },
                        { "ButtonOne", buttonOneAlias },
                        { "ButtonTwo", buttonTwoAlias },
                        { "StartMenu", startMenuAlias },
                        { "TouchModifier", touchModifier },
                        { "HairTouchModifier", hairTouchModifier }
                    };
                controllerSDK.SetKeyMappings (keyMappings);
            }
            rightHand.gameObject.SetActive(true);
            leftHand.gameObject.SetActive(true);
            crossHairPanel.SetActive(false);

            neckPosition = neck.transform.localPosition;
            rightHandPosition = rightHand.transform.localPosition;
            leftHandPosition = leftHand.transform.localPosition;

            _audioSource.clip = walkSound;
            _audioSource.loop = true;
        }

        protected virtual void OnDestroy()
        {
            VRTK_SDKManager
                .AttemptRemoveBehaviourToToggleOnLoadedSetupChange(this);
            destroyed = true;
        }

        void FixedUpdate()
        {
            if (Input.GetKeyDown(toggleControlHints))
            {
                showControlHints = !showControlHints;
                hintCanvas.SetActive (showControlHints);
            }

            if (Input.GetKeyDown(toggleMouseLock))
            {
                lockMouseToView = !lockMouseToView;
            }

            if (mouseMovementInput == MouseInputMode.RequiresButtonPress)
            {
                if (lockMouseToView)
                {
                    Cursor.lockState =
                        Input.GetKey(mouseMovementKey)
                            ? CursorLockMode.Locked
                            : CursorLockMode.None;
                }
                else if (Input.GetKeyDown(mouseMovementKey))
                {
                    oldPos = Input.mousePosition;
                }
            }
            else
            {
                Cursor.lockState =
                    lockMouseToView
                        ? CursorLockMode.Locked
                        : CursorLockMode.None;
            }

            if (Input.GetKeyDown(handsOnOff))
            {
                if (isHand)
                {
                    SetMove();
                    ToggleGuidePlanes(false, false);
                }
                else
                {
                    SetHand();
                }
            }

            if (Input.GetKeyDown(changeHands))
            {
                if (currentHand.name == "LeftHand")
                {
                    currentHand = rightHand;
                    rightController.selected = true;
                    leftController.selected = false;
                }
                else
                {
                    currentHand = leftHand;
                    rightController.selected = false;
                    leftController.selected = true;
                }
            }

            if (isHand)
            {
                UpdateHands();
            }
            else
            {
                UpdateRotation();
                if (
                    Input.GetKeyDown(distancePickupRight) &&
                    Input.GetKey(distancePickupModifier)
                )
                {
                    TryPickup(true);
                }
                else if (
                    Input.GetKeyDown(distancePickupLeft) &&
                    Input.GetKey(distancePickupModifier)
                )
                {
                    TryPickup(false);
                }
                if (Input.GetKey(sprint))
                {
                    sprintMultiplier = playerSprintMultiplier;
                }
                else
                {
                    sprintMultiplier = 1;
                }
                if (Input.GetKeyDown(distancePickupModifier))
                {
                    crossHairPanel.SetActive(true);
                }
                else if (Input.GetKeyUp(distancePickupModifier))
                {
                    crossHairPanel.SetActive(false);
                }
            }

            UpdatePosition();
            Jump();
            WalkSound();
            if (!jumpping) AdjustHeight(GetCollisionHeight());
            if (showControlHints)
            {
                UpdateHints();
            }

            // _neckRigidbody.freezeRotation = true;
        }

        protected virtual void SetHandColor(Transform hand, Color givenColor)
        {
            Transform foundHand = hand.Find("Hand");
            if (foundHand != null && givenColor != Color.clear)
            {
                Renderer[] renderers =
                    foundHand.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].material.color = givenColor;
                }
            }
        }

        protected virtual void TryPickup(bool rightHand)
        {
            Ray screenRay =
                Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            if (Physics.Raycast(screenRay, out hit))
            {
                VRTK_InteractableObject io =
                    hit
                        .collider
                        .gameObject
                        .GetComponent<VRTK_InteractableObject>();
                if (io != null)
                {
                    GameObject hand;
                    if (rightHand)
                    {
                        hand = VRTK_DeviceFinder.GetControllerRightHand();
                    }
                    else
                    {
                        hand = VRTK_DeviceFinder.GetControllerLeftHand();
                    }
                    VRTK_InteractGrab grab =
                        hand.GetComponent<VRTK_InteractGrab>();
                    if (grab.GetGrabbedObject() == null)
                    {
                        hand
                            .GetComponent<VRTK_InteractTouch>()
                            .ForceTouch(hit.collider.gameObject);
                        grab.AttemptGrab();
                    }
                }
            }
        }

        protected virtual void UpdateHands()
        {
            Vector3 mouseDiff = GetMouseDelta();

            if (IsAcceptingMouseInput())
            {
                if (Input.GetKey(changeAxis))
                {
                    ToggleGuidePlanes(false, true);
                    if (Input.GetKey(rotationPosition))
                    {
                        Vector3 rot = Vector3.zero;
                        rot.x += (mouseDiff * handRotationMultiplier).y;
                        rot.y += (mouseDiff * handRotationMultiplier).x;
                        currentHand.transform.Rotate(rot * Time.deltaTime);
                    }
                    else
                    {
                        Vector3 pos = Vector3.zero;
                        pos += mouseDiff * handMoveMultiplier;
                        currentHand.transform.Translate(pos * Time.deltaTime);
                    }
                }
                else
                {
                    ToggleGuidePlanes(true, false);
                    if (Input.GetKey(rotationPosition))
                    {
                        Vector3 rot = Vector3.zero;
                        rot.z += (mouseDiff * handRotationMultiplier).x;
                        rot.x += (mouseDiff * handRotationMultiplier).y;
                        currentHand.transform.Rotate(rot * Time.deltaTime);
                    }
                    else
                    {
                        Vector3 pos = Vector3.zero;
                        pos.x += (mouseDiff * handMoveMultiplier).x;
                        pos.z += (mouseDiff * handMoveMultiplier).y;
                        currentHand.transform.Translate(pos * Time.deltaTime);
                    }
                }
                if (
                    !jumpping //防止跳跃过程更新导致更新位置时不跟随
                )
                {
                    rightHandPosition = rightHand.transform.localPosition;
                    leftHandPosition = leftHand.transform.localPosition;
                }
            }
        }

        protected virtual void UpdateRotation()
        {
            Vector3 mouseDiff = GetMouseDelta();

            if (IsAcceptingMouseInput())
            {
                Vector3 rot = transform.localRotation.eulerAngles;
                rot.y += (mouseDiff * playerRotationMultiplier).x;
                transform.localRotation = Quaternion.Euler(rot);

                rot = neck.rotation.eulerAngles;

                if (rot.x > 180)
                {
                    rot.x -= 360;
                }

                if (rot.x < 80 && rot.x > -80)
                {
                    rot.x += (mouseDiff * playerRotationMultiplier).y * -1;
                    rot.x = Mathf.Clamp(rot.x, -79, 79);
                    neck.rotation = Quaternion.Euler(rot);
                }
            }
        }

        private float prevHeight = 0f;

        private float gap = 0.3f;

        // private Vector3 currentVelocity = Vector3.zero; //当前速度，这个值每次调用SmoothDamp这个函数时被修改
        /// <summary>
        /// 悬浮高度调整
        /// </summary>
        /// <param name="height"></param>
        private void AdjustHeight(float height)
        {
            if (height > -5f)
            {
                _rigidbody.velocity -= new Vector3(0, _rigidbody.velocity.y, 0); //防止碰撞升天

                if (prevHeight != height)
                {
                    transform.position +=
                        new Vector3(0, height + gap - transform.position.y, 0);
                    prevHeight = height;
                    // Debug.Log (height);
                }

                // Vector3
                //     .SmoothDamp(transform.position,
                //     transform.position +
                //     new Vector3(0, height + 0.1f - transform.position.y),
                //     ref currentVelocity,
                //     movementSmoothness);

                // prevHeight = height;
            }
            else
            {
            }
        }

        /// return first ground collider height
        private float GetCollisionHeight()
        {
            var bounds = _collider.bounds;
            var extents = bounds.extents;
            var radius = extents.x;
            RaycastHit hit;

            // foreach (var hit in _groundCastResults)
            // {
            //     if (hit.collider != null && hit.collider != _collider)
            //     {
            //         return hit.collider.gameObject.transform.position.y;
            //     }
            // }
            // if (
            //     !Physics
            //         .Raycast(transform.position,
            //         Vector3.down,
            //         out hit,
            //         // extents.y - radius * 0.5f,
            //         10f,
            //         ~(1 << 9),
            //         QueryTriggerInteraction.Ignore)
            // )
            if (
                !Physics
                    .SphereCast(transform.position,
                    radius,
                    Vector3.down,
                    out hit,
                    10f,
                    ~(1 << 9), //camerarig放在第9层
                    QueryTriggerInteraction.Ignore)
            )
            {
                return -5f; //最低下界
            }
            return hit.collider.ClosestPoint(transform.position).y;

            // if (
            //     !_groundCastResults
            //         .Any(hit =>
            //             hit.collider != null && hit.collider != _collider)
            // )

            // for (var i = 0; i < _groundCastResults.Length; i++)
            // {
            //     _groundCastResults[i] = new RaycastHit();
            // }

            // _isGrounded = true;
        }

        private bool jumpping = false; //防止长按升天

        protected virtual void Jump()
        {
            // if (
            //     rightHandEventController.buttonOnePressed && jumpping == false //用右手buttonOne控制跳跃
            // )
            if (
                (
                rightHandEventController.buttonOnePressed ||
                Input.GetKeyDown(KeyCode.J)
                ) &&
                jumpping == false //简便起见添加J跳跃
            )
            {
                jumpping = true;

                _rigidbody.useGravity = true;

                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                Invoke("StopGravity", 1f);
            }
        }

        private void StopGravity()
        {
            jumpping = false;

            _rigidbody.useGravity = false;

            _rigidbody.velocity = Vector3.zero; //取消惯性

            // 回到地面后重新悬浮
            AdjustHeight(GetCollisionHeight());
        }

        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private AudioClip walkSound;

        [SerializeField]
        private float jumpForce;

        private void WalkSound()
        {
            var v =
                Math
                    .Sqrt(_rigidbody.velocity.x * _rigidbody.velocity.x +
                    _rigidbody.velocity.z * _rigidbody.velocity.z);

            //    _rigidbody.velocity.sqrMagnitude
            if (v > 0.1f)
            {
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }
            }
            else
            {
                if (_audioSource.isPlaying)
                {
                    _audioSource.Pause();
                }
            }
        }

        protected virtual void UpdatePosition()
        {
            float moveMod =
                Time.deltaTime * playerMoveMultiplier * sprintMultiplier;
            var direction =
                new Vector3(Input.GetAxisRaw("Horizontal"),
                    0f,
                    Input.GetAxisRaw("Vertical")).normalized;

            var worldDirection = transform.TransformDirection(direction);

            if (CheckCollisionsWithWalls(worldDirection))
            {
                _velocityX.Current = _velocityZ.Current = 0f;
                return;
            }
            var velocity = worldDirection * playerMoveMultiplier;

            var smoothX = _velocityX.Update(velocity.x, movementSmoothness);
            var smoothZ = _velocityZ.Update(velocity.z, movementSmoothness);
            var rigidbodyVelocity = _rigidbody.velocity;
            var force =
                new Vector3(smoothX - rigidbodyVelocity.x,
                    0f,
                    smoothZ - rigidbodyVelocity.z);

            _rigidbody.AddForce(force, ForceMode.VelocityChange); //给刚体施加力移动不会自动更新子组件位置
            neck.transform.position =
                transform.position + transform.TransformVector(neckPosition);
            rightHand.transform.position =
                transform.position +
                transform.TransformVector(rightHandPosition);
            leftHand.transform.position =
                transform.position +
                transform.TransformVector(leftHandPosition);

            // transform.Translate(worldDirection * moveMod, Space.World);

            // if (Input.GetKey(moveForward))
            // {
            //     transform.Translate(transform.forward * moveMod, Space.World);
            // }
            // else if (Input.GetKey(moveBackward))
            // {
            //     transform.Translate(-transform.forward * moveMod, Space.World);
            // }
            // if (Input.GetKey(moveLeft))
            // {
            //     transform.Translate(-transform.right * moveMod, Space.World);
            // }
            // else if (Input.GetKey(moveRight))
            // {
            //     transform.Translate(transform.right * moveMod, Space.World);
            // }
        }

        private bool CheckCollisionsWithWalls(Vector3 velocity)
        {
            var bounds = _collider.bounds;
            var radius = _collider.radius;
            var halfHeight = _collider.height * 0.5f - radius * 1.0f;
            var point1 = bounds.center;
            point1.y += halfHeight;
            var point2 = bounds.center;
            point2.y -= halfHeight;
            Physics
                .CapsuleCastNonAlloc(point1,
                point2,
                radius,
                velocity.normalized,
                _wallCastResults,
                radius * 0.04f,
                ~0,
                QueryTriggerInteraction.Ignore); //对场景中的所有碰撞体投掷一个胶囊，并返回有关被击中缓冲区的内容的详细信息
            var collides =
                _wallCastResults
                    .Any(hit =>
                        hit.collider != null && hit.collider != _collider);
            if (!collides) return false;
            for (var i = 0; i < _wallCastResults.Length; i++)
            {
                _wallCastResults[i] = new RaycastHit();
            }

            return true;
        }

        protected virtual void SetHand()
        {
            Cursor.visible = false;
            isHand = true;
            rightHand.gameObject.SetActive(true);
            leftHand.gameObject.SetActive(true);
            oldPos = Input.mousePosition;
            if (resetHandsAtSwitch)
            {
                rightHand.transform.localPosition =
                    new Vector3(0.2f, 1.2f, 0.5f);
                rightHand.transform.localRotation = Quaternion.identity;
                leftHand.transform.localPosition =
                    new Vector3(-0.2f, 1.2f, 0.5f);
                leftHand.transform.localRotation = Quaternion.identity;
            }
        }

        protected virtual void SetMove()
        {
            Cursor.visible = true;
            isHand = false;
            if (hideHandsAtSwitch)
            {
                rightHand.gameObject.SetActive(false);
                leftHand.gameObject.SetActive(false);
            }
        }

        protected virtual void UpdateHints()
        {
            string hints = "";
            Func<KeyCode, string> key = (k) => "<b>" + k.ToString() + "</b>";

            string mouseInputRequires = "";
            if (mouseMovementInput == MouseInputMode.RequiresButtonPress)
            {
                mouseInputRequires = " (" + key(mouseMovementKey) + ")";
            }

            // WASD Movement
            string movementKeys =
                moveForward.ToString() +
                moveLeft.ToString() +
                moveBackward.ToString() +
                moveRight.ToString();
            hints +=
                "Toggle Control Hints: " + key(toggleControlHints) + "\n\n";
            hints += "Toggle Mouse Lock: " + key(toggleMouseLock) + "\n";
            hints += "Move Player/Playspace: <b>" + movementKeys + "</b>\n";
            hints += "Sprint Modifier: (" + key(sprint) + ")\n\n";

            if (isHand)
            {
                // Controllers
                if (Input.GetKey(rotationPosition))
                {
                    hints +=
                        "Mouse: <b>Controller Rotation" +
                        mouseInputRequires +
                        "</b>\n";
                }
                else
                {
                    hints +=
                        "Mouse: <b>Controller Position" +
                        mouseInputRequires +
                        "</b>\n";
                }
                hints +=
                    "Modes: HMD (" +
                    key(handsOnOff) +
                    "), Rotation (" +
                    key(rotationPosition) +
                    ")\n";

                hints +=
                    "Controller Hand: " +
                    currentHand.name.Replace("Hand", "") +
                    " (" +
                    key(changeHands) +
                    ")\n";

                string axis = Input.GetKey(changeAxis) ? "X/Y" : "X/Z";
                hints += "Axis: " + axis + " (" + key(changeAxis) + ")\n";

                // Controller Buttons
                string pressMode = "Press";
                if (Input.GetKey(hairTouchModifier))
                {
                    pressMode = "Hair Touch";
                }
                else if (Input.GetKey(touchModifier))
                {
                    pressMode = "Touch";
                }

                hints +=
                    "\nButton Press Mode Modifiers: Touch (" +
                    key(touchModifier) +
                    "), Hair Touch (" +
                    key(hairTouchModifier) +
                    ")\n";

                hints +=
                    "Trigger " + pressMode + ": " + key(triggerAlias) + "\n";
                hints += "Grip " + pressMode + ": " + key(gripAlias) + "\n";
                if (!Input.GetKey(hairTouchModifier))
                {
                    hints +=
                        "Touchpad " +
                        pressMode +
                        ": " +
                        key(touchpadAlias) +
                        "\n";
                    hints +=
                        "Button One " +
                        pressMode +
                        ": " +
                        key(buttonOneAlias) +
                        "\n";
                    hints +=
                        "Button Two " +
                        pressMode +
                        ": " +
                        key(buttonTwoAlias) +
                        "\n";
                    hints +=
                        "Start Menu " +
                        pressMode +
                        ": " +
                        key(startMenuAlias) +
                        "\n";
                }
            }
            else
            {
                // HMD Input
                hints +=
                    "Mouse: <b>HMD Rotation" + mouseInputRequires + "</b>\n";
                hints += "Modes: Controller (" + key(handsOnOff) + ")\n";
                hints +=
                    "Distance Pickup Modifier: (" +
                    key(distancePickupModifier) +
                    ")\n";
                hints +=
                    "Distance Pickup Left Hand: (" +
                    key(distancePickupLeft) +
                    ")\n";
                hints +=
                    "Distance Pickup Right Hand: (" +
                    key(distancePickupRight) +
                    ")\n";
            }

            hintText.text = hints.TrimEnd();
        }

        protected virtual bool IsAcceptingMouseInput()
        {
            return mouseMovementInput == MouseInputMode.Always ||
            Input.GetKey(mouseMovementKey);
        }

        protected virtual Vector3 GetMouseDelta()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                return new Vector3(Input.GetAxis("Mouse X"),
                    Input.GetAxis("Mouse Y")) *
                lockedCursorMultiplier;
            }
            else
            {
                Vector3 mouseDiff = Input.mousePosition - oldPos;
                oldPos = Input.mousePosition;
                return mouseDiff;
            }
        }

        protected virtual void ToggleGuidePlanes(
            bool horizontalState,
            bool verticalState
        )
        {
            if (!showHandAxisHelpers)
            {
                horizontalState = false;
                verticalState = false;
            }

            if (leftHandHorizontalAxisGuide != null)
            {
                leftHandHorizontalAxisGuide.gameObject.SetActive (
                    horizontalState
                );
            }

            if (leftHandVerticalAxisGuide != null)
            {
                leftHandVerticalAxisGuide.gameObject.SetActive (verticalState);
            }

            if (rightHandHorizontalAxisGuide != null)
            {
                rightHandHorizontalAxisGuide.gameObject.SetActive (
                    horizontalState
                );
            }

            if (rightHandVerticalAxisGuide != null)
            {
                rightHandVerticalAxisGuide.gameObject.SetActive (verticalState);
            }
        }
    }
}
