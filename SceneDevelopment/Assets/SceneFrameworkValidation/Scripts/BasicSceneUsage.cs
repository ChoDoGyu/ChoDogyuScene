using CDG.Core.Results;
using CDG.Scene;
using UnityEngine;

namespace CDG.Scene.Validation
{
    /// <summary>
    /// Scene Framework의 기본 사용 흐름을 실제 Scene에서 검증하기 위한 예제 컴포넌트입니다.
    /// Single Load 이후에도 검증을 계속할 수 있도록 이 샘플 오브젝트 자체는 명시적으로 유지됩니다.
    /// </summary>
    public sealed class BasicSceneUsage : MonoBehaviour
    {
        [SerializeField]
        private SceneReference sceneA;

        [SerializeField]
        private SceneReference sceneB;

        [SerializeField]
        private SceneReference additiveScene;

        private SceneController controller;

        /// <summary>
        /// 이 샘플에서 사용하는 SceneController를 반환합니다.
        /// </summary>
        public SceneController Controller => controller;

        private void Awake()
        {
            controller = new SceneController();

            controller.OperationStarted += OnOperationStarted;
            controller.OperationCompleted += OnOperationCompleted;

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (controller == null)
            {
                return;
            }

            controller.OperationStarted -= OnOperationStarted;
            controller.OperationCompleted -= OnOperationCompleted;
        }

        /// <summary>
        /// SceneA를 Single 방식으로 로드합니다.
        /// </summary>
        [ContextMenu("SceneA Single Load")]
        public void LoadSceneA()
        {
            Result<SceneOperation> result = controller.LoadAsync(sceneA, SceneLoadMode.Single);

            LogOperationResult(result, "SceneA Single Load");
        }

        /// <summary>
        /// SceneB를 Single 방식으로 로드합니다.
        /// </summary>
        [ContextMenu("SceneB Single Load")]
        public void LoadSceneB()
        {
            Result<SceneOperation> result = controller.LoadAsync(sceneB, SceneLoadMode.Single);

            LogOperationResult(result, "SceneB Single Load");
        }

        /// <summary>
        /// AdditiveScene을 Additive 방식으로 로드합니다.
        /// </summary>
        [ContextMenu("AdditiveScene Load")]
        public void LoadAdditiveScene()
        {
            Result<SceneOperation> result = controller.LoadAsync(additiveScene, SceneLoadMode.Additive);

            LogOperationResult(result, "AdditiveScene Load");
        }

        /// <summary>
        /// SceneB를 Active Scene으로 변경합니다.
        /// SceneB가 현재 로드되어 있어야 합니다.
        /// </summary>
        [ContextMenu("SceneB Set Active")]
        public void SetSceneBActive()
        {
            Result result = controller.SetActiveScene(sceneB);

            LogResult(result, "SceneB Set Active");
        }

        /// <summary>
        /// AdditiveScene을 Active Scene으로 변경합니다.
        /// AdditiveScene이 현재 로드되어 있어야 합니다.
        /// </summary>
        [ContextMenu("AdditiveScene Set Active")]
        public void SetAdditiveSceneActive()
        {
            Result result = controller.SetActiveScene(additiveScene);

            LogResult(result, "AdditiveScene Set Active");
        }

        /// <summary>
        /// AdditiveScene을 언로드합니다.
        /// AdditiveScene이 Active Scene인 경우 먼저 다른 Scene을 Active로 변경해야 합니다.
        /// </summary>
        [ContextMenu("AdditiveScene Unload")]
        public void UnloadAdditiveScene()
        {
            Result<SceneOperation> result = controller.UnloadAsync(additiveScene);

            LogOperationResult(result, "AdditiveScene Unload");
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