using UnityEngine;

namespace BoardDefence.Events
{
    public struct DragStartedEvent
    {
        public string DefenceKey;
        public Vector2 ScreenPosition;
    }

    public struct DragUpdatedEvent
    {
        public string DefenceKey;
        public Vector2 ScreenPosition;
    }

    public struct DragEndedEvent
    {
        public string DefenceKey;
        public Vector2 ScreenPosition;
    }
}
