using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RTG
{
    public delegate void CameraCanProcessInputHandler(YesNoAnswer answer);
    public delegate void CameraCanUseScrollWheelHandler(YesNoAnswer answer);

    public class RTFocusCamera : MonoSingleton<RTFocusCamera>
    {
        public event CameraProjectionSwitchBeginHandler PrjSwitchTransitionBegin;
        public event CameraProjectionSwitchUpdateHandler PrjSwitchTransitionUpdate;
        public event CameraProjectionSwitchBeginHandler PrjSwitchTransitionEnd;
        public event CameraCanProcessInputHandler CanProcessInput;
        public event CameraCanUseScrollWheelHandler CanUseScrollWheel;

        private enum MoveDirection
        {
            Left = 0,
            Right,
            Up,
            Down,
            Forward,
            Backwards
        }

        [SerializeField]
        private Camera _targetCamera;
        private Transform _targetTransform;

        [SerializeField]
        private float _fieldOfView;

        private WorldTransformSnapshot _worldTransformSnapshot = new WorldTransformSnapshot();
        private CameraPrjSwitchTransition _prjSwitchTranstion = new CameraPrjSwitchTransition();
        private bool _isDoingFocus;
        private IEnumerator _focusCrtn;
        private bool _isDoingRotationSwitch;
        private IEnumerator _genricCamTransformCrtn;

        private bool _isObjectVisibilityDirty = true;
        private List<GameObject> _visibleObjects = new List<GameObject>();
        private float _focusPointOffset = 5.0f;
        private Vector3 _lastFocusPoint;

        private bool[] _moveDirFlags = new bool[Enum.GetValues(typeof(MoveDirection)).Length];
        private float _currentAcceleration = 0.0f;

        [SerializeField]
        private CameraSettings _settings = new CameraSettings();
        [SerializeField]
        private CameraMoveSettings _moveSettings = new CameraMoveSettings();
        [SerializeField]
        private CameraPanSettings _panSettings = new CameraPanSettings();
        [SerializeField]
        private CameraLookAroundSettings _lookAroundSettings = new CameraLookAroundSettings();
        [SerializeField]
        private CameraOrbitSettings _orbitSettings = new CameraOrbitSettings();
        [SerializeField]
        private CameraZoomSettings _zoomSettings = new CameraZoomSettings();
        [SerializeField]
        private CameraFocusSettings _focusSettings = new CameraFocusSettings();
        [SerializeField]
        private CameraRotationSwitchSettings _rotationSwitchSettings = new CameraRotationSwitchSettings();
        [SerializeField]
        private CameraProjectionSwitchSettings _projectionSwitchSettings = new CameraProjectionSwitchSettings();
        [SerializeField]
        private CameraHotkeys _hotkeys = new CameraHotkeys();

        public Camera TargetCamera { get { return _targetCamera; } }
        public bool IsDoingProjectionSwitch { get { return _prjSwitchTranstion.IsActive; } }
        public CameraPrjSwitchTransition.Type PrjSwitchTransitionType { get { return _prjSwitchTranstion.TransitionType; } }
        public bool IsDoingRotationSwitch { get { return _isDoingRotationSwitch; } }
        public float PrjSwitchProgress { get { return _prjSwitchTranstion.Progress; } }
        public float PrjSwitchDurationInSeconds { get { return _projectionSwitchSettings.TransitionDurationInSeconds; } }
        public bool IsDoingFocus { get { return _isDoingFocus; } }
        public Vector3 WorldPosition 
        { 
            get { return _targetTransform.position; }
            set
            {
                Vector3 focusPt = GetFocusPoint();
                _targetTransform.position = value;
                SetFocusPoint(focusPt);
            }
        }
        public Quaternion WorldRotation 
        { 
            get { return _targetTransform.rotation; } 
            set
            {
                _targetTransform.rotation = value;
            }
        }
        public Vector3 Right { get { return _targetTransform.right; } }
        public Vector3 Up { get { return _targetTransform.up; } }
        public Vector3 Look { get { return _targetTransform.forward; } }
        public bool IsMovingForward { get { return _moveDirFlags[(int)MoveDirection.Forward]; } }
        public bool IsMovingBackwards { get { return _moveDirFlags[(int)MoveDirection.Backwards]; } }
        public bool IsMovingLeft { get { return _moveDirFlags[(int)MoveDirection.Left]; } }
        public bool IsMovingRight { get { return _moveDirFlags[(int)MoveDirection.Right]; } }
        public bool IsMovingUp { get { return _moveDirFlags[(int)MoveDirection.Up]; } }
        public bool IsMovingDown { get { return _moveDirFlags[(int)MoveDirection.Down]; } }

        public CameraSettings Settings { get { return _settings; } }
        public CameraMoveSettings MoveSettings { get { return _moveSettings; } }
        public CameraPanSettings PanSettings { get { return _panSettings; } }
        public CameraLookAroundSettings LookAroundSettings { get { return _lookAroundSettings; } }
        public CameraOrbitSettings OrbitSettings { get { return _orbitSettings; } }
        public CameraZoomSettings ZoomSettings { get { return _zoomSettings; } }
        public CameraFocusSettings FocusSettings { get { return _focusSettings; } }
        public CameraRotationSwitchSettings RotationSwitchSettings { get { return _rotationSwitchSettings; } }
        public CameraProjectionSwitchSettings ProjectionSwitchSettings { get { return _projectionSwitchSettings; } }
        public CameraHotkeys Hotkeys { get { return _hotkeys; } }

        public bool Orthgraphic
        {
            get
            {
                return _targetCamera.orthographic;
            }
            set
            {

                if (value == false)
                {
                    _targetCamera.orthographic = value;
                    return;
                }

                if (!_targetCamera.orthographic)
                {
                    var rot = _targetCamera.transform.rotation;
                    rot.eulerAngles = new Vector3(90f, rot.eulerAngles.y, rot.eulerAngles.z);
                    _targetCamera.transform.rotation = rot;
                }
                _targetCamera.orthographic = value;
            }
        }

        public bool IsViewportHoveredByDevice()
        {
            Vector2 devicePos = RTInputDevice.Get.Device.GetPositionYAxisUp();
            Vector3 viewportPoint = TargetCamera.ScreenToViewportPoint(devicePos);
            return viewportPoint.x >= 0.0f && viewportPoint.x <= 1.0f &&
                   viewportPoint.y >= 0.0f && viewportPoint.y <= 1.0f;
        }

        public void SetTargetCamera(Camera camera)
        {
            if (camera == null)
                return;

            if (Application.isPlaying)
            {
                if (IsDoingFocus)
                    return;

                if (IsDoingProjectionSwitch)
                    return;

                if (IsDoingRotationSwitch)
                    return;

                _targetCamera = camera;
                _targetTransform = camera.transform;
                _fieldOfView = camera.fieldOfView;

                SetFocusPoint(GetFocusPoint());
                AdjustOrthoSizeForFocusPt();

                _isObjectVisibilityDirty = true;
                return;
            }

            if (!GameObjectEx.IsSceneObject(camera.gameObject))
                return;
         
            _targetCamera = camera;
            _fieldOfView = camera.fieldOfView;
        }

        public void SetFieldOfView(float fov)
        {
            _targetCamera.fieldOfView = fov;
            _fieldOfView = fov;
        }

        public void SetObjectVisibilityDirty()
        {
            _isObjectVisibilityDirty = true;
        }

        public void GetVisibleObjects(List<GameObject> visibleObjects)
        {
            visibleObjects.Clear();

            if (_isObjectVisibilityDirty)
            {
                TargetCamera.GetVisibleObjects(new CameraViewVolume(TargetCamera), _visibleObjects);
                _isObjectVisibilityDirty = false;
            }

            if (_visibleObjects.Count != 0) 
                visibleObjects.AddRange(_visibleObjects);
        }

        public void PerformRotationSwitch(Quaternion targetRotation)
        {
            if (IsDoingProjectionSwitch)
                return;

            StopCamTransform();
            StopFocus();

            if (RotationSwitchSettings.SwitchMode == CameraRotationSwitchMode.Constant)
            {
                StartCoroutine(_genricCamTransformCrtn = DoConstantRotationSwitch(targetRotation));
                return;
            }
            
            if (RotationSwitchSettings.SwitchMode == CameraRotationSwitchMode.Smooth)
            {
                StartCoroutine(_genricCamTransformCrtn = DoSmoothRotationSwitch(targetRotation));
                return;
            }
            
            if (RotationSwitchSettings.SwitchType == CameraRotationSwitchType.InPlace)
            {
                _targetTransform.rotation = targetRotation;
                return;
            }
            
            Vector3 focusPt = GetFocusPoint();
            _targetTransform.rotation = targetRotation;
            _targetTransform.position = focusPt - _targetTransform.forward * _focusPointOffset;
        }

        public void PerformProjectionSwitch()
        {
            StopCamTransform();
            StopFocus();

            if (ProjectionSwitchSettings.SwitchMode == CameraProjectionSwitchMode.Transition)
            {
                _prjSwitchTranstion.TargetCamera = _targetCamera;
                _prjSwitchTranstion.CamFieldOfView = _fieldOfView;
                _prjSwitchTranstion.CamFocusPoint = GetFocusPoint();
                _prjSwitchTranstion.DurationInSeconds = ProjectionSwitchSettings.TransitionDurationInSeconds;
                _prjSwitchTranstion.Begin();
            }
            else 
                PerformInstantProjectionSwitch();
        }

        public void Focus(List<GameObject> gameObjects)
        {
            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.NoVolumeSize = Vector3.one * 0.01f;
            boundsQConfig.ObjectTypes = GameObjectType.Mesh | GameObjectType.Sprite | GameObjectType.Terrain;

            AABB focusAABB = ObjectBounds.CalcObjectCollectionWorldAABB(gameObjects, boundsQConfig);
            if (focusAABB.IsValid) 
                Focus(focusAABB);

//             var parents = GameObjectEx.FilterParentsOnly(gameObjects);
//             if (parents.Count != 0)
//             {
//                 AABB focusAABB = ObjectBounds.CalcHierarchyCollectionWorldAABB(parents, boundsQConfig);
//                 if (focusAABB.IsValid) Focus(focusAABB);
//             }       
        }

        public void Focus(AABB focusAABB)
        {
            if (_isDoingFocus)
                return;

            if (IsDoingProjectionSwitch)
                return;

            if (IsDoingRotationSwitch)
                return;

            if (!focusAABB.IsValid)
                return;

            StopCamTransform();

            CameraFocus.Data focusData = CameraFocus.CalculateFocusData(TargetCamera, focusAABB, FocusSettings);

            switch (FocusSettings.FocusMode)
            {
                case CameraFocusMode.Instant:
                    PerformInstantFocus(focusData);
                    break;
                case CameraFocusMode.Constant:
                    StartCoroutine(_focusCrtn = DoConstantFocus(focusData));
                    break;
                case CameraFocusMode.Smooth:
                    StartCoroutine(_focusCrtn = DoSmoothFocus(focusData));
                    break;
            }
        }

        public void Update_SystemCall()
        {
            HandleMouseAndKeyboardInput();

            if (_worldTransformSnapshot.SameAs(_targetTransform))
                return;
            SetObjectVisibilityDirty();
            _worldTransformSnapshot.Snaphot(_targetTransform);
        }

        private void Awake()
        {
            SetTargetCamera(Camera.main) ;
            _worldTransformSnapshot.Snaphot(_targetTransform);

            _prjSwitchTranstion.TargetMono = this;
            _prjSwitchTranstion.TransitionBegin += OnPrjSwitchTransitionBegin;
            _prjSwitchTranstion.TransitionUpdate += OnPrjSwitchTransitionUpate;
            _prjSwitchTranstion.TransitionEnd += OnPrjSwitchTransitionEnd;
        }

        private void Start()
        {
            _lastFocusPoint = Vector3.zero;
            SetFocusPoint(_lastFocusPoint);
            AdjustOrthoSizeForFocusPt(); 
        }

        private void HandleMouseAndKeyboardInput()
        {
            if (!CanCameraProcessInput())
                return;

            if (RTInputDevice.Get.DeviceType != InputDeviceType.Mouse)
                return;

            GetInput();

            Movement();
            Scroll();
        }
        
        void GetInput()
        {
            float moveAmount = (_moveSettings.MoveSpeed + _currentAcceleration) * Time.deltaTime;

            Vector3 moveVector = Vector3.zero;
            _moveDirFlags[(int)MoveDirection.Forward] = Hotkeys.MoveForward.IsActive();
            _moveDirFlags[(int)MoveDirection.Backwards] = !_moveDirFlags[(int)MoveDirection.Forward] && Hotkeys.MoveBack.IsActive();
            _moveDirFlags[(int)MoveDirection.Left] = Hotkeys.StrafeLeft.IsActive();
            _moveDirFlags[(int)MoveDirection.Right] = !_moveDirFlags[(int)MoveDirection.Left] && Hotkeys.StrafeRight.IsActive();
            _moveDirFlags[(int)MoveDirection.Up] = Hotkeys.MoveUp.IsActive();
            _moveDirFlags[(int)MoveDirection.Down] = !_moveDirFlags[(int)MoveDirection.Up] && Hotkeys.MoveDown.IsActive();

            bool wasZoomed = false;
            if (IsMovingForward)
            {
                Zoom(moveAmount);
                wasZoomed = true;
            }
            else if (IsMovingBackwards)
            {
                Zoom(-moveAmount);
                wasZoomed = true;
            }

            if (IsMovingLeft)
            {
                moveVector -= _targetTransform.right * moveAmount;
            }
            else if (IsMovingRight)
            {
                moveVector += _targetTransform.right * moveAmount;
            }
            if (IsMovingUp)
            {
                moveVector += _targetTransform.up * moveAmount;
            }
            else if (IsMovingDown)
            {
                moveVector -= _targetTransform.up * moveAmount;
            }

            bool needsToMove = moveVector.sqrMagnitude != 0.0f;
            if (needsToMove)
                _targetTransform.position += moveVector;

            if (needsToMove || wasZoomed)
            {
                float accelAdd = MoveSettings.AccelerationRate * Mathf.Abs(_targetCamera.EstimateZoomFactor(_lastFocusPoint)) * Time.deltaTime;
                _currentAcceleration += accelAdd;
            }
            else
            {
                _currentAcceleration = 0.0f;
            }
        }

        void Movement()
        {
            // Get the mouse axes values. We need these for panning and rotation.
            float mouseX = RTInput.MouseAxisX();
            float mouseY = RTInput.MouseAxisY();

            // Only proceed if at least one mouse axis value is != 0
            if (Mathf.Approximately(mouseX, 0f) && Mathf.Approximately(mouseY, 0f))
                return;

            if (_panSettings.IsPanningEnabled && Hotkeys.Pan.IsActive())
            {
                if (_panSettings.PanMode == CameraPanMode.Standard)
                {
                    Pan(CalculatePanAmount(mouseX, mouseY));
                }
                else
                {
                    StopCamTransform();
                    StartCoroutine(_genricCamTransformCrtn = DoSmoothPan(mouseX, mouseY));
                }
            }

            if (_orbitSettings.IsOrbitEnabled && Hotkeys.Orbit.IsActive())
            {
                if (_orbitSettings.OrbitMode == CameraOrbitMode.Standard)
                {
                    Vector2 rotation = CalculateOrbitRotation(mouseX, mouseY);
                    Orbit(rotation.x, rotation.y);
                }
                else
                {
                    StopCamTransform();
                    StartCoroutine(_genricCamTransformCrtn = DoSmoothOrbit(mouseX, mouseY));
                }
            }
            else if (_lookAroundSettings.IsLookAroundEnabled && Hotkeys.LookAround.IsActive())
            {
                if (Orthgraphic)
                    mouseY = 0f;

                if (_lookAroundSettings.LookAroundMode == CameraLookAroundMode.Standard)
                {
                    Vector2 rotation = CalculateLookAroundRotation(mouseX, mouseY);
                    LookAround(rotation.x, rotation.y);
                }
                else
                {
                    StopCamTransform();
                    StartCoroutine(_genricCamTransformCrtn = DoSmoothLookAround(mouseX, mouseY));
                }
            }
        }
        void Scroll()
        {
            if (!CanUseMouseScrollWheel())
                return;

            if (!_zoomSettings.IsZoomEnabled)
                return;

            float mouseScroll = RTInput.MouseScroll();
            if (Mathf.Approximately(mouseScroll, 0.0f))
                return;

            if (_zoomSettings.ZoomMode == CameraZoomMode.Standard)
            {
                Zoom(CalculateScrollZoomAmount(mouseScroll));
                //마우스 포인터위치로 
            }
            else
            {
                StopCamTransform();
                StartCoroutine(_genricCamTransformCrtn = DoSmoothZoom(mouseScroll));
            }
        }

        public void SliderZoomInOut(int value)
        {
            if (Orthgraphic)
            {
                var test = 910 - (225 * value);
                TargetCamera.orthographicSize = test;
            }
        }

        public void PerspectiveZoomIn(int value)
        {
            var test = 225 * value;
            var p = _targetTransform.position + _targetTransform.forward * test;
            Debug.Log(_targetTransform.forward * test);
            _targetTransform.position = p;
        }

        public void PerspectiveZoomOut(int value)
        {
            var test = 225 * value;
            var p = _targetTransform.position - _targetTransform.forward * test;
            _targetTransform.position = p;
        }

        private bool CanUseMouseScrollWheel()
        {
            if (CanUseScrollWheel == null) 
                return true;

            YesNoAnswer answer = new YesNoAnswer();
            CanUseScrollWheel(answer);
            return answer.HasOnlyYes;
        }

        private bool CanCameraProcessInput()
        {
            if (!_settings.CanProcessInput)
                return false; 
            if(_isDoingFocus)
                return false;
            if(IsDoingProjectionSwitch)
                return false;
            if (_isDoingRotationSwitch)
                return false;

            if (CanProcessInput == null) 
                return true;

            YesNoAnswer answer = new YesNoAnswer();
            CanProcessInput(answer);
            return answer.HasOnlyYes;
        }

        private void Zoom(float zoomAmount)
        {
            Vector3 focusPoint = GetFocusPoint();

            if (!Orthgraphic)
                _targetTransform.position += _targetTransform.forward * zoomAmount;
            //if (TargetCamera.orthographic)
            //{
            //    Vector3 toFocusPt = focusPoint - _targetTransform.position;
            //    if (Vector3.Dot(toFocusPt, _targetTransform.forward) < 1e-2f)
            //    {
            //        _targetTransform.position = focusPoint - _targetTransform.forward * 1e-3f;
            //    }
            //}
            
            SetFocusPoint(focusPoint);
            AdjustOrthoSizeForFocusPt(zoomAmount);
        }


        private Vector3 GetFocusPoint()
        {
            return _targetTransform.position + _targetTransform.forward * _focusPointOffset;
        }

        private float CalculateScrollZoomAmount(float deviceScroll)
        {
            float zoomAmount = deviceScroll * _zoomSettings.GetZoomSensitivity(TargetCamera);
            
            if (_zoomSettings.InvertZoomAxis) 
                zoomAmount *= -1.0f;

            zoomAmount *= _targetCamera.EstimateZoomFactorSpherical(_lastFocusPoint);

            return zoomAmount;
        }

        private void Pan(Vector2 panAmount)
        {
            _targetTransform.position += _targetTransform.right * panAmount.x + _targetTransform.up * panAmount.y;
        }

        public void LookAround(float degreesLocalX, float degreesWorldY)
        {
            _targetTransform.Rotate(Vector3.up, degreesWorldY, Space.World);
            _targetTransform.Rotate(_targetTransform.right, degreesLocalX, Space.World);
        }

        private void Orbit(float degreesLocalX, float degreesWorldY)
        {
            Vector3 orbitPoint = _targetTransform.position + _targetTransform.forward * _focusPointOffset;

            _targetTransform.RotateAround(orbitPoint, Vector3.up, degreesWorldY);
            _targetTransform.RotateAround(orbitPoint, _targetTransform.right, degreesLocalX);
            _targetTransform.LookAt(orbitPoint, _targetTransform.up);
        }

        private void PerformInstantFocus(CameraFocus.Data focusData)
        {
            _targetTransform.position = focusData.CameraWorldPosition;
            SetFocusPoint(focusData.FocusPoint);
            _lastFocusPoint = focusData.FocusPoint;

            AdjustOrthoSizeForFocusPt();
        }

        private void PerformInstantProjectionSwitch()
        {
            TargetCamera.orthographic = !TargetCamera.orthographic;
        }

        private Vector2 CalculateLookAroundRotation(float deviceAxisX, float deviceAxisY)
        {
            Vector2 rotation = Vector2.zero;
            
            rotation.x = -deviceAxisY * _lookAroundSettings.Sensitivity;
            if (_lookAroundSettings.InvertY) 
                rotation.x *= -1.0f;       
            
            rotation.y = deviceAxisX * _lookAroundSettings.Sensitivity;
            if (_lookAroundSettings.InvertX) 
                rotation.y *= -1.0f;       

            return rotation;
        }

        private Vector2 CalculateOrbitRotation(float deviceAxisX, float deviceAxisY)
        {
            Vector2 rotation = Vector2.zero;
            
            rotation.x = -deviceAxisY * _orbitSettings.OrbitSensitivity;
            if (_orbitSettings.InvertY) 
                rotation.x *= -1.0f;       
            
            rotation.y = deviceAxisX * _orbitSettings.OrbitSensitivity;
            if (_orbitSettings.InvertX) 
                rotation.y *= -1.0f;      

            return rotation;
        }

        private Vector2 CalculatePanAmount(float deviceAxisX, float deviceAxisY)
        {
            Vector2 panAmount = Vector2.zero;
            
            panAmount.x = -deviceAxisX * _panSettings.Sensitivity;
            if (_panSettings.InvertX) 
                panAmount.x *= -1.0f;         
            
            panAmount.y = -deviceAxisY * _panSettings.Sensitivity;
            if (_panSettings.InvertY) 
                panAmount.y *= -1.0f;

            panAmount *= Mathf.Abs(_targetCamera.EstimateZoomFactorSpherical(_lastFocusPoint));

            return panAmount;
        }

        private void StopCamTransform()
        {
            if (_genricCamTransformCrtn == null)
                return;
         
            StopCoroutine(_genricCamTransformCrtn);
            _genricCamTransformCrtn = null;
        }

        private void StopFocus()
        {
            if (_focusCrtn == null)
                return;
            StopCoroutine(_focusCrtn);
            _focusCrtn = null;
        }

        private void SetFocusPoint(Vector3 focusPoint)
        {
            _focusPointOffset = (focusPoint - _targetTransform.position).magnitude;
        }

        private void AdjustOrthoSizeForFocusPt(float amount)
        {
            TargetCamera.orthographicSize -= amount;
        }
        private void AdjustOrthoSizeForFocusPt()
        {
            TargetCamera.orthographicSize = Mathf.Max(0.5f * TargetCamera.GetFrustumHeightFromDistance(_focusPointOffset), 1e-4f);
        }

        private IEnumerator DoSmoothPan(float deviceAxisX, float deviceAxisY)
        {
            Vector2 panAmount = CalculatePanAmount(deviceAxisX, deviceAxisY);

            while(!Mathf.Approximately(panAmount.sqrMagnitude, 0.0f))
            {
                Pan(panAmount);
                panAmount = Vector2.Lerp(panAmount, Vector2.zero, _panSettings.SmoothValue * Time.deltaTime);

                yield return null;
            }
        }

        private IEnumerator DoSmoothLookAround(float deviceAxisX, float deviceAxisY)
        {
            Vector2 rotationAmount = CalculateLookAroundRotation(deviceAxisX, deviceAxisY);

            while (!Mathf.Approximately(rotationAmount.sqrMagnitude, 0.0f))
            {
                LookAround(rotationAmount.x, rotationAmount.y);
                rotationAmount = Vector2.Lerp(rotationAmount, Vector2.zero, _lookAroundSettings.SmoothValue * Time.deltaTime);

                yield return null;
            }
        }

        private IEnumerator DoSmoothOrbit(float deviceAxisX, float deviceAxisY)
        {
            Vector2 rotationAmount = CalculateOrbitRotation(deviceAxisX, deviceAxisY);

            while (!Mathf.Approximately(rotationAmount.sqrMagnitude, 0.0f))
            {
                Orbit(rotationAmount.x, rotationAmount.y);
                rotationAmount = Vector2.Lerp(rotationAmount, Vector2.zero, _orbitSettings.SmoothValue * Time.deltaTime);

                yield return null;
            }
        }

        private IEnumerator DoSmoothZoom(float deviceScroll)
        {
            float zoomAmount = CalculateScrollZoomAmount(deviceScroll);

            while(!Mathf.Approximately(zoomAmount, 0.0f))
            {
                Zoom(zoomAmount);
                zoomAmount = Mathf.Lerp(zoomAmount, 0.0f, _zoomSettings.GetZoomSmoothValue(TargetCamera) * Time.deltaTime);

                yield return null;
            }
        }

        private IEnumerator DoConstantRotationSwitch(Quaternion targetRotation)
        {
            Quaternion sourceRotation = _targetTransform.rotation;
            float elapsedTime = 0.0f;

            _isDoingRotationSwitch = true;
            if (RotationSwitchSettings.SwitchType == CameraRotationSwitchType.InPlace)
            {
                while (Mathf.Abs(Quaternion.Angle(_targetTransform.rotation, targetRotation)) >= 1e-4f)
                {
                    _targetTransform.rotation = Quaternion.Slerp(sourceRotation, targetRotation, elapsedTime / RotationSwitchSettings.ConstantSwitchDurationInSeconds);
                    elapsedTime += Time.deltaTime;

                    yield return null;
                }
            }
            else
            {
                Vector3 focusPt = GetFocusPoint();
                while (Mathf.Abs(Quaternion.Angle(_targetTransform.rotation, targetRotation)) >= 1e-4f)
                {
                    float t = elapsedTime / RotationSwitchSettings.ConstantSwitchDurationInSeconds;
                    _targetTransform.rotation = Quaternion.Slerp(sourceRotation, targetRotation, t);
                    _targetTransform.position = focusPt - _targetTransform.forward * _focusPointOffset;
                    elapsedTime += Time.deltaTime;

                    yield return null;
                }
            }
            _targetTransform.rotation = targetRotation;
            _isDoingRotationSwitch = false;
        }

        private IEnumerator DoSmoothRotationSwitch(Quaternion targetRotation)
        {
            _isDoingRotationSwitch = true;
            if (RotationSwitchSettings.SwitchType == CameraRotationSwitchType.InPlace)
            {
                while (Mathf.Abs(Quaternion.Angle(_targetTransform.rotation, targetRotation)) >= 1e-4f)
                {
                    _targetTransform.rotation = Quaternion.Slerp(_targetTransform.rotation, targetRotation, Time.deltaTime * RotationSwitchSettings.SmoothValue);
                    yield return null;
                }
            }
            else
            {
                Vector3 focusPt = GetFocusPoint();
                while (Mathf.Abs(Quaternion.Angle(_targetTransform.rotation, targetRotation)) >= 1e-4f)
                {
                    float t = Time.deltaTime * RotationSwitchSettings.SmoothValue;
                    _targetTransform.rotation = Quaternion.Slerp(_targetTransform.rotation, targetRotation, t);
                    _targetTransform.position = focusPt - _targetTransform.forward * _focusPointOffset;

                    yield return null;
                }
            }
            _targetTransform.rotation = targetRotation;
            _isDoingRotationSwitch = false;
        }

        private IEnumerator DoConstantFocus(CameraFocus.Data focusData)
        {
            float targetOrthoSize = 0.5f * TargetCamera.GetFrustumHeightFromDistance(focusData.FocusPointOffset);

            Vector3 camStartPos = _targetTransform.position;
            Vector3 camMoveDir = Vector3.Normalize(focusData.CameraWorldPosition - camStartPos);
            float distanceToTravel = (camStartPos - focusData.CameraWorldPosition).magnitude;
            float initialCamOrthoSize = TargetCamera.orthographicSize;

            _isDoingFocus = true;
            while(Vector3.Dot(camMoveDir, focusData.CameraWorldPosition - _targetTransform.position) > 0.0f)
            {
                _targetTransform.position += camMoveDir * FocusSettings.ConstantSpeed * Time.deltaTime;
                float t = 1.0f - (_targetTransform.position - focusData.CameraWorldPosition).magnitude / distanceToTravel;
                TargetCamera.orthographicSize = Mathf.Lerp(initialCamOrthoSize, targetOrthoSize, t);

                yield return null;
            }

            _targetTransform.position = focusData.CameraWorldPosition;
            TargetCamera.orthographicSize = targetOrthoSize;
            SetFocusPoint(focusData.FocusPoint);
            _lastFocusPoint = focusData.FocusPoint;
            _isDoingFocus = false;
        }

        private IEnumerator DoSmoothFocus(CameraFocus.Data focusData)
        {
            float targetOrthoSize = 0.5f * TargetCamera.GetFrustumHeightFromDistance(focusData.FocusPointOffset);

            Vector3 camStartPos = _targetTransform.position;
            Vector3 camMoveDir = Vector3.Normalize(focusData.CameraWorldPosition - camStartPos);
            float elapsedTime = 0.0f;

            _isDoingFocus = true;
            while (Vector3.Dot(camMoveDir, focusData.CameraWorldPosition - _targetTransform.position) > 0.0f)
            {
                float t = elapsedTime / FocusSettings.SmoothTime;
                _targetTransform.position = Vector3.Lerp(_targetTransform.position, focusData.CameraWorldPosition, t);
                TargetCamera.orthographicSize = Mathf.Lerp(TargetCamera.orthographicSize, targetOrthoSize, t);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            _targetTransform.position = focusData.CameraWorldPosition;
            TargetCamera.orthographicSize = targetOrthoSize;
            SetFocusPoint(focusData.FocusPoint);
            _lastFocusPoint = focusData.FocusPoint;
            _isDoingFocus = false;
        }

        private void OnPrjSwitchTransitionBegin(CameraPrjSwitchTransition.Type transitionType)
        {
            PrjSwitchTransitionBegin?.Invoke(transitionType);
        }

        private void OnPrjSwitchTransitionUpate(CameraPrjSwitchTransition.Type transitionType)
        {
            PrjSwitchTransitionUpdate?.Invoke(transitionType);
        }

        private void OnPrjSwitchTransitionEnd(CameraPrjSwitchTransition.Type transitionType)
        {
            PrjSwitchTransitionEnd?.Invoke(transitionType);
        }
    }
}
