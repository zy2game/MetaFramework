using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEngine.UI.Selectable;
using static UnityEngine.UI.InputField;
using System.Linq;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class InputFieldNode : ViewNode, UIComponent
    {
        public sealed class Data : NodeData
        {
            public Color seletion = Color.white;
            public List<string> sprite;
            public List<Color> color;
            public Transition transition { get; set; }
            public ContentType contentType { get; set; }
            public LineType lineType { get; set; }
            public InputType inputType { get; set; }
            public TouchScreenKeyboardType keyboardType { get; set; }
            public CharacterValidation characterValidation { get; set; }

            public Data()
            {
                sprite = new List<string>() { String.Empty, String.Empty, String.Empty, String.Empty };
                color = new List<Color>() { Color.white, Color.white, Color.white, Color.white };
            }
        }
        public Data Datable;
        private List<Sprite> sprites;
        private string[] stateName = new[] { "Highlighted", "Pressed", "Selected", "Disabled" };
        public InputFieldNode()
        {
            this.AddPort<EntityNode>("Text", Direction.Input, Capacity.Single);
            this.AddPort<EntityNode>("Placeholder", Direction.Input, Capacity.Single);
            this.AddPort<UIEvent>("Submit", Direction.Input, Capacity.Multi);
            this.AddPort<InputFieldNode>("Content", Direction.Output, Capacity.Single);
        }
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
                GUILayout.Label("Transition");
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

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Selection Color");
                Datable.seletion = EditorGUILayout.ColorField("", Datable.seletion);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Content Type", GUILayout.Width(100));
                Datable.contentType = (ContentType)EditorGUILayout.EnumPopup(Datable.contentType);
                GUILayout.EndHorizontal();
            }
            if (Datable.contentType == ContentType.Standard || Datable.contentType == ContentType.Autocorrected || Datable.contentType == ContentType.Custom)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("lineType", GUILayout.Width(100));
                    Datable.lineType = (LineType)EditorGUILayout.EnumPopup(Datable.lineType);
                    GUILayout.EndHorizontal();
                }
            }
            if (Datable.contentType == ContentType.Custom)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("inputType", GUILayout.Width(100));
                    Datable.inputType = (InputType)EditorGUILayout.EnumPopup(Datable.inputType);
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("keyboardType", GUILayout.Width(100));
                    Datable.keyboardType = (TouchScreenKeyboardType)EditorGUILayout.EnumPopup(Datable.keyboardType);
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("character Validation", GUILayout.Width(100));
                    Datable.characterValidation = (CharacterValidation)EditorGUILayout.EnumPopup(Datable.characterValidation);
                    GUILayout.EndHorizontal();
                }
            }
        }

        public override void GenerateComponent(GameObject target, string savedAssetPath)
        {
            if (target.GetComponent<UnityEngine.UI.InputField>() != null)
            {
                return;
            }
            UnityEngine.UI.InputField inputField = target.AddComponent<UnityEngine.UI.InputField>();
            inputField.transition = Datable.transition;
            inputField.contentType = Datable.contentType;
            inputField.lineType = Datable.lineType;
            inputField.inputType = Datable.inputType;
            inputField.keyboardType = Datable.keyboardType;
            inputField.characterValidation = Datable.characterValidation;
            inputField.selectionColor = Datable.seletion;
            if (Datable.transition == Transition.SpriteSwap)
            {
                inputField.spriteState = new UnityEngine.UI.SpriteState()
                {
                    highlightedSprite = sprites[0],
                    pressedSprite = sprites[1],
                    selectedSprite = sprites[2],
                    disabledSprite = sprites[3],
                };
            }
            if (Datable.transition == Transition.ColorTint)
            {
                inputField.colors = new UnityEngine.UI.ColorBlock()
                {
                    normalColor = Color.white,
                    highlightedColor = Datable.color[0],
                    pressedColor = Datable.color[1],
                    selectedColor = Datable.color[2],
                    disabledColor = Datable.color[3],
                };
            }

            Edge text = this.GetPort("Text").connections.FirstOrDefault();
            if (text != null)
            {
                EntityNode lableNode = (EntityNode)text.output.node;
                GameObject textObject = lableNode.GenerateGameObject(target.transform, savedAssetPath);
                textObject.name = target.name + "_Text";
                lableNode.GenerateComponent(textObject, savedAssetPath);
                inputField.textComponent = textObject.GetComponent<UnityEngine.UI.Text>();
            }

            Edge Placeholder = this.GetPort("Placeholder").connections.FirstOrDefault();
            if (Placeholder != null)
            {
                EntityNode lableNode = (EntityNode)Placeholder.output.node;
                GameObject PlaceholderObject = lableNode.GenerateGameObject(target.transform, savedAssetPath);
                PlaceholderObject.name = target.name + "_Placeholder";
                lableNode.GenerateComponent(PlaceholderObject, savedAssetPath);
                inputField.placeholder = PlaceholderObject.GetComponent<UnityEngine.UI.Text>();
            }
        }
    }
}