using System;
using UnityEngine;

namespace CDG.Scene
{
    /// <summary>
    /// Unity의 AsyncOperation을 Scene Framework 내부 비동기 작업 인터페이스로 연결합니다.
    /// Unity API에 대한 직접 의존을 상위 Scene 로직으로부터 분리하기 위해 사용합니다.
    /// </summary>
    internal sealed class UnitySceneAsyncOperation : ISceneAsyncOperation
    {
        private readonly AsyncOperation operation;

        /// <inheritdoc/>
        public float Progress => operation.progress;

        /// <inheritdoc/>
        public bool IsDone => operation.isDone;

        /// <inheritdoc/>
        public event Action Completed;

        /// <summary>
        /// 지정된 Unity AsyncOperation을 감싸는 Adapter를 생성합니다.
        /// null 작업은 허용하지 않습니다.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// operation이 null인 경우 발생합니다.
        /// </exception>
        internal UnitySceneAsyncOperation(AsyncOperation operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
            this.operation.completed += OnCompleted;
        }

        private void OnCompleted(AsyncOperation completedOperation)
        {
            Completed?.Invoke();
        }
    }
}