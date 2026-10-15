using UnityEditor;
using Templates = Mane.Unity.Editor.ScriptTemplates;

namespace Mane.Unity.UI.Editor
{
    internal static class ScriptTemplates
    {
        // Built-in scripting templates use priorities 1-3. Stay in that group, ahead of Assembly Definition at 20.
        private const int UIBehaviourPriority = 4;
        private const int ManeUIBehaviourPriority = 5;

        [MenuItem("Assets/Create/Scripting/UIBehaviour", false, UIBehaviourPriority)]
        private static void CreateUIBehaviour() =>
            Templates.CreateScript(typeof(ScriptTemplates).Assembly, "UIBehaviour.cs.txt", "NewUIBehaviourScript.cs");

        [MenuItem("Assets/Create/Scripting/ManeUIBehaviour", false, ManeUIBehaviourPriority)]
        private static void CreateManeUIBehaviour() =>
            Templates.CreateScript(typeof(ScriptTemplates).Assembly, "ManeUIBehaviour.cs.txt",
                "NewManeUIBehaviourScript.cs");
    }
}
