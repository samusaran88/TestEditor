 
using UnityEngine;
using UnityEditor;

namespace RTG
{
    public static class EditorUndoEx
    {
        public static void Record(UnityEngine.Object objectToRecord)
        {

#if UNITY_EDITOR
            if (!Application.isPlaying) Undo.RecordObject(objectToRecord, "RTTGizmos Undo");
#endif
        }
    }
}
 