using CDG.Core.Results;
using CDG.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CDG.Scene.Samples.BasicUsage
{
    /// <summary>
    /// ChoDogyu Scene & Loading Framework의 기본 Runtime 사용 흐름을 직접 확인할 수 있는 Sample입니다.
    /// Single Load, Additive Load, Active Scene 변경 및 Unload를 하나의 실행 흐름에서 확인할 수 있습니다.
    /// </summary>
    public sealed class BasicSceneUsage : MonoBehaviour
    {
        private static BasicSceneUsage instance;

        private SceneController controller;
        private SceneReference sceneA;
        private SceneReference sceneB;
        private SceneReference additiveScene;

        /// <summary>
        /// 이 Sample에서 사용하는 SceneController를 반환합니다.
        /// </summary>
        public SceneController Controller => controller;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            InitializeSceneReferences();

            controller = new SceneController();
            controller.OperationStarted += OnOperationStarted;
            controller.OperationCompleted += OnOperationCompleted;

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.OperationStarted -= OnOperationStarted;
                controller.OperationCompleted -= OnOperationCompleted;
            }

            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnGUI()
        {
            if (controller == null)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(20f, 20f, 420f, 500f), GUI.skin.box);

            GUILayout.Label("ChoDogyu Scene & Loading Framework - Basic Usage");
            GUILayout.Space(10f);

            DrawSingleLoadSection();
            GUILayout.Space(10f);

            DrawAdditiveSection();
            GUILayout.Space(10f);

            DrawRuntimeState();

            GUILayout.EndArea();
        }

        /// <summary>
        /// SceneA를 Single 방식으로 로드합니다.
        /// </summary>
        public void LoadSceneA()
        {
            Result<SceneOperation> result = controller.LoadAsync(sceneA, SceneLoadMode.Single);

            LogOperationResult(result, "SceneA Single Load");
        }

        /// <summary>
        /// SceneB를 Single 방식으로 로드합니다.
        /// </summary>
        public void LoadSceneB()
        {
            Result<SceneOperation> result = controller.LoadAsync(sceneB, SceneLoadMode.Single);

            LogOperationResult(result, "SceneB Single Load");
        }

        /// <summary>
        /// AdditiveScene을 Additive 방식으로 로드합니다.
        /// </summary>
        public void LoadAdditiveScene()
        {
            Result<SceneOperation> result = controller.LoadAsync(additiveScene, SceneLoadMode.Additive);

            LogOperationResult(result, "AdditiveScene Load");
        }

        /// <summary>
        /// SceneB를 Active Scene으로 변경합니다.
        /// </summary>
        public void SetSceneBActive()
        {
            Result result = controller.SetActiveScene(sceneB);

            LogResult(result, "SceneB Set Active");
        }

        /// <summary>
        /// AdditiveScene을 Active Scene으로 변경합니다.
        /// </summary>
        public void SetAdditiveSceneActive()
        {
            Result result = controller.SetActiveScene(additiveScene);

            LogResult(result, "AdditiveScene Set Active");
        }

        /// <summary>
        /// AdditiveScene을 언로드합니다.
        /// Active Scene인 경우 Framework 정책에 따라 실패합니다.
        /// </summary>
        public void UnloadAdditiveScene()
        {
            Result<SceneOperation> result = controller.UnloadAsync(additiveScene);

            LogOperationResult(result, "AdditiveScene Unload");
        }

        private void InitializeSceneReferences()
        {
            string currentScenePath = SceneManager.GetActiveScene().path;
            int separatorIndex = currentScenePath.LastIndexOf('/');

            if (separatorIndex < 0)
            {
                Debug.LogError("[Scene Sample] Sample Scene 경로를 확인할 수 없습니다.", this);
                return;
            }

            string sceneDirectory = currentScenePath.Substring(0, separatorIndex);

            sceneA = new SceneReference($"{sceneDirectory}/SceneA.unity");
            sceneB = new SceneReference($"{sceneDirectory}/SceneB.unity");
            additiveScene = new SceneReference($"{sceneDirectory}/AdditiveScene.unity");
        }

        private void DrawSingleLoadSection()
        {
            GUILayout.Label("Single Load");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Load Scene A"))
            {
                LoadSceneA();
            }

            if (GUILayout.Button("Load Scene B"))
            {
                LoadSceneB();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawAdditiveSection()
        {
            GUILayout.Label("Additive Scene");

            if (GUILayout.Button("Load Additive Scene"))
            {
                LoadAdditiveScene();
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Set Scene B Active"))
            {
                SetSceneBActive();
            }

            if (GUILayout.Button("Set Additive Active"))
            {
                SetAdditiveSceneActive();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Unload Additive Scene"))
            {
                UnloadAdditiveScene();
            }
        }

        private void DrawRuntimeState()
        {
            GUILayout.Label("Runtime State");

            GUILayout.Label($"Active Scene: {controller.ActiveScene.Path}");
            GUILayout.Label($"Busy: {controller.IsBusy}");
            GUILayout.Label($"Additive Loaded: {controller.IsSceneLoaded(additiveScene)}");

            SceneOperation operation = controller.CurrentOperation;

            if (operation == null)
            {
                GUILayout.Label("Current Operation: None");
                return;
            }

            GUILayout.Label($"Current Operation: {operation.Kind}");
            GUILayout.Label($"Target Scene: {operation.Scene.Path}");
            GUILayout.Label($"Progress: {operation.Progress:0.00}");
        }

        private void OnOperationStarted(SceneOperation operation)
        {
            Debug.Log($"[Scene Sample] 작업 시작 - {operation.Kind} / {operation.Scene.Path}", this);
        }

        private void OnOperationCompleted(SceneOperation operation)
        {
            Debug.Log($"[Scene Sample] 작업 완료 - {operation.Kind} / {operation.Scene.Path}", this);
        }

        private void LogOperationResult(Result<SceneOperation> result, string actionName)
        {
            if (result.IsFailure)
            {
                Debug.LogWarning($"[Scene Sample] {actionName} 실패 - {result.Error.Code}", this);
                return;
            }

            Debug.Log($"[Scene Sample] {actionName} 요청 성공 - {result.Value.Kind}", this);
        }

        private void LogResult(Result result, string actionName)
        {
            if (result.IsFailure)
            {
                Debug.LogWarning($"[Scene Sample] {actionName} 실패 - {result.Error.Code}", this);
                return;
            }

            Debug.Log($"[Scene Sample] {actionName} 성공", this);
        }
    }
}