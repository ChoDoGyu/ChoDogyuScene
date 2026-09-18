namespace CDG.Scene
{
    /// <summary>
    /// Scene & Loading Framework에서 사용하는 안정적인 오류 코드 모음입니다.
    /// 외부 코드에서는 오류 메시지보다 오류 코드를 기준으로 실패 원인을 구분할 수 있습니다.
    /// </summary>
    public static class SceneErrorCodes
    {
        /// <summary>
        /// SceneReference가 비어 있거나 Scene을 식별할 수 없는 값을 가지고 있을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string InvalidReference = "SCENE_INVALID_REFERENCE";

        /// <summary>
        /// 요청한 Scene이 현재 Build Settings 또는 Build Profile에 포함되어 있지 않을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string NotInBuild = "SCENE_NOT_IN_BUILD";

        /// <summary>
        /// Load를 요청한 Scene이 이미 로드되어 있을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string AlreadyLoaded = "SCENE_ALREADY_LOADED";

        /// <summary>
        /// Unload 또는 Active Scene 변경 대상으로 요청한 Scene이 현재 로드되어 있지 않을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string NotLoaded = "SCENE_NOT_LOADED";

        /// <summary>
        /// 다른 Scene 작업이 진행 중이어서 새로운 작업을 시작할 수 없을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string OperationInProgress = "SCENE_OPERATION_IN_PROGRESS";

        /// <summary>
        /// 지원하지 않거나 유효하지 않은 Scene Load Mode가 전달되었을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string InvalidLoadMode = "SCENE_INVALID_LOAD_MODE";

        /// <summary>
        /// Unity Scene Load 작업을 시작하지 못했을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string LoadStartFailed = "SCENE_LOAD_START_FAILED";

        /// <summary>
        /// Unity Scene Unload 작업을 시작하지 못했을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string UnloadStartFailed = "SCENE_UNLOAD_START_FAILED";

        /// <summary>
        /// 현재 Active Scene을 직접 Unload하려고 했을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string CannotUnloadActive = "SCENE_CANNOT_UNLOAD_ACTIVE";

        /// <summary>
        /// 요청한 Scene을 Active Scene으로 변경하지 못했을 때 사용하는 오류 코드입니다.
        /// </summary>
        public const string SetActiveFailed = "SCENE_SET_ACTIVE_FAILED";
    }
}