using UnityEditor;
using UnityEditor.UI;

namespace Mane.Unity.UI.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ThreeStatesToggle), true)]
    public class ThreeStatesToggleEditor : SelectableEditor
    {
        private SerializedProperty _onValueChangedProperty;
        private SerializedProperty _transitionProperty;
        private SerializedProperty _graphicProperty;
        private SerializedProperty _offGraphicProperty;
        private SerializedProperty _undefinedGraphicProperty;
        private SerializedProperty _stateProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            _transitionProperty = serializedObject.FindProperty(ThreeStatesToggle.TransitionPropertyName);
            _graphicProperty = serializedObject.FindProperty(ThreeStatesToggle.GraphicPropertyName);
            _offGraphicProperty = serializedObject.FindProperty(ThreeStatesToggle.OffGraphicPropertyName);
            _undefinedGraphicProperty = serializedObject.FindProperty(ThreeStatesToggle.UndefinedGraphicPropertyName);
            _stateProperty = serializedObject.FindProperty(ThreeStatesToggle.StatePropertyName);
            _onValueChangedProperty = serializedObject.FindProperty(ThreeStatesToggle.OnStateValueChangedPropertyName);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_stateProperty);
            EditorGUILayout.PropertyField(_transitionProperty);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(_graphicProperty);
            EditorGUILayout.PropertyField(_offGraphicProperty);
            EditorGUILayout.PropertyField(_undefinedGraphicProperty);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_onValueChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
