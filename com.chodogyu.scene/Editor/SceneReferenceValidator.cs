using System;
using CDG.Core.Results;
using UnityEditor;

namespace CDG.Scene.Editor
{
    /// <summary>
    /// SceneReference가 실제 Scene Asset을 가리키고 있으며 현재 Build에 사용할 수 있도록 등록되어 있는지 검사합니다.
    /// 검증 과정에서는 SceneReference의 저장된 경로를 수정하거나 정규화하지 않습니다.
    /// </summary>
    public static class SceneReferenceValidator
    {
        /// <summary>
        /// 지정된 SceneReference의 Asset 존재 여부와 Build 등록 상태를 검사합니다.
        /// 유효하지 않은 참조 또는 Build에 포함되지 않은 Scene은 실패 결과를 반환합니다.
        /// </summary>
        public static Result Validate(SceneReference scene)
        {
            if (scene.IsEmpty)
            {
                return Result.Failure(new ResultError(SceneErrorCodes.InvalidReference, "Scene reference is empty."));
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.Path);

            if (sceneAsset == null)
            {
                return Result.Failure(new ResultError(SceneErrorCodes.InvalidReference, "The referenced scene asset does not exist."));
            }

            if (!IsSceneEnabledInBuild(scene.Path))
            {
                return Result.Failure(new ResultError(SceneErrorCodes.NotInBuild, "The scene is not enabled in the current build settings."));
            }

            return Result.Success();
        }

        private static bool IsSceneEnabledInBuild(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene buildScene = scenes[i];

                if (buildScene.enabled && string.Equals(buildScene.path, scenePath, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}