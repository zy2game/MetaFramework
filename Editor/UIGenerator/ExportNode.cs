using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using System.Text;
using GameFramework.Runtime.Game;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class ExportNode : ViewNode
    {
        private UIGeneratorWindow window;
        public ExportNode()
        {
            window = UIGeneratorWindow.window;
            this.AddPort<EntityNode>("Childs", Direction.Input, Capacity.Multi);
            this.AddPort<UIComponent>("Components", Direction.Input, Capacity.Single);
        }



        public string GenerateScriptCode(string name)
        {
            StringBuilder builder = new StringBuilder();
            List<TweenNode> tweens = UIGeneratorWindow.window.GetNodes<TweenNode>();
            List<AudioNode> audios = UIGeneratorWindow.window.GetNodes<AudioNode>();

            builder.AppendLine($"---@class {name} ui½çÃæ");
            builder.AppendLine($"local {name} = {{}}");
            builder.AppendLine($"local this = {name}");
            builder.AppendLine();
            builder.AppendLine("this.script = nil");
            builder.AppendLine();
            builder.AppendLine("--- initialize the ui script");
            builder.AppendLine($"function {name}:start()");
            builder.AppendLine();
            tweens.ForEach(x =>
            {
                if (x.Datable.trigger == TriggerType.Start)
                {

                }
            });
            audios.ForEach(x =>
            {
                if (x.Datable.trigger == TriggerType.Start)
                {

                }
            });
            builder.AppendLine("end");
            builder.AppendLine();
            builder.AppendLine("--- disposed the ui");
            builder.AppendLine($"function {name}:dispose()");
            builder.AppendLine();
            builder.AppendLine("end");
            builder.AppendLine();
            builder.AppendLine("--- show in the ui gameobject");
            builder.AppendLine($"function {name}:enable()");
            builder.AppendLine();
            builder.AppendLine("end");
            builder.AppendLine();
            builder.AppendLine("--- hide the ui gameobject");
            builder.AppendLine($"function {name}:disable()");
            builder.AppendLine();
            builder.AppendLine("end");

            builder.AppendLine($"function {name}:eventHandle(eventId, sender, args)");
            List<ButtonNode> buttons = UIGeneratorWindow.window.GetNodes<ButtonNode>();
            foreach (var button in buttons)
            {
                EntityNode entity = button.GetEntityNode();
                if (entity == null)
                {
                    continue;
                }
                builder.AppendLine($"\tif eventId == \"{entity.Datable.name}\" then");
                tweens.ForEach(x =>
                {
                    EntityNode basic = x.GetEntityNode();
                    if (basic != null && basic == entity && x.Datable.trigger == TriggerType.Click)
                    {
                        builder.AppendLine($"\t\tthis:on_invoke_playerTween_{x.Datable.TweenName}()");
                    }
                });
                audios.ForEach(x =>
                {
                    EntityNode basic = x.GetEntityNode();
                    if (basic != null && basic.guid == entity.guid && x.Datable.trigger == TriggerType.Click)
                    {
                        builder.AppendLine($"\t\tthis.script:PlaySound(\"{x.clip?.name}\")");
                    }
                });
                builder.AppendLine($"\t\tthis:on_handle_clickEvent_{entity.Datable.name}(sender, args)");
                builder.AppendLine("\t\treturn;");
                builder.AppendLine("\tend");
            }

            List<InputFieldNode> inputs = UIGeneratorWindow.window.GetNodes<InputFieldNode>();
            foreach (var item in inputs)
            {
                EntityNode entity = item.GetEntityNode();
                if (entity == null)
                {
                    continue;
                }
                builder.AppendLine($"\tif eventId == \"{entity.Datable.name}\" then");
                tweens.ForEach(x =>
                {
                    EntityNode basic = x.GetEntityNode();
                    if (basic != null && basic == entity && x.Datable.trigger == TriggerType.Click)
                    {
                        builder.AppendLine($"\t\tthis:on_invoke_playerTween_{x.Datable.TweenName}()");
                    }
                });
                audios.ForEach(x =>
                {
                    EntityNode basic = x.GetEntityNode();
                    if (basic != null && basic.guid == entity.guid && x.Datable.trigger == TriggerType.Click)
                    {
                        builder.AppendLine($"\t\tthis.script:PlaySound(\"{x.clip?.name}\")");
                    }
                });
                builder.AppendLine($"\t\tthis:on_handle_submitInput_{entity.Datable.name}(sender, args)");
                builder.AppendLine("\t\treturn;");
                builder.AppendLine("\tend");
            }

            builder.AppendLine("end");
            foreach (var item in tweens)
            {
                EntityNode entity = item.GetEntityNode();
                if (entity == null)
                {
                    continue;
                }
                builder.AppendLine(item.GenerateScriptCode(name));
            }
            foreach (var item in buttons)
            {
                EntityNode entity = item.GetEntityNode();
                if (entity == null)
                {
                    continue;
                }
                builder.AppendLine($"function {name}:on_handle_clickEvent_{entity.Datable.name}(sender, args)");
                builder.AppendLine("\t--todo please write your code to here");
                builder.AppendLine("end");
            }
            foreach (var item in inputs)
            {
                EntityNode entity = item.GetEntityNode();
                if (entity == null)
                {
                    continue;
                }
                builder.AppendLine($"function {name}:on_handle_submitInput_{entity.Datable.name}(sender, args)");
                builder.AppendLine("\t--todo please write your code to here");
                builder.AppendLine("end");
            }
            return builder.ToString();
        }

        public override GameObject GenerateGameObject(Transform parent, string savedAssetPath)
        {
            GameObject obj = new GameObject();
            RectTransform transform = obj.AddComponent<RectTransform>();
            obj.transform.parent = parent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            Port port = this.GetPort("Childs");
            foreach (var item in port.connections)
            {
                ViewNode node = (ViewNode)item.output.node;
                if (node == null)
                {
                    continue;
                }
                node.GenerateGameObject(obj.transform, savedAssetPath);
            }
            return obj;
        }
    }
}