
using UnityEngine;
using UnityEditor;

namespace UixFramework
{  
    public class InspectorBase : Editor
    {

        public virtual bool DrawButton(string name)
        {
            return GUILayout.Button(name);
        }
    }

}