using UnityEditor;
using UnityEngine;

namespace CDG.Scene.Editor
{
    /// <summary>
    /// SceneReference를 Inspector에서 Scene Asset으로 선택할 수 있도록 표시합니다.
    /// 선택된 Scene의 전체 Asset Path를 SceneReference 내부 직렬화 값으로 저장합니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReference))]
    public sealed class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty pathProperty = property.FindPropertyRelative("path");

            if (pathProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "SceneReference path property not found.");
                EditorGUI.EndProperty();
                return;
            }

            SceneAsset currentScene = string.IsNullOrEmpty(pathProperty.stringValue)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(pathProperty.stringValue);

            EditorGUI.BeginChangeCheck();

            SceneAsset selectedScene = (SceneAsset)EditorGUI.ObjectField(position, label, currentScene, typeof(SceneAsset), false);

            if (EditorGUI.EndChangeCheck())
            {
                pathProperty.stringValue = selectedScene == null
                    ? string.Empty
                    : AssetDatabase.GetAssetPath(selectedScene);
            }

            EditorGUI.EndProperty();
        }
    }
}