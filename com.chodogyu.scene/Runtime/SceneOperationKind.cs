namespace CDG.Scene
{
    /// <summary>
    /// Scene Framework에서 실행되는 비동기 Scene 작업의 종류를 나타냅니다.
    /// </summary>
    public enum SceneOperationKind
    {
        /// <summary>
        /// 기존 Scene을 교체하는 Single Scene Load 작업입니다.
        /// </summary>
        SingleLoad = 0,

        /// <summary>
        /// 기존 Scene을 유지하면서 추가 Scene을 로드하는 Additive Scene Load 작업입니다.
        /// </summary>
        AdditiveLoad = 1,

        /// <summary>
        /// 현재 로드된 Scene을 비동기로 제거하는 Unload 작업입니다.
        /// </summary>
        Unload = 2
    }
}