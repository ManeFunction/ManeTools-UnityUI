using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Mane.Unity.UI.Editor
{
    internal static class ScriptTemplates
    {
        // Built-in scripting templates use priorities 1-3. Stay in that group, ahead of Assembly Definition at 20.
        private const int UIBehaviourPriority = 4;
        private const int ManeUIBehaviourPriority = 5;

        [MenuItem("Assets/Create/Scripting/UIBehaviour", false, UIBehaviourPriority)]
        private static void CreateUIBehaviour() =>
            CreateScript("UIBehaviour.cs.txt", "NewUIBehaviourScript.cs");

        [MenuItem("Assets/Create/Scripting/ManeUIBehaviour", false, ManeUIBehaviourPriority)]
        private static void CreateManeUIBehaviour() =>
            CreateScript("ManeUIBehaviour.cs.txt", "NewManeUIBehaviourScript.cs");

        private static void CreateScript(string templateFileName, string defaultFileName)
        {
            string templatePath = TemplatePath(templateFileName);
            if (templatePath == null)
                return;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultFileName);
        }

        private static string TemplatePath(string templateFileName)
        {
            PackageInfo packageInfo = PackageInfo.FindForAssembly(typeof(ScriptTemplates).Assembly);
            if (packageInfo == null)
            {
                Debug.LogError("Could not resolve the Mane Tools for Unity UI package path for script templates.");
                return null;
            }

            return Path.Combine(packageInfo.resolvedPath, "Editor", "ScriptTemplates", templateFileName);
        }
    }
}
