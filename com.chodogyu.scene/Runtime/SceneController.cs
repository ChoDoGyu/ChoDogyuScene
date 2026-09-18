using System;

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