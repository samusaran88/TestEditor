using UnityEngine;
using System.Collections.Generic;

namespace RTG
{
    /// <summary>
    /// The main focus in this tutorial is to learn how to assign multiple
    /// target objects to a gizmo. This enables our gizmos to control more
    /// than one object at a time.
    /// </summary>
    public class Tut_2_MultipleTargetObjects : MonoBehaviour
    {
        /// <summary>
        /// A private enum which is used by the class to differentiate between different 
        /// gizmo types. An example where this enum will come in handy is when we use the 
        /// W,E,R,T keys to switch between different types of gizmos. When the W key is 
        /// pressed for example, we will call the 'SetWorkGizmoId' function passing 
        /// GizmoId.Move as the parameter.
        /// </summary>
        private enum GizmoId
        {
            Move = 1,
            Rotate,
            Scale,
            Universal
        }

        /// <summary>
        /// The following 4 variables are references to the ObjectTransformGizmo behaviours
        /// that will be used to move, rotate and scale our objects.
        /// </summary>
        private ObjectTransformGizmo _objectMoveGizmo;
        private ObjectTransformGizmo _objectRotationGizmo;
        private ObjectTransformGizmo _objectScaleGizmo;
        private ObjectTransformGizmo _objectUniversalGizmo;

        /// <summary>
        /// The current work gizmo id. The work gizmo is the gizmo which is currently used
        /// to transform objects. The W,E,R,T keys can be used to change the work gizmo as
        /// needed.
        /// </summary>
        private GizmoId _workGizmoId;
        /// <summary>
        /// A reference to the current work gizmo. If the work gizmo id is GizmoId.Move, then
        /// this will point to '_objectMoveGizmo'. For GizmoId.Rotate, it will point to 
        /// '_objectRotationGizmo' and so on.
        /// </summary>
        private ObjectTransformGizmo _workGizmo;
        /// <summary>
        /// A list of objects which are currently selected. This is also the list that holds
        /// the gizmo target objects. 
        /// </summary>
        private List<GameObject> _selectedObjects = new List<GameObject>();

        /// <summary>
        /// Performs all necessary initializations.
        /// </summary>
        private void Start()
        {
            // Create the 4 gizmos
            _objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
            _objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
            _objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
            _objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

            _objectMoveGizmo.Gizmo.SetEnabled(false);
            _objectRotationGizmo.Gizmo.SetEnabled(false);
            _objectScaleGizmo.Gizmo.SetEnabled(false);
            _objectUniversalGizmo.Gizmo.SetEnabled(false);

            _objectMoveGizmo.SetTargetObjects(_selectedObjects);
            _objectRotationGizmo.SetTargetObjects(_selectedObjects);
            _objectScaleGizmo.SetTargetObjects(_selectedObjects);
            _objectUniversalGizmo.SetTargetObjects(_selectedObjects);

            _workGizmo = _objectMoveGizmo;
            _workGizmoId = GizmoId.Move;
        }

        /// <summary>
        /// Called every frame to perform all necessary updates. In this tutorial,
        /// we listen to user input and take action. 
        /// </summary>
        private void Update()
        {
            if (RTInput.GetMouseButtonDown(0) && 
                RTGizmosEngine.Get.HoveredGizmo == null)
            {
                GameObject pickedObject = PickGameObject();
                if (pickedObject != null)
                {
                    if (RTInput.GetKey(KeyCode.LeftControl))
                    {
                        if (_selectedObjects.Contains(pickedObject)) 
                            _selectedObjects.Remove(pickedObject);
                        else 
                            _selectedObjects.Add(pickedObject);
                        
                        OnSelectionChanged();
                    }
                    else
                    {
                        _selectedObjects.Clear();
                        _selectedObjects.Add(pickedObject);

                        OnSelectionChanged();
                    }
                }
                else
                {
                    _selectedObjects.Clear();
                    OnSelectionChanged();

                    OnSelectionChanged();
                }
            }

            if (RTInput.GetKeyDown(KeyCode.W)) 
                SetWorkGizmoId(GizmoId.Move);
            else if (RTInput.GetKeyDown(KeyCode.E)) 
                SetWorkGizmoId(GizmoId.Rotate);
            else if (RTInput.GetKeyDown(KeyCode.R)) 
                SetWorkGizmoId(GizmoId.Scale);
            else if (RTInput.GetKeyDown(KeyCode.T)) 
                SetWorkGizmoId(GizmoId.Universal);
        }

        /// <summary>
        /// Uses the mouse position to pick a game object in the scene. Returns
        /// the picked game object or null if no object is picked.
        /// </summary>
        /// <remarks>
        /// Objects must have colliders attached.
        /// </remarks>
        private GameObject PickGameObject()
        {
            // Build a ray using the current mouse cursor position
            Ray ray = Camera.main.ScreenPointToRay(RTInput.mousePosition);

            // Check if the ray intersects a game object. If it does, return it
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, float.MaxValue))
                return rayHit.collider.gameObject;

            // No object is intersected by the ray. Return null.
            return null;
        }

        /// <summary>
        /// This function is called to change the type of work gizmo. This is
        /// used in the 'Update' function in response to the user pressing the
        /// W,E,R,T keys to switch between different gizmo types.
        /// </summary>
        private void SetWorkGizmoId(GizmoId gizmoId)
        {
            // If the specified gizmo id is the same as the current id, there is nothing left to do
            if (gizmoId == _workGizmoId) 
                return;

            // Start with a clean slate and disable all gizmos
            _objectMoveGizmo.Gizmo.SetEnabled(false);
            _objectRotationGizmo.Gizmo.SetEnabled(false);
            _objectScaleGizmo.Gizmo.SetEnabled(false);
            _objectUniversalGizmo.Gizmo.SetEnabled(false);

            // At this point all gizmos are disabled. Now we need to check the gizmo id
            // and adjust the '_workGizmo' variable.
            _workGizmoId = gizmoId;
            if (gizmoId == GizmoId.Move) 
                _workGizmo = _objectMoveGizmo;
            else if (gizmoId == GizmoId.Rotate) 
                _workGizmo = _objectRotationGizmo;
            else if (gizmoId == GizmoId.Scale) 
                _workGizmo = _objectScaleGizmo;
            else if (gizmoId == GizmoId.Universal) 
                _workGizmo = _objectUniversalGizmo;

            if (_selectedObjects.Count != 0)
            {
                _workGizmo.Gizmo.SetEnabled(true);
              _workGizmo.RefreshPositionAndRotation();
            }
        }

        /// <summary>
        /// Called from the 'Update' function whenever the '_selectedObjects' list
        /// changes. It is responsible for updating the gizmos accordingly.
        /// </summary>
        private void OnSelectionChanged()
        {
            if (_selectedObjects.Count != 0)
            {
                 _workGizmo.Gizmo.SetEnabled(true);
               _workGizmo.RefreshPositionAndRotation();
            }
            else
            {
                _objectMoveGizmo.Gizmo.SetEnabled(false);
                _objectRotationGizmo.Gizmo.SetEnabled(false);
                _objectScaleGizmo.Gizmo.SetEnabled(false);
                _objectUniversalGizmo.Gizmo.SetEnabled(false);
            }
        }
    }
}
