using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using System.Text;
using System.Linq;
using GameFramework.Runtime.Game;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class TweenNode : ViewNode, UIEvent, UIComponent
    {
        public enum AniType : byte
        {
            Line,
            Pinpong,
        }
        public enum TweenType : byte
        {
            Position,
            Rotation,
            Scale,
            Color,
        }

        public sealed class Data : NodeData
        {
            public TweenType tweenType;
            public List<Vector3> path;
            public List<Color> color;
            internal bool loop;
            internal AniType aniType;
            internal bool havChild;
            internal float time;
            public TriggerType trigger;
            public float delayTime;
            public string TweenName;
            public Data()
            {
                path = new List<Vector3>();
                color = new List<Color>();
            }
        }
        private Edge edge;
        public Data Datable;
        public TweenNode()
        {
            this.AddPort<TweenNode>("Content", Direction.Output, Capacity.Multi);
        }
        public override void Initialized(NodeData data)
        {
            if (data == null)
            {
                data = new Data();
            }
            Datable = (Data)data;
        }
        public override NodeData GetNodeData()
        {
            return Datable;
        }
        public override void OnInspector()
        {
            if (this.GetPort("Content").connections.Count() > 0)
            {
                edge = this.GetPort("Content").connections.FirstOrDefault();
                if (edge.input.portName == "Components")
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Trigger");
                        Datable.trigger = (TriggerType)EditorGUILayout.EnumPopup("", Datable.trigger);
                        GUILayout.EndHorizontal();
                    }
                    if (Datable.trigger == TriggerType.Time)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Delay Time");
                            Datable.delayTime = EditorGUILayout.FloatField("", Datable.delayTime);
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    Datable.trigger = TriggerType.Click;
                }
            }
            else
            {
                if (edge != null)
                {
                    edge = null;
                }
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Animator Name");
                GUILayout.FlexibleSpace();
                Datable.TweenName = EditorGUILayout.TextField(Datable.TweenName);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Tween Type");
                GUILayout.FlexibleSpace();
                Datable.tweenType = (TweenType)EditorGUILayout.EnumPopup(Datable.tweenType);
                if (GUI.changed)
                {
                    Datable.color.Clear();
                    Datable.path.Clear();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Time");
                Datable.time = EditorGUILayout.FloatField("", Datable.time);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Loop");
                GUILayout.FlexibleSpace();
                Datable.loop = GUILayout.Toggle(Datable.loop, "");
                GUILayout.EndHorizontal();
            }

            if (Datable.tweenType == TweenType.Color)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Have Childs");
                    GUILayout.FlexibleSpace();
                    Datable.havChild = GUILayout.Toggle(Datable.havChild, "");
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Animator Type");
                GUILayout.FlexibleSpace();
                Datable.aniType = (AniType)EditorGUILayout.EnumPopup(Datable.aniType);
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            switch (Datable.tweenType)
            {
                case TweenType.Position:
                case TweenType.Rotation:
                case TweenType.Scale:
                    for (int i = 0; i < Datable.path.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Node:");
                            Datable.path[i] = EditorGUILayout.Vector3Field("", Datable.path[i]);
                            GUILayout.EndHorizontal();
                        }
                    }
                    if (GUILayout.Button("+"))
                    {
                        Datable.path.Add(Vector3.zero);
                    }
                    break;
                case TweenType.Color:
                    for (int i = 0; i < Datable.color.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Node:");
                            Datable.color[i] = EditorGUILayout.ColorField("", Datable.color[i]);
                            GUILayout.EndHorizontal();
                        }
                    }
                    if (GUILayout.Button("+"))
                    {
                        Datable.color.Add(Color.white);
                    }
                    break;
            }
        }
        public string GenerateScriptCode(string exportName)
        {
            StringBuilder builder = new StringBuilder();
            EntityNode entity = GetEntityNode();
            builder.AppendLine($"function {exportName}:on_invoke_playerTween_{Datable.TweenName}()");
            if (Datable.tweenType == TweenType.Color)
            {
                builder.AppendLine("\tlocal colors = {}");
                for (var i = 0; i < Datable.color.Count; i++)
                {
                    builder.AppendLine($"\ttable.insert(colors, {(i + 1)}, Color.New({Datable.color[i].r}, {Datable.color[i].g}, {Datable.color[i].b}, {Datable.color[i].a}))");
                }
                builder.AppendLine($"\tthis.script:TweenColor(\"{entity.Datable.name}\", colors, {Datable.time}, function()\n\t\tthis:on_handle_playerTween_{Datable.TweenName}_Callback()\n\tend)");
            }
            else
            {
                builder.AppendLine("\tlocal paths = {}");
                for (var i = 0; i < Datable.path.Count; i++)
                {
                    builder.AppendLine($"\ttable.insert(paths, {(i + 1)}, Vector3.New({Datable.path[i].x},{Datable.path[i].y},{Datable.path[i].z}))");
                }
                if (Datable.tweenType == TweenType.Position)
                {
                    builder.AppendLine($"\tthis.script:TweenMovement(\"{entity.Datable.name}\", paths, {Datable.time}), function()\n\t\tthis:on_handle_playerTween_{Datable.TweenName}_Callback()\n\tend)");
                }
                if (Datable.tweenType == TweenType.Rotation)
                {
                    builder.AppendLine($"\tthis.script:TweenRotation(\"{entity.Datable.name}\", paths, {Datable.time}, function()\n\t\tthis:on_handle_playerTween_{Datable.TweenName}_Callback()\n\tend))");
                }
                if (Datable.tweenType == TweenType.Scale)
                {
                    builder.AppendLine($"\tthis.script:TweenScale(\"{entity.Datable.name}\", paths, {Datable.time}, function()\n\t\tthis:on_handle_playerTween_{Datable.TweenName}_Callback()\n\tend))");
                }
            }
            builder.AppendLine("end");
            builder.AppendLine($"function {exportName}:on_handle_playerTween_{Datable.TweenName}_Callback()");
            builder.AppendLine("\t--todo please write your  code to here");
            builder.AppendLine("end");
            return builder.ToString();
        }
    }
}