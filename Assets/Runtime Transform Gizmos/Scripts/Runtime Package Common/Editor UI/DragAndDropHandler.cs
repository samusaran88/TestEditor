 
using UnityEngine;
using UnityEditor;

namespace RTG
{
    public abstract class DragAndDropHandler
    {
        public void Handle(Event dragAndDropEvent, Rect dropAreaRectangle)
        {

#if UNITY_EDITOR
            switch (dragAndDropEvent.type)
            {
                case EventType.DragUpdated:

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    break;

                case EventType.DragPerform:

                    if (dropAreaRectangle.Contains(dragAndDropEvent.mousePosition) &&
                        dragAndDropEvent.type == EventType.DragPerform) PerformDrop();
                    break;
            }
#endif
        }

        protected abstract void PerformDrop();
    }
}
 