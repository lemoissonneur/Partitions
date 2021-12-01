using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace CobayeStudio
{
    [CustomPropertyDrawer(typeof(PartitionBase), true)]
    public class PartitionDrawer : PropertyDrawer
    {
        private Rect GraphRect(Rect position) => new Rect(position)
        {
            yMin = position.yMin + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
            height = PartitionGUI.SliderbarHeight,
        };

        private Rect ListRect(Rect position) => new Rect(position)
        {
            yMin = position.yMin + EditorGUIUtility.singleLineHeight + PartitionGUI.SliderbarHeight + EditorGUIUtility.standardVerticalSpacing * 2
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty listProperty = property.FindPropertyRelative("Elements");

            float listHeight = EditorGUI.GetPropertyHeight(listProperty);

            return EditorGUIUtility.singleLineHeight + PartitionGUI.SliderbarHeight + EditorGUIUtility.standardVerticalSpacing * 2 + listHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty listProperty = property.FindPropertyRelative("Elements");

            EditorGUI.PrefixLabel(position, label, EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            if (listProperty.arraySize > 0)
            {
                List<Element> elements = new List<Element>();

                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    elements.Add(new Element(listProperty.GetArrayElementAtIndex(i), i));
                }

                PartitionGUI.HandlePartitionGUI(EditorGUI.IndentedRect(GraphRect(position)), elements);
            }
            else
            {
                EditorGUI.LabelField(EditorGUI.IndentedRect(GraphRect(position)), "empty", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(ListRect(position), listProperty, true);

            EditorGUI.indentLevel-=2;
        }
    }

    public class Element
    {
        public SerializedProperty Property { get; }
        public Color Color { get; }
        public string Name { get; }
        public float Value
        {
            get
            {
                return Property.FindPropertyRelative("Value").floatValue;
            }

            set
            {
                Property.FindPropertyRelative("Value").floatValue = value;
                Property.serializedObject.ApplyModifiedProperties();
            }
        }

        public Element(SerializedProperty property, int index)
        {
            Property = property;

            Color = Property.FindPropertyRelative("Color").colorValue;

            SerializedProperty Object = Property.FindPropertyRelative("Object");

            Name = Object != null
                && Object.propertyType == SerializedPropertyType.ObjectReference
                && Object.objectReferenceValue != null?
                Object.objectReferenceValue.name : index.ToString();
        }
    }


    /// <summary>
    /// This is an Edit of unity's ShadowCascadeSplitGUI class from
    /// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/ShadowCascadeSplitGUI.cs
    /// </summary>
    internal static class PartitionGUI
    {
        public static int SliderbarHeight => (int)EditorGUIUtility.singleLineHeight * 2;

        private const int kSliderbarTopMargin = 0;
        private const int kPartitionHandleWidth = 2;
        private const int kPartitionHandleExtraHitAreaWidth = 2;

        // using a LODGroup skin
        private static readonly GUIStyle s_CascadeSliderBG = "LODSliderRange";
        private static readonly GUIStyle s_TextCenteredStyle = new GUIStyle(EditorStyles.whiteMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };

        // Internal struct to bundle drag information
        private class DragCache
        {
            public int m_ActivePartition;          // the cascade partition that we are currently dragging/resizing
            public float m_NormalizedPartitionSize;  // the normalized size of the partition (0.0f < size < 1.0f)
            public Vector2 m_LastCachedMousePosition;  // mouse position the last time we registered a drag or mouse down.

            public DragCache(int activePartition, float normalizedPartitionSize, Vector2 currentMousePos)
            {
                m_ActivePartition = activePartition;
                m_NormalizedPartitionSize = normalizedPartitionSize;
                m_LastCachedMousePosition = currentMousePos;
            }
        }
        private static DragCache s_DragCache;

        private static readonly int s_CascadeSliderId = "s_CascadeSliderId".GetHashCode();

        private static SceneView s_RestoreSceneView;
        private static SceneView.CameraMode s_OldSceneDrawMode;
        private static bool s_OldSceneLightingMode;


        /// <summary>
        /// Static function to handle the GUI and User input related to the slider.
        /// </summary>
        /// <param name="position">Rect to display the slider</param>
        /// <param name="elements">elements in the partitions</param>
        public static void HandlePartitionGUI(Rect position, List<Element> elements)
        {
            float currentX = position.x;
            float cascadeBoxStartY = position.y + kSliderbarTopMargin;
            float cascadeSliderWidth = position.width - (elements.Count * kPartitionHandleWidth);
            Color origTextColor = GUI.color;
            Color origBackgroundColor = GUI.backgroundColor;

            // check for user input on any of the partition handles
            // this mechanism gets the current event in the queue... make sure that the mouse is over our control before consuming the event
            int sliderControlId = GUIUtility.GetControlID(s_CascadeSliderId, FocusType.Passive);
            Event currentEvent = Event.current;
            int hotPartitionHandleIndex = -1; // the index of any partition handle that we are hovering over or dragging

            // draw each cascade partition
            for (int i = 0; i < elements.Count; ++i)
            {
                Element currentElement = elements[i];

                GUI.backgroundColor = currentElement.Color;
                float boxLength = (cascadeSliderWidth * currentElement.Value);

                // main cascade box
                Rect partitionRect = new Rect(currentX, cascadeBoxStartY, boxLength, SliderbarHeight);
                GUI.Box(partitionRect, GUIContent.none, s_CascadeSliderBG);
                currentX += boxLength;

                // cascade box percentage text
                GUI.color = Color.white;
                Rect textRect = partitionRect;
                var cascadeText = String.Format("{0}\n{1:F1}%", i, currentElement.Value * 100.0f);
                var tooltipText = String.Format("{0}\n{1:F1}%", currentElement.Name, currentElement.Value * 100.0f);

                GUI.Label(textRect, new GUIContent(cascadeText, tooltipText), s_TextCenteredStyle);

                // no need to draw the partition handle for last box
                if (i == elements.Count - 1)
                    break;

                // partition handle
                GUI.backgroundColor = Color.black;
                Rect handleRect = partitionRect;
                handleRect.x = currentX;
                handleRect.width = kPartitionHandleWidth;
                GUI.Box(handleRect, GUIContent.none, s_CascadeSliderBG);
                // we want a thin handle visually (since wide black bar looks bad), but a slightly larger
                // hit area for easier manipulation
                Rect handleHitRect = handleRect;
                handleHitRect.xMin -= kPartitionHandleExtraHitAreaWidth;
                handleHitRect.xMax += kPartitionHandleExtraHitAreaWidth;
                if (handleHitRect.Contains(currentEvent.mousePosition))
                    hotPartitionHandleIndex = i;

                // add regions to slider where the cursor changes to Resize-Horizontal
                if (s_DragCache == null)
                {
                    EditorGUIUtility.AddCursorRect(handleHitRect, MouseCursor.ResizeHorizontal, sliderControlId);
                }

                currentX += kPartitionHandleWidth;
            }

            GUI.color = origTextColor;
            GUI.backgroundColor = origBackgroundColor;

            EventType eventType = currentEvent.GetTypeForControl(sliderControlId);
            switch (eventType)
            {
                case EventType.MouseDown:
                    if (hotPartitionHandleIndex >= 0)
                    {
                        s_DragCache = new DragCache(hotPartitionHandleIndex, elements[hotPartitionHandleIndex].Value, currentEvent.mousePosition);
                        if (GUIUtility.hotControl == 0)
                            GUIUtility.hotControl = sliderControlId;
                        currentEvent.Use();

                        // Switch active scene view into shadow cascades visualization mode, once we start
                        // tweaking cascade splits.
                        if (s_RestoreSceneView == null)
                        {
                            s_RestoreSceneView = SceneView.lastActiveSceneView;
                            if (s_RestoreSceneView != null)
                            {
                                s_OldSceneDrawMode = s_RestoreSceneView.cameraMode;
                                s_OldSceneLightingMode = s_RestoreSceneView.sceneLighting;
                                s_RestoreSceneView.cameraMode = SceneView.GetBuiltinCameraMode(DrawCameraMode.ShadowCascades);
                            }
                        }
                    }
                    break;

                case EventType.MouseUp:
                    // mouseUp event anywhere should release the hotcontrol (if it belongs to us), drags (if any)
                    if (GUIUtility.hotControl == sliderControlId)
                    {
                        GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                    s_DragCache = null;

                    // Restore previous scene view drawing mode once we stop tweaking cascade splits.
                    if (s_RestoreSceneView != null)
                    {
                        s_RestoreSceneView.cameraMode = s_OldSceneDrawMode;
                        s_RestoreSceneView.sceneLighting = s_OldSceneLightingMode;
                        s_RestoreSceneView = null;
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != sliderControlId)
                        break;

                    // convert the mouse movement to normalized cascade width. Make sure that we are safe to apply the delta before using it.
                    float delta = (currentEvent.mousePosition - s_DragCache.m_LastCachedMousePosition).x / cascadeSliderWidth;
                    bool isLeftPartitionHappy = ((elements[s_DragCache.m_ActivePartition].Value + delta) > 0.0f);
                    bool isRightPartitionHappy = ((elements[s_DragCache.m_ActivePartition + 1].Value - delta) > 0.0f);
                    if (isLeftPartitionHappy && isRightPartitionHappy)
                    {
                        s_DragCache.m_NormalizedPartitionSize += delta;
                        elements[s_DragCache.m_ActivePartition].Value = s_DragCache.m_NormalizedPartitionSize;
                        if (s_DragCache.m_ActivePartition < elements.Count - 1)
                            elements[s_DragCache.m_ActivePartition + 1].Value -= delta;
                        GUI.changed = true;
                    }
                    s_DragCache.m_LastCachedMousePosition = currentEvent.mousePosition;
                    currentEvent.Use();
                    break;
            }

            // data correction
            float[] values = new float[elements.Count];
            for (int i = 0; i < elements.Count; i++) values[i] = elements[i].Value;

            // correct values with the rule
            PartitionManagement.CorrectPartition(values, s_DragCache.m_ActivePartition, PartitionEditRule.AdjustLeftAndRight);

            // re applly corrected values
            for (int i = 0; i < elements.Count; i++) elements[i].Value = values[i];
        }
    }
}
