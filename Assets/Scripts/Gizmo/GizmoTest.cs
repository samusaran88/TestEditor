using RTG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace XED
{
    public class GizmoTest : MonoBehaviour
    {
        public GameObject testObject;
        ObjectTransformGizmo moveGizmo;
        ObjectTransformGizmo rotateGizmo;
        ObjectTransformGizmo scaleGizmo;
        ObjectTransformGizmo universalGizmo;
        HashSet<GameObject> gizmoTargets = new();
        // Start is called before the first frame update
        void Start()
        {
            moveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo(); 
            rotateGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo(); 
            scaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
            universalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();
            moveGizmo.SetTargetObjects(gizmoTargets);
            rotateGizmo.SetTargetObjects(gizmoTargets);
            scaleGizmo.SetTargetObjects(gizmoTargets);
            universalGizmo.SetTargetObjects(gizmoTargets);
            moveGizmo.Gizmo.SetEnabled(false);
            rotateGizmo.Gizmo.SetEnabled(false);
            scaleGizmo.Gizmo.SetEnabled(false);
            universalGizmo.Gizmo.SetEnabled(false);
            gizmoTargets.Add(testObject);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                moveGizmo.Gizmo.SetEnabled(true);
                rotateGizmo.Gizmo.SetEnabled(false);
                scaleGizmo.Gizmo.SetEnabled(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                moveGizmo.Gizmo.SetEnabled(false);
                rotateGizmo.Gizmo.SetEnabled(true);
                scaleGizmo.Gizmo.SetEnabled(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                moveGizmo.Gizmo.SetEnabled(false);
                rotateGizmo.Gizmo.SetEnabled(false);
                scaleGizmo.Gizmo.SetEnabled(true);
            }
        }
    }
}
