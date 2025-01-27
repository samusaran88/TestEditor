﻿using UnityEngine;
using System;
using UnityEditor;

namespace RTG
{
    [Serializable]
    public class UniversalGizmoHotkeys : Settings
    {
        [SerializeField]
        private Hotkeys _enable2DMode = new Hotkeys("Enable 2D mode", new HotkeysStaticData { CanHaveMouseButtons = false })
        {
            Key = KeyCode.None,
            LShift = true
        };
        [SerializeField]
        private Hotkeys _enableSnapping = new Hotkeys("Enable snapping", new HotkeysStaticData { CanHaveMouseButtons = false })
        {
            Key = KeyCode.None,
            LCtrl = true
        };
        [SerializeField]
        private Hotkeys _enableVertexSnapping = new Hotkeys("Enable vertex snapping", new HotkeysStaticData { CanHaveMouseButtons = false })
        {
            UseStrictModifierCheck = false,
            Key = KeyCode.V
        };

        public Hotkeys Enable2DMode { get { return _enable2DMode; } }
        public Hotkeys EnableSnapping { get { return _enableSnapping; } }
        public Hotkeys EnableVertexSnapping { get { return _enableVertexSnapping; } }

        protected override void RenderContent(UnityEngine.Object undoRecordObject)
        {

#if UNITY_EDITOR
            _enable2DMode.RenderEditorGUI(undoRecordObject);
            _enableSnapping.RenderEditorGUI(undoRecordObject);
            _enableVertexSnapping.RenderEditorGUI(undoRecordObject);
#endif
        }
    }
}
