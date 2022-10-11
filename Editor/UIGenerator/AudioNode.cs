using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using System.Reflection;
using GameFramework.Utils;
using System.Linq;
using GameFramework.Runtime.Game;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class AudioNode : ViewNode, UIEvent
    {

        public sealed class Data : NodeData
        {
            public string audioGuid;
            public bool loop;
            public TriggerType trigger;
            internal float time;
            public float delayTime;
            public bool isFinishCllback;
        }
        MethodWrapper _playPreviewClip;
        public AudioClip clip { get; set; }
        private Edge edge;
        public Data Datable;
        public AudioNode()
        {
            this.AddPort<AudioNode>("Content", Direction.Output, Capacity.Multi);
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            _playPreviewClip = new MethodWrapper(audioUtilClass, "PlayPreviewClip", new[] { typeof(AudioClip), typeof(int), typeof(bool) });
        }
        public override void Initialized(NodeData data)
        {
            if (data == null)
            {
                data = new Data();
            }
            Datable = (Data)data;
            if (string.IsNullOrEmpty(Datable.audioGuid))
            {
                return;
            }
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(Datable.audioGuid));
        }
        public override NodeData GetNodeData()
        {
            Datable.audioGuid = AssetDatabase.GetAssetPath(clip);
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
                GUILayout.Label("Is Callback");
                GUILayout.FlexibleSpace();
                Datable.isFinishCllback = GUILayout.Toggle(Datable.isFinishCllback, "");
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Loop");
                GUILayout.FlexibleSpace();
                Datable.loop = GUILayout.Toggle(Datable.loop, "");
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Clip");
                GUILayout.FlexibleSpace();
                clip = (AudioClip)EditorGUILayout.ObjectField("", clip, typeof(AudioClip), false);
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Play"))
            {
                _playPreviewClip.Invoke(clip, 0, false);
            }
        }
    }
}