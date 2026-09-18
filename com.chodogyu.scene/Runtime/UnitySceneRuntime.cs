using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace CDG.Scene
{
    /// <summary>
    /// Unity SceneManager를 사용하여 Scene 조회, 로드, 언로드 및 Active Scene 변경을 수행합니다.
    /// Unity Scene API 사용을 Scene Framework의 내부 Runtime 인터페이스 뒤로 분리합니다.
    /// </summary>
    internal sealed class UnitySceneRuntime : ISceneRuntime
    {
        /// <inheritdoc/>
        public bool IsSceneInBuild(SceneReference scene)
        {
            if (scene.IsEmpty)
            {
                return false;
            }

            return SceneUtility.GetBuildIndexByScenePath(scene.Path) >= 0;
        }

        /// <inheritdoc/>
        public bool IsSceneLoaded(SceneReference scene)
        {
            if (scene.IsEmpty)
            {
                return false;
            }

            UnityScene loadedScene = SceneManager.GetSceneByPath(scene.Path);
            return loadedScene.IsValid() && loadedScene.isLoaded;
        }

        /// <inheritdoc/>
        public SceneReference GetActiveScene()
        {
            UnityScene activeScene = SceneManager.GetActiveScene();
            return new SceneReference(activeScene.path);
        }

        /// <inheritdoc/>
        public ISceneAsyncOperation LoadSingleAsync(SceneReference scene)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Single);

            if (operation == null)
            {
                return null;
            }

            return new UnitySceneAsyncOperation(operation);
        }

        /// <inheritdoc/>
        public ISceneAsyncOperation LoadAdditiveAsync(SceneReference scene)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);

            if (operation == null)
            {
                return null;
            }

            return new UnitySceneAsyncOperation(operation);
        }

        /// <inheritdoc/>
        public ISceneAsyncOperation UnloadAsync(SceneReference scene)
        {
            UnityScene loadedScene = SceneManager.GetSceneByPath(scene.Path);

            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                return null;
            }

            AsyncOperation operation = SceneManager.UnloadSceneAsync(loadedScene);

            if (operation == null)
            {
                return null;
            }

            return new UnitySceneAsyncOperation(operation);
        }

        /// <inheritdoc/>
        public bool SetActiveScene(SceneReference scene)
        {
            UnityScene loadedScene = SceneManager.GetSceneByPath(scene.Path);

            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                return false;
            }

            return SceneManager.SetActiveScene(loadedScene);
        }
    }
}