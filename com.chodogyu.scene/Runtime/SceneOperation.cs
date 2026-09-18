using System;

namespace CDG.Scene
{
    /// <summary>
    /// Scene Framework에서 실행 중인 하나의 비동기 Scene 작업을 나타냅니다.
    /// 외부에서는 대상 Scene, 작업 종류, 정규화된 진행률 및 완료 상태를 이 타입을 통해 조회할 수 있습니다.
    /// </summary>
    public sealed class SceneOperation
    {
        private const float LoadReadyProgress = 0.9f;

        private readonly ISceneAsyncOperation operation;
        private bool isCompleted;

        /// <summary>
        /// 이 작업의 대상 Scene을 반환합니다.
        /// </summary>
        public SceneReference Scene { get; }

        /// <summary>
        /// 이 작업이 수행하는 Scene 작업 종류를 반환합니다.
        /// </summary>
        public SceneOperationKind Kind { get; }

        /// <summary>
        /// 현재 작업의 진행률을 0에서 1 범위로 정규화하여 반환합니다.
        /// Load 작업에서는 Unity의 0에서 0.9 범위를 Framework의 0에서 1 범위로 변환합니다.
        /// </summary>
        public float Progress
        {
            get
            {
                if (IsDone)
                {
                    return 1f;
                }

                float rawProgress = Clamp01(operation.Progress);

                if (Kind == SceneOperationKind.SingleLoad || Kind == SceneOperationKind.AdditiveLoad)
                {
                    return Clamp01(rawProgress / LoadReadyProgress);
                }

                return rawProgress;
            }
        }

        /// <summary>
        /// 비동기 Scene 작업이 완료되었는지 여부를 반환합니다.
        /// Progress가 1이더라도 실제 작업이 아직 완료되지 않았다면 false일 수 있습니다.
        /// </summary>
        public bool IsDone => isCompleted || operation.IsDone;

        /// <summary>
        /// 비동기 Scene 작업이 완료되었을 때 한 번 호출됩니다.
        /// </summary>
        public event Action Completed;

        /// <summary>
        /// 지정된 Scene과 작업 종류 및 내부 비동기 작업으로 SceneOperation을 생성합니다.
        /// 내부 비동기 작업은 null일 수 없습니다.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// operation이 null인 경우 발생합니다.
        /// </exception>
        internal SceneOperation(SceneReference scene, SceneOperationKind kind, ISceneAsyncOperation operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));

            Scene = scene;
            Kind = kind;
            isCompleted = operation.IsDone;

            this.operation.Completed += OnCompleted;
        }

        private void OnCompleted()
        {
            if (isCompleted)
            {
                return;
            }

            isCompleted = true;
            Completed?.Invoke();
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }
    }
}