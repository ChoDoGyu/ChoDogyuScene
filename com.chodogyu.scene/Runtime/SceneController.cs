using System;
using CDG.Core.Results;

namespace CDG.Scene
{
    /// <summary>
    /// Scene 로드, 언로드 및 Active Scene 관리를 제공하는 Scene Framework의 중심 진입점입니다.
    /// 현재 Scene 작업 상태를 관리하며 실제 Unity Scene API 호출은 내부 Runtime 구현에 위임합니다.
    /// </summary>
    public sealed class SceneController
    {
        private readonly ISceneRuntime runtime;
        private SceneOperation currentOperation;

        /// <summary>
        /// 현재 비동기 Scene 작업이 진행 중인지 여부를 반환합니다.
        /// </summary>
        public bool IsBusy => currentOperation != null;

        /// <summary>
        /// 현재 진행 중인 Scene 작업을 반환합니다.
        /// 작업이 없으면 null을 반환합니다.
        /// </summary>
        public SceneOperation CurrentOperation => currentOperation;

        /// <summary>
        /// 현재 진행 중인 작업의 대상 Scene을 반환합니다.
        /// 작업이 없으면 빈 SceneReference를 반환합니다.
        /// </summary>
        public SceneReference TargetScene => currentOperation?.Scene ?? default;

        /// <summary>
        /// 현재 Unity Active Scene을 반환합니다.
        /// </summary>
        public SceneReference ActiveScene => runtime.GetActiveScene();

        /// <summary>
        /// 새로운 Scene 작업이 Controller에 등록되었을 때 호출됩니다.
        /// </summary>
        public event Action<SceneOperation> OperationStarted;

        /// <summary>
        /// 현재 Scene 작업이 완료되어 Controller의 Busy 상태가 해제된 후 호출됩니다.
        /// </summary>
        public event Action<SceneOperation> OperationCompleted;

        /// <summary>
        /// Unity Scene Runtime을 사용하는 기본 SceneController를 생성합니다.
        /// </summary>
        public SceneController()
            : this(new UnitySceneRuntime())
        {
        }

        /// <summary>
        /// 지정된 Scene Runtime을 사용하는 SceneController를 생성합니다.
        /// 테스트 및 내부 구현에서 Unity Scene API와 Controller를 분리하기 위해 사용합니다.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// runtime이 null인 경우 발생합니다.
        /// </exception>
        internal SceneController(ISceneRuntime runtime)
        {
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        /// <summary>
        /// 지정된 Scene을 비동기로 로드합니다.
        /// v1.0의 현재 구현에서는 Single 모드의 로드를 지원합니다.
        /// </summary>
        public Result<SceneOperation> LoadAsync(SceneReference scene, SceneLoadMode mode = SceneLoadMode.Single)
        {
            Result validationResult = ValidateLoadRequest(scene, mode);

            if (validationResult.IsFailure)
            {
                return Result<SceneOperation>.Failure(validationResult.Error);
            }

            ISceneAsyncOperation asyncOperation = runtime.LoadSingleAsync(scene);

            if (asyncOperation == null)
            {
                return Result<SceneOperation>.Failure(new ResultError(SceneErrorCodes.LoadStartFailed, "Failed to start the scene load operation."));
            }

            SceneOperation operation = new SceneOperation(scene, SceneOperationKind.SingleLoad, asyncOperation);

            TrackOperation(operation);

            return Result<SceneOperation>.Success(operation);
        }

        /// <summary>
        /// 지정된 Scene 작업을 현재 작업으로 등록하고 완료 상태를 추적합니다.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// operation이 null인 경우 발생합니다.
        /// </exception>
        internal void TrackOperation(SceneOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            currentOperation = operation;
            currentOperation.Completed += OnOperationCompleted;

            OperationStarted?.Invoke(currentOperation);

            if (currentOperation.IsDone)
            {
                OnOperationCompleted();
            }
        }

        private Result ValidateLoadRequest(SceneReference scene, SceneLoadMode mode)
        {
            if (scene.IsEmpty)
            {
                return Result.Failure(new ResultError(SceneErrorCodes.InvalidReference, "Scene reference is empty."));
            }

            if (mode != SceneLoadMode.Single)
            {
                return Result.Failure(new ResultError(SceneErrorCodes.InvalidLoadMode, "The specified scene load mode is not supported."));
            }

            if (!runtime.IsSceneInBuild(scene))
            {
                return Result.Failure(new ResultError(SceneErrorCodes.NotInBuild, "The scene is not registered in the current build."));
            }

            if (runtime.IsSceneLoaded(scene))
            {
                return Result.Failure(new ResultError(SceneErrorCodes.AlreadyLoaded, "The scene is already loaded."));
            }

            return Result.Success();
        }

        private void OnOperationCompleted()
        {
            if (currentOperation == null)
            {
                return;
            }

            SceneOperation completedOperation = currentOperation;

            completedOperation.Completed -= OnOperationCompleted;
            currentOperation = null;

            OperationCompleted?.Invoke(completedOperation);
        }
    }
}