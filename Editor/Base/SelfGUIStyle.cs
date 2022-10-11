using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace GameEditor
{
    public class SelfGUIStyle
    {
        public GUIStyle line;
        public GUIStyle item;
        public GUIStyle delItem;
        public GUIStyle newItem;
        public GUIStyle label;
        public GUIStyle delButton;

        public Texture2D blackTex;
        public Texture2D grayTex;
        public Texture2D radTex;
        public Texture2D greenTex;

        public SelfGUIStyle()
        {
            blackTex = CreateTexture(new Color(0.12f, 0.12f, 0.12f, 1));
            grayTex = CreateTexture(new Color(0.3f, 0.3f, 0.3f, 1));
            radTex = CreateTexture(new Color(0.6f, 0.3f, 0.3f, 1));
            greenTex = CreateTexture(new Color(0.3f, 0.5f, 0.4f, 1));

            line = new GUIStyle();
            line.alignment = TextAnchor.MiddleLeft;
            line.normal = new GUIStyleState { background = blackTex };

            item = new GUIStyle();
            item.alignment = TextAnchor.MiddleLeft;
            item.normal = new GUIStyleState { background = grayTex };

            delItem = new GUIStyle();
            delItem.alignment = TextAnchor.MiddleLeft;
            delItem.normal = new GUIStyleState { background = radTex };

            newItem = new GUIStyle();
            newItem.alignment = TextAnchor.MiddleLeft;
            newItem.normal = new GUIStyleState { background = greenTex };

            label = new GUIStyle();
            label.normal = new GUIStyleState { textColor = Color.white };

            delButton = new GUIStyle();
            delButton.normal = new GUIStyleState { textColor = Color.red };

        }

        private Texture2D CreateTexture(Color color)
        {
            Texture2D t2d = new Texture2D(1, 1);
            t2d.SetPixel(0, 0, color);
            t2d.Apply();
            return t2d;
        }
    }
}
