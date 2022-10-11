using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEngine.UI.Selectable;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class ButtonNode : ViewNode, UIComponent
    {
        public sealed class Data : NodeData
        {
            public Transition transition;
            public List<string> sprite;
            public List<Color> color;
            public Data()
            {
                sprite = new List<string>() { string.Empty, string.Empty, string.Empty, string.Empty };
                color = new List<Color>() { Color.white, Color.white, Color.white, Color.white };
            }
        }
        public Data Datable;
        private string[] stateName = new[] { "Highlighted", "Pressed", "Selected", "Disabled" };
        public ButtonNode()
        {
            this.AddPort<ButtonNode>("Content", Direction.Output, Capacity.Single);
            this.AddPort<UIEvent>("Click Event", Direction.Input, Capacity.Multi);

        }
        private List<Sprite> sprites;
        public override void Initialized(NodeData data)
        {
            if (data == null)
            {
                data = new Data();
            }
            Datable = (Data)data;
            sprites = new List<Sprite>() { null, null, null, null };
            for (int i = 0; i < Datable.sprite.Count; i++)
            {
                if (string.IsNullOrEmpty(Datable.sprite[i]))
                {
                    continue;
                }
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(Datable.sprite[i]));
            }
        }
        public override NodeData GetNodeData()
        {
            if (Datable.transition == Transition.SpriteSwap)
            {
                for (int i = 0; i < sprites.Count; i++)
                {
                    Datable.sprite[i] = AssetDatabase.GetAssetPath(sprites[i]);
                }
            }
            return Datable;
        }
        public override void OnInspector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Button Type");
                Datable.transition = (Transition)EditorGUILayout.EnumPopup(Datable.transition);
                GUILayout.EndHorizontal();
            }
            switch (Datable.transition)
            {
                case Transition.None:
                case Transition.Animation:
                    break;
                case Transition.SpriteSwap:
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(stateName[i]);
                            sprites[i] = (Sprite)EditorGUILayout.ObjectField("", sprites[i], typeof(Sprite), false);
                            GUILayout.EndHorizontal();
                        }
                    }
                    break;
                case Transition.ColorTint:
                    for (int i = 0; i < Datable.color.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(stateName[i]);
                            Datable.color[i] = EditorGUILayout.ColorField("", Datable.color[i]);
                            GUILayout.EndHorizontal();
                        }
                    }
                    break;

            }
        }
        public override void GenerateComponent(GameObject target, string savedAssetPath)
        {
            if (target.GetComponent<UnityEngine.UI.Button>() != null)
            {
                return;
            }
            UnityEngine.UI.Button button = target.AddComponent<UnityEngine.UI.Button>();
            button.transition = Datable.transition;
            if (button.transition == Transition.ColorTint)
            {
                UnityEngine.UI.SpriteState spriteState = new UnityEngine.UI.SpriteState();
                spriteState.highlightedSprite = sprites[0];
                spriteState.pressedSprite = sprites[1];
                spriteState.selectedSprite = sprites[2];
                spriteState.disabledSprite = sprites[3];
                button.spriteState = spriteState;

            }
            if (button.transition == Transition.SpriteSwap)
            {
                UnityEngine.UI.ColorBlock colorBlock = new UnityEngine.UI.ColorBlock();
                colorBlock.normalColor = Color.white;
                colorBlock.highlightedColor = Datable.color[0];
                colorBlock.pressedColor = Datable.color[1];
                colorBlock.selectedColor = Datable.color[2];
                colorBlock.disabledColor = Datable.color[3];
                button.colors = colorBlock;
            }
        }
    }
}