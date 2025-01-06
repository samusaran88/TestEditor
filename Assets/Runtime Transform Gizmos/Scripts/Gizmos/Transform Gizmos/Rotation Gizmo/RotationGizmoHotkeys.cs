using UnityEngine;
using UnityEditor;
using System;

namespace RTG
{
    [Serializable]
    public class RotationGizmoHotkeys : Settings
    {
        [SerializeField]
        private Hotkeys _enableSnapping = new Hotkeys("Enable snapping", new HotkeysStaticData { CanHaveMouseButtons = false })
        {
            Key = KeyCode.None,
            LCtrl = true
        };

        public Hotkeys EnableSnapping { get { return _enableSnapping; } }

        protected override void RenderContent(UnityEngine.Object undoRecordObject)
        {

#if UNITY_EDITOR
            _enableSnapping.RenderEditorGUI(undoRecordObject);
#endif
        }
    }
}
