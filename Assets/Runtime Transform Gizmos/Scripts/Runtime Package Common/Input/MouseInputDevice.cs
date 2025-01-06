using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RTG
{
    public class MouseInputDevice : InputDeviceBase
    {
        private Vector3 _frameDelta;
        private Vector3 _mousePosInLastFrame;

        public override InputDeviceType DeviceType { get { return InputDeviceType.Mouse; } }

        public MouseInputDevice()
        {
            _frameDelta = Vector3.zero;
            _mousePosInLastFrame = RTInput.mousePosition;
        }

        public override Vector3 GetFrameDelta()
        {
            return _frameDelta;
        }

        public override Ray GetRay(Camera camera)
        {
            return camera.ScreenPointToRay(RTInput.mousePosition);
        }

        public override Vector3 GetPositionYAxisUp()
        {
            return RTInput.mousePosition;
        }

        public override bool HasPointer()
        {
            return RTInput.mousePresent;
        }

        public override bool IsButtonPressed(int buttonIndex)
        {
            return Input.GetMouseButton(buttonIndex);
        }

        public override bool WasButtonPressedInCurrentFrame(int buttonIndex)
        {
            return RTInput.GetMouseButtonDown(buttonIndex);
        }

        public override bool WasButtonReleasedInCurrentFrame(int buttonIndex)
        {
            return Input.GetMouseButtonUp(buttonIndex);
        }


        public override bool WasMoved()
        {
            return RTInput.WasMouseMoved();
        }

        protected override void UpateFrameDeltas()
        {
            _frameDelta = RTInput.mousePosition - _mousePosInLastFrame;
            _mousePosInLastFrame = RTInput.mousePosition;
        }
    }
}
