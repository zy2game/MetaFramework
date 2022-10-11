using GameFramework.Editor.UIGenerator;
using UnityEditor;

namespace GameFramework.Editor
{
    public static class EditorMenu
    {
        [MenuItem("Game/UI Generate %`")]
        public static void ShowEditor()
        {
            UIGeneratorWindow.GetWindow<UIGeneratorWindow>("UI Generate", true).Show();
        }
    }
}