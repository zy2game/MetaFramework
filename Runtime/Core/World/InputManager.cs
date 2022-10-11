using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public sealed class InputEvent
    {
        public Vector3 position;
        public float scrollWheel;
        private bool isUsed;
        internal InputEvent(Vector3 position)
        {
            this.position = position;
            isUsed = false;
        }

        internal InputEvent(float position)
        {
            this.scrollWheel = position;
            isUsed = false;
        }
        public void Use()
        {
            isUsed = true;
        }

        public bool HasUseged()
        {
            return isUsed;
        }
    }
    public sealed class InputManager : GObject
    {
        private GameFrameworkAction<InputEvent> mouseDownEvent;
        private GameFrameworkAction<InputEvent> mouseUpEvent;
        private GameFrameworkAction<InputEvent> mouseUpdateEvent;
        private GameFrameworkAction<InputEvent> mouseScrollWheel;

        private float scroolWheel;

        public void Dispose()
        {
        }

        internal void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseDownEvent?.Invoke(new InputEvent(Input.mousePosition));
                return;
            }

            if (Input.GetMouseButtonUp(0))
            {
                mouseUpEvent?.Invoke(new InputEvent(Input.mousePosition));
                return;
            }

            if (Input.GetMouseButton(0))
            {
                mouseUpdateEvent?.Invoke(new InputEvent(Input.mousePosition));
                return;
            }
            scroolWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scroolWheel != 0)
            {
                mouseScrollWheel?.Invoke(new InputEvent(scroolWheel));
            }

        }

        public void AddMouseButtonDownEvent(GameFrameworkAction<InputEvent> listener)
        {
            this.mouseDownEvent += listener;
        }
        public void RemoveMouseButtonDownEvent(GameFrameworkAction<InputEvent> listener)
        {
            this.mouseDownEvent -= listener;
        }

        public void AddMouseButtonUpEvent(GameFrameworkAction<InputEvent> listener)
        {
            this.mouseUpEvent += listener;
        }

        public void RemoveMouseButtonUpEvent(GameFrameworkAction<InputEvent> listener)
        {
            this.mouseUpEvent -= listener;
        }

        public void AddMouseButtonUpdateEvent(GameFrameworkAction<InputEvent> listener)
        {
            this.mouseUpdateEvent += listener;
        }

        public void RemoveMouseButtonUpdateEvent(GameFrameworkAction<InputEvent> listener)
        {
            this.mouseUpdateEvent -= listener;
        }
        public void AddMouseScrollWheelEvent(GameFrameworkAction<InputEvent> listener)
        {
            this.mouseScrollWheel += listener;
        }

        public void RemoveMouseScrollWheelEvent(GameFrameworkAction<InputEvent> listener)
        {
            this.mouseScrollWheel -= listener;
        }
    }
}