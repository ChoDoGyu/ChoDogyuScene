using System;

namespace CDG.Scene
{
    /// <summary>
    /// Scene Framework 내부에서 비동기 Scene 작업의 진행 상태와 완료 시점을 추상화합니다.
    /// Unity의 AsyncOperation 구현 세부 사항을 상위 Scene 로직과 분리하기 위해 사용합니다.
    /// </summary>
    internal interface ISceneAsyncOperation
    {
        /// <summary>
        /// 현재 비동기 작업의 원본 진행률을 반환합니다.
        /// 값의 정규화 규칙은 상위 Scene 작업 계층에서 처리합니다.
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// 비동기 작업이 완료되었는지 여부를 반환합니다.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// 비동기 작업이 완료되었을 때 호출됩니다.
        /// </summary>
        event Action Completed;
    }
}