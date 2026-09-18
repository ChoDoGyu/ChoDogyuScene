namespace CDG.Scene
{
    /// <summary>
    /// Scene Framework 내부에서 Scene 조회, 로드, 언로드 및 Active Scene 변경 기능을 추상화합니다.
    /// Unity SceneManager에 대한 직접 의존을 상위 Scene 로직과 분리하기 위해 사용합니다.
    /// </summary>
    internal interface ISceneRuntime
    {
        /// <summary>
        /// 지정된 Scene이 현재 빌드 대상 Scene으로 등록되어 있는지 여부를 반환합니다.
        /// </summary>
        bool IsSceneInBuild(SceneReference scene);

        /// <summary>
        /// 지정된 Scene이 현재 로드되어 있는지 여부를 반환합니다.
        /// </summary>
        bool IsSceneLoaded(SceneReference scene);

        /// <summary>
        /// 현재 Active Scene을 SceneReference로 반환합니다.
        /// </summary>
        SceneReference GetActiveScene();

        /// <summary>
        /// 지정된 Scene을 Single 모드로 비동기 로드합니다.
        /// 작업을 시작하지 못한 경우 null을 반환할 수 있습니다.
        /// </summary>
        ISceneAsyncOperation LoadSingleAsync(SceneReference scene);

        /// <summary>
        /// 지정된 Scene을 Additive 모드로 비동기 로드합니다.
        /// 작업을 시작하지 못한 경우 null을 반환할 수 있습니다.
        /// </summary>
        ISceneAsyncOperation LoadAdditiveAsync(SceneReference scene);

        /// <summary>
        /// 지정된 Scene을 비동기로 언로드합니다.
        /// 작업을 시작하지 못한 경우 null을 반환할 수 있습니다.
        /// </summary>
        ISceneAsyncOperation UnloadAsync(SceneReference scene);

        /// <summary>
        /// 이미 로드된 지정 Scene을 Active Scene으로 변경합니다.
        /// 변경 성공 여부를 반환합니다.
        /// </summary>
        bool SetActiveScene(SceneReference scene);
    }
}