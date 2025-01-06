using UnityEngine;
using UnityEngine.UIElements;


namespace RTG
{
    public static class RTInput
    {
        public static Vector3 mousePosition 
        { 
            get 
            {
                return Input.mousePosition;
            }
        }

        public static bool mousePresent
        {
            get
            {
                return Input.mousePresent;
            }
        }

        public static int touchCount
        {
            get
            {
                return Input.touchCount;
            }
        }

        public static bool GetMouseButtonDown(int i)
        {
            return Input.GetMouseButtonDown(i);
        }

        public static bool WasMouseMoved()
        {
            return Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f;
        }

        public static float MouseAxisX()
        {
            return Input.GetAxis("Mouse X");
        }

        public static float MouseAxisY()
        {
            return Input.GetAxis("Mouse Y");
        }

        public static float MouseScroll()
        {
            return Input.GetAxis("Mouse ScrollWheel");
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);
        }

        public static bool GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }

        public static Vector2 GetTouchDelta(int touchIndex)
        {
            return Input.GetTouch(touchIndex).deltaPosition;
        }

        public static Vector2 TouchPosition(int touchIndex)
        {
            return Input.GetTouch(touchIndex).position;
        }

        public static bool TouchBegan(int touchIndex)
        {
            Touch touch = Input.GetTouch(touchIndex);
            return touch.phase == UnityEngine.TouchPhase.Began;
        }

        public static bool TouchEndedOrCanceled(int touchIndex)
        {
            Touch touch = Input.GetTouch(touchIndex);
            return touch.phase == UnityEngine.TouchPhase.Ended || touch.phase == UnityEngine.TouchPhase.Canceled;
        }

        public static bool TouchMoved(int touchIndex)
        {
            Touch touch = Input.GetTouch(touchIndex);
            return touch.phase == UnityEngine.TouchPhase.Moved;
        }
    }
}
