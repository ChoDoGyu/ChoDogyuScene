using System;
using CDG.Scene.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CDG.Scene.Tests.Editor
{
    public sealed class SceneReferenceValidatorTests
    {
        private const string TempFolderPath = "Assets/CDGSceneTests";
        private const string TempScenePath = TempFolderPath + "/ValidationScene.unity";

        private EditorBuildSettingsScene[] originalBuildScenes;
        private SceneSetup[] originalSceneSetup;
        private bool canRestoreOriginalSceneSetup;
        private bool tempSceneCreated;

        [SetUp]
        public void SetUp()
        {
            originalBuildScenes = EditorBuildSettings.scenes;
            originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();
            canRestoreOriginalSceneSetup = CanRestoreSceneSetup(originalSceneSetup);
            tempSceneCreated = false;
        }

        [TearDown]
        public void TearDown()
        {
            EditorBuildSettings.scenes = originalBuildScenes;

            if (tempSceneCreated)
            {
                if (canRestoreOriginalSceneSetup)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
                }
                else
                {
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }
            }

            if (AssetDatabase.IsValidFolder(TempFolderPath))
            {
                AssetDatabase.DeleteAsset(TempFolderPath);
            }

            AssetDatabase.Refresh();
        }

        [Test]
        public void Validate_WithEmptySceneReference_ReturnsInvalidReference()
        {
            var result = SceneReferenceValidator.Validate(default);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.InvalidReference));
        }

        [Test]
        public void Validate_WithMissingSceneAsset_ReturnsInvalidReference()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/MissingScene.unity");

            var result = SceneReferenceValidator.Validate(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.InvalidReference));
        }

        [Test]
        public void Validate_WithSceneNotInBuild_ReturnsNotInBuild()
        {
            CreateTempScene();

            EditorBuildSettings.scenes = Array.Empty<EditorBuildSettingsScene>();

            SceneReference scene = new SceneReference(TempScenePath);

            var result = SceneReferenceValidator.Validate(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.NotInBuild));
        }

        [Test]
        public void Validate_WithDisabledSceneInBuild_ReturnsNotInBuild()
        {
            CreateTempScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(TempScenePath, false)
            };

            SceneReference scene = new SceneReference(TempScenePath);

            var result = SceneReferenceValidator.Validate(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.NotInBuild));
        }

        [Test]
        public void Validate_WithEnabledSceneInBuild_ReturnsSuccess()
        {
            CreateTempScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(TempScenePath, true)
            };

            SceneReference scene = new SceneReference(TempScenePath);

            var result = SceneReferenceValidator.Validate(scene);

            Assert.That(result.IsSuccess, Is.True);
        }

        private void CreateTempScene()
        {
            if (!AssetDatabase.IsValidFolder(TempFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "CDGSceneTests");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            tempSceneCreated = true;

            bool saved = EditorSceneManager.SaveScene(scene, TempScenePath);

            Assert.That(saved, Is.True);

            AssetDatabase.Refresh();
        }

        private static bool CanRestoreSceneSetup(SceneSetup[] sceneSetup)
        {
            if (sceneSetup == null || sceneSetup.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < sceneSetup.Length; i++)
            {
                if (string.IsNullOrEmpty(sceneSetup[i].path))
                {
                    return false;
                }
            }

            return true;
        }
    }
}