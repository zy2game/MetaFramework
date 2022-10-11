using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.Experimental.GraphView.Port;
using PhotoshopFile;
using System.IO;

namespace GameFramework.Editor.UIGenerator
{
    public sealed class SpriteNode : ViewNode, UIComponent
    {
        public sealed class Data : NodeData
        {
            public string spriteGuid;
            public byte alpha { get; set; }
            public bool isPsd { get; set; }
            public bool RaycastTarget { get; set; }
            public bool Maskble { get; set; }
            public UnityEngine.UI.Image.Type imageType { get; set; }
            public UnityEngine.UI.Image.FillMethod fillMethod { get; set; }
            public UnityEngine.UI.Image.Origin360 origin { get; set; }
            public float fillamount { get; set; }
            public bool useSpriteMesh { get; set; }
            public bool Clockwise { get; set; }
            public bool PreserveAspect { get; set; }
            public bool FillCenter { get; set; }
            public int PixelsPerUnitMultiplier { get; set; }
            public Color color = Color.white;
            public string material;
        }
        public Sprite sprite { get; set; }
        public Material material;
        public Data Datable;
        public SpriteNode()
        {
            this.AddPort<SpriteNode>("Content", Direction.Output, Capacity.Multi);
            this.Refresh();
        }
        public override void Initialized(NodeData data)
        {
            if (data == null)
            {
                data = new Data();
            }
            Datable = (Data)data;
            if (!string.IsNullOrEmpty(Datable.material))
            {
                material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(Datable.material));
            }
            if (!string.IsNullOrEmpty(Datable.spriteGuid))
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(Datable.spriteGuid));
            }
        }
        public override NodeData GetNodeData()
        {
            if (material != null)
            {
                Datable.material = AssetDatabase.GetAssetPath(material);
            }
            if (!Datable.isPsd && sprite != null)
            {
                Datable.spriteGuid = AssetDatabase.GetAssetPath(sprite);
            }
            return Datable;
        }

        public void SetPsdLayer(Layer layer)
        {
            Datable.isPsd = true;
            Datable.alpha = layer.Opacity;
            Texture2D texture = layer.CreateTexture();
            this.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2);
            this.sprite.name = layer.Name;
        }
        public override void OnInspector()
        {
            GUI.enabled = !Datable.isPsd;
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Sprite");
                sprite = (Sprite)EditorGUILayout.ObjectField("", sprite, typeof(Sprite), false);
                GUILayout.EndVertical();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Color");
                Datable.color = EditorGUILayout.ColorField("", Datable.color);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Material");
                material = (Material)EditorGUILayout.ObjectField("", material, typeof(Material), false);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("RaycastTarget");
                GUILayout.FlexibleSpace();
                Datable.RaycastTarget = GUILayout.Toggle(Datable.RaycastTarget, "");
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Maskble");
                GUILayout.FlexibleSpace();
                Datable.Maskble = GUILayout.Toggle(Datable.Maskble, "");
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Image Type");
                Datable.imageType = (UnityEngine.UI.Image.Type)EditorGUILayout.EnumPopup(Datable.imageType);
                GUILayout.EndHorizontal();
            }
            if (Datable.imageType == UnityEngine.UI.Image.Type.Simple)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("useSpriteMesh");
                    GUILayout.FlexibleSpace();
                    Datable.useSpriteMesh = GUILayout.Toggle(Datable.useSpriteMesh, "");
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("PreserveAspect");
                    GUILayout.FlexibleSpace();
                    Datable.PreserveAspect = GUILayout.Toggle(Datable.PreserveAspect, "");
                    GUILayout.EndHorizontal();
                }
            }
            if (Datable.imageType == UnityEngine.UI.Image.Type.Sliced || Datable.imageType == UnityEngine.UI.Image.Type.Tiled)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("useSpriteMesh");
                    GUILayout.FlexibleSpace();
                    Datable.useSpriteMesh = GUILayout.Toggle(Datable.useSpriteMesh, "");
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("PixelsPerUnitMultiplier");
                    Datable.PixelsPerUnitMultiplier = EditorGUILayout.IntField(Datable.PixelsPerUnitMultiplier);
                    GUILayout.EndHorizontal();
                }
            }
            if (Datable.imageType == UnityEngine.UI.Image.Type.Filled)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Fill Method");
                    Datable.fillMethod = (UnityEngine.UI.Image.FillMethod)EditorGUILayout.EnumPopup(Datable.fillMethod);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Fill Origin");
                    Datable.origin = (UnityEngine.UI.Image.Origin360)EditorGUILayout.EnumPopup(Datable.origin);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Fill Amount");
                    Datable.fillamount = EditorGUILayout.Slider(Datable.fillamount, 0, 1f);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Clockwise");
                    GUILayout.FlexibleSpace();
                    Datable.Clockwise = GUILayout.Toggle(Datable.Clockwise, "");
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("PreserveAspect");
                    GUILayout.FlexibleSpace();
                    Datable.PreserveAspect = GUILayout.Toggle(Datable.PreserveAspect, "");
                    GUILayout.EndHorizontal();
                }
            }
        }

        public Sprite ExportSpriteAssetFile(string path)
        {
            string spPath = path + "/" + sprite.name + ".png";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            if (File.Exists(spPath))
            {
                File.Delete(spPath);
            }
            File.WriteAllBytes(spPath, sprite.texture.EncodeToPNG());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            string[] temp = spPath.Split("Assets");
            if (temp.Length < 2)
            {
                throw new System.Exception("path error:" + path);
            }
            path = "Assets/" + temp[1];
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.compressionQuality = 100;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(importer.assetPath);
        }
        public override void GenerateComponent(GameObject target, string savedAssetPath)
        {
            if (target.GetComponent<UnityEngine.UI.Image>() != null)
            {
                return;
            }
            UnityEngine.UI.Image image = target.AddComponent<UnityEngine.UI.Image>();
            image.sprite = ExportSpriteAssetFile(Path.GetDirectoryName(savedAssetPath) + "/sprite");
            image.raycastTarget = Datable.RaycastTarget;
            image.maskable = Datable.Maskble;
            image.type = Datable.imageType;
            image.fillMethod = Datable.fillMethod;
            image.fillAmount = Datable.fillamount;
            image.useSpriteMesh = Datable.useSpriteMesh;
            image.fillClockwise = Datable.Clockwise;
            image.preserveAspect = Datable.PreserveAspect;
            image.fillCenter = Datable.FillCenter;
            image.pixelsPerUnitMultiplier = Datable.PixelsPerUnitMultiplier;
            image.color = Datable.color;
            image.material = material;
        }
    }
}