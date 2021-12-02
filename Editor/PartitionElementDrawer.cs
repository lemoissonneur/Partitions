using UnityEngine;
using UnityEditor;


namespace CobayeStudio
{
    [CustomPropertyDrawer(typeof(PartitionBase.ElementBase), true)]
    public class PartitionElementDrawer : PropertyDrawer
    {
        private Rect IndexLabelRect(Rect position) => new Rect(position)
        {
            height = EditorGUIUtility.singleLineHeight,
            width = EditorGUIUtility.singleLineHeight,
        };

        private Rect ColorRect(Rect position) => new Rect(position)
        {
            height = EditorGUIUtility.singleLineHeight,
            x = IndexLabelRect(position).xMax + EditorGUIUtility.standardVerticalSpacing,
            width = EditorGUIUtility.singleLineHeight * 3,
        };

        private Rect ValueRect(Rect position) => new Rect(position)
        {
            height = EditorGUIUtility.singleLineHeight,
            x = ColorRect(position).xMax + EditorGUIUtility.standardVerticalSpacing,
            width = EditorGUIUtility.singleLineHeight * 5,
        };

        private Rect ObjectRect(Rect position)
        {
            if (position.height > (EditorGUIUtility.singleLineHeight + 2))
            {
                return new Rect(position)
                {
                    yMin = position.yMin + 1 + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight,
                    yMax = position.yMax - 1,
                };
            }
            else
            {
                return new Rect(position)
                {
                    height = EditorGUIUtility.singleLineHeight,
                    xMin = ValueRect(position).xMax + EditorGUIUtility.singleLineHeight,
                };
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty objectProp = property.FindPropertyRelative("Object");
            
            float objectPropHeight = objectProp != null ?
                EditorGUI.GetPropertyHeight(objectProp) :
                EditorGUIUtility.singleLineHeight;

            float height = objectPropHeight > EditorGUIUtility.singleLineHeight ?
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + objectPropHeight :
                EditorGUIUtility.singleLineHeight;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // get properties
            SerializedProperty colorProp = property.FindPropertyRelative("Color");
            SerializedProperty valueProp = property.FindPropertyRelative("Value");
            SerializedProperty objectProp = property.FindPropertyRelative("Object");
            int index = GetIndexFromPath(property.propertyPath);

            // draw index, color and value
            EditorGUI.LabelField(IndexLabelRect(position), index.ToString());

            EditorGUI.PropertyField(ColorRect(position), colorProp, GUIContent.none);

            EditorGUI.PropertyField(ValueRect(position), valueProp, GUIContent.none);

            // draw generic field if needed
            if (objectProp != null)
            {
                EditorGUI.PropertyField(ObjectRect(position), objectProp, GUIContent.none, true);
            }
        }

        public static int GetIndexFromPath(string path)
        {
            int startIndex = path.LastIndexOf('[') + 1;
            int length = path.LastIndexOf(']') - startIndex;

            if (startIndex < 0 || length < 1)
                return -1;

            string indexStr = path.Substring(startIndex, length);

            if (int.TryParse(indexStr, out int index))
                return index;
            else return -1;
        }
    }
}
