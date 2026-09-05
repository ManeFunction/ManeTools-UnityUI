using Mane.Unity.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.UI.Editor
{
    [CustomEditor(typeof(ColorSchemeComponent), true)]
    public sealed class ColorSchemeComponentEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset xml;
        [SerializeField] private VisualTreeAsset rowXml;

        private VisualElement _root;
        private VisualElement _colorsContainer;
        private VisualElement _emptyBox;
        private VisualElement _schemeTracker;
        private int _builtColorCount = -1;
        private bool _rebuilding;

        private void OnEnable()
        {
            if (target is ColorSchemeComponent component)
                component.Refresh();
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            ManeEditorStyles.Apply(root);
            xml.CloneTree(root);
            _root = root;

            _emptyBox = root.Q<VisualElement>("emptyBox");
            _colorsContainer = root.Q<VisualElement>("colorsContainer");
            if (_emptyBox == null || _colorsContainer == null)
            {
                Debug.LogError("ColorSchemeComponentEditor UXML is missing expected elements.");
                return root;
            }

            SerializedProperty schemeProp =
                serializedObject.FindProperty(ColorSchemeComponent.ColorSchemePropertyName);

            RebuildColors();
            root.TrackPropertyValue(schemeProp, _ => RebuildColors());
            root.TrackSerializedObjectValue(serializedObject, _ =>
            {
                if (_rebuilding)
                    return;

                root.schedule.Execute(RefreshTarget);
            });

            return root;
        }

        private void RebuildColors()
        {
            if (_rebuilding || _colorsContainer == null)
                return;

            _rebuilding = true;
            try
            {
                serializedObject.UpdateIfRequiredOrScript();
                _colorsContainer.Clear();

                ColorScheme scheme = GetScheme();
                if (scheme == null)
                {
                    _builtColorCount = 0;
                    _emptyBox.style.display = DisplayStyle.Flex;
                    _colorsContainer.style.display = DisplayStyle.None;
                    return;
                }

                _emptyBox.style.display = DisplayStyle.None;
                _colorsContainer.style.display = DisplayStyle.Flex;

                SerializedProperty graphic =
                    serializedObject.FindProperty(ColorSchemeComponent.GraphicPropertyName);

                bool sizeChanged = false;
                while (graphic.arraySize < scheme.Length)
                {
                    graphic.InsertArrayElementAtIndex(graphic.arraySize);
                    sizeChanged = true;
                }

                while (graphic.arraySize > scheme.Length)
                {
                    graphic.DeleteArrayElementAtIndex(graphic.arraySize - 1);
                    sizeChanged = true;
                }

                if (sizeChanged)
                {
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.UpdateIfRequiredOrScript();
                }

                _builtColorCount = scheme.Length;

                for (int i = 0; i < scheme.Length; i++)
                {
                    SerializedProperty graphicArray = graphic.GetArrayElementAtIndex(i)
                        .FindPropertyRelative(ColorSchemeComponent.GraphicPropertyName);

                    if (graphicArray.arraySize == 0)
                    {
                        graphicArray.InsertArrayElementAtIndex(0);
                        serializedObject.ApplyModifiedProperties();
                    }

                    VisualElement group = new();
                    group.AddToClassList("color-group");
                    _colorsContainer.Add(group);

                    int last = graphicArray.arraySize - 1;
                    for (int j = 0; j < graphicArray.arraySize; j++)
                        AddRow(group, scheme, i, graphicArray, j, last);
                }
            }
            finally
            {
                _rebuilding = false;
                RebindSchemeLengthTracking();
                ManeEditorStyles.RefreshFieldLayout(_root);
            }
        }

        private void RebindSchemeLengthTracking()
        {
            _schemeTracker?.RemoveFromHierarchy();
            _schemeTracker = null;

            ColorScheme scheme = GetScheme();
            if (scheme == null || _root == null)
                return;

            SerializedObject schemeSo = new(scheme);
            SerializedProperty colors = schemeSo.FindProperty(ColorScheme.ColorsPropertyName);
            if (colors == null)
                return;

            _schemeTracker = new VisualElement { name = "schemeTracker" };
            _schemeTracker.style.display = DisplayStyle.None;
            _root.Add(_schemeTracker);
            _schemeTracker.TrackPropertyValue(colors, property =>
            {
                if (property.arraySize != _builtColorCount)
                    RebuildColors();
            });
        }

        private void AddRow(VisualElement group, ColorScheme scheme, int colorIndex,
            SerializedProperty graphicArray, int graphicIndex, int lastIndex)
        {
            if (rowXml == null)
            {
                Debug.LogError("ColorGraphicRow UXML not found.");
                return;
            }

            rowXml.CloneTree(group);
            VisualElement row = group[group.childCount - 1];

            VisualElement lead = row.Q<VisualElement>("lead");
            VisualElement graphicSlot = row.Q<VisualElement>("graphicField");
            Button rowButton = row.Q<Button>("rowButton");
            if (lead == null || graphicSlot == null || rowButton == null)
            {
                Debug.LogError("ColorGraphicRow UXML is missing expected elements.");
                return;
            }

            ObjectField graphicField = new()
            {
                label = string.Empty,
                objectType = typeof(MaskableGraphic),
                allowSceneObjects = true
            };
            graphicField.AddToClassList("graphic-field");
            graphicSlot.Add(graphicField);

            string propertyPath = graphicArray.GetArrayElementAtIndex(graphicIndex).propertyPath;
            graphicField.bindingPath = propertyPath;
            graphicField.Bind(serializedObject);
            EnableGameObjectDrop(graphicField, propertyPath);
            rowButton.BringToFront();

            if (graphicIndex == 0)
            {
                ColorField colorField = new()
                {
                    label = string.Empty,
                    showAlpha = true,
                    showEyeDropper = true,
                    hdr = false,
                    value = scheme[colorIndex]
                };
                colorField.RegisterValueChangedCallback(evt => SetColor(colorIndex, evt.newValue));
                lead.Add(colorField);

                rowButton.text = "+";
                rowButton.clicked += () => AddGraphic(colorIndex);
            }
            else
            {
                Label branch = new(graphicIndex == lastIndex ? "\u2514" : "\u251c");
                branch.AddToClassList("branch-label");
                lead.Add(branch);

                rowButton.text = "-";
                rowButton.clicked += () => RemoveGraphic(colorIndex, graphicIndex);
            }
        }

        private void AddGraphic(int colorIndex)
        {
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty graphicArray = GetGraphicArray(colorIndex);
            if (graphicArray.arraySize == 0)
                graphicArray.InsertArrayElementAtIndex(0);

            // Inserting at the end is a no-op while the last Object reference is null.
            int insertAt = graphicArray.arraySize - 1;
            graphicArray.InsertArrayElementAtIndex(insertAt);
            graphicArray.GetArrayElementAtIndex(graphicArray.arraySize - 1).objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            RebuildColors();
        }

        private void RemoveGraphic(int colorIndex, int graphicIndex)
        {
            serializedObject.UpdateIfRequiredOrScript();
            GetGraphicArray(colorIndex).DeleteArrayElementAtIndex(graphicIndex);
            serializedObject.ApplyModifiedProperties();
            RebuildColors();
        }

        private void SetColor(int colorIndex, Color color)
        {
            ColorScheme scheme = GetScheme();
            if (scheme == null)
                return;

            SerializedObject schemeSo = new(scheme);
            SerializedProperty colors = schemeSo.FindProperty(ColorScheme.ColorsPropertyName);
            if (colors == null || colorIndex < 0 || colorIndex >= colors.arraySize)
                return;

            colors.GetArrayElementAtIndex(colorIndex).colorValue = color;
            schemeSo.ApplyModifiedProperties();
            RefreshTarget();
        }

        private void EnableGameObjectDrop(ObjectField field, string propertyPath)
        {
            field.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (FindDraggedGraphic() == null)
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);

            field.RegisterCallback<DragPerformEvent>(evt =>
            {
                MaskableGraphic graphic = FindDraggedGraphic();
                if (graphic == null)
                    return;

                DragAndDrop.AcceptDrag();
                AssignGraphic(propertyPath, graphic);
                field.SetValueWithoutNotify(graphic);
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);
        }

        private void AssignGraphic(string propertyPath, MaskableGraphic graphic)
        {
            serializedObject.UpdateIfRequiredOrScript();
            serializedObject.FindProperty(propertyPath).objectReferenceValue = graphic;
            serializedObject.ApplyModifiedProperties();
            RefreshTarget();
        }

        private void RefreshTarget()
        {
            if (target is ColorSchemeComponent component)
                component.Refresh();
        }

        private static MaskableGraphic FindDraggedGraphic()
        {
            UnityObject[] refs = DragAndDrop.objectReferences;
            if (refs == null || refs.Length == 0)
                return null;

            return FindGraphic(refs[0]);
        }

        private static MaskableGraphic FindGraphic(UnityObject obj) =>
            obj switch
            {
                MaskableGraphic graphic => graphic,
                GameObject go => go.GetComponent<MaskableGraphic>(),
                Component component => component.GetComponent<MaskableGraphic>(),
                _ => null
            };

        private SerializedProperty GetGraphicArray(int colorIndex)
        {
            return serializedObject.FindProperty(ColorSchemeComponent.GraphicPropertyName)
                .GetArrayElementAtIndex(colorIndex)
                .FindPropertyRelative(ColorSchemeComponent.GraphicPropertyName);
        }

        private ColorScheme GetScheme()
        {
            SerializedProperty schemeProp =
                serializedObject.FindProperty(ColorSchemeComponent.ColorSchemePropertyName);
            return schemeProp.objectReferenceValue as ColorScheme;
        }
    }
}
