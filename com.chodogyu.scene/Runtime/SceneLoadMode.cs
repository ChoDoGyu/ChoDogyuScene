namespace CDG.Scene
{
    /// <summary>
    /// Scene을 로드할 때 사용할 로딩 방식을 나타냅니다.
    /// Unity의 LoadSceneMode에 직접 의존하지 않는 Framework Public API로 사용됩니다.
    /// </summary>
    public enum SceneLoadMode
    {
        /// <summary>
        /// 현재 로드된 Scene을 교체하면서 대상 Scene을 로드합니다.
        /// </summary>
        Single = 0,

        /// <summary>
        /// 현재 Scene을 유지하면서 대상 Scene을 추가로 로드합니다.
        /// </summary>
        Additive = 1
    }
}