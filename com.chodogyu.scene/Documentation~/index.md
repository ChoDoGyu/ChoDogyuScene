# ChoDogyu Scene & Loading Framework

ChoDogyu Scene & Loading Framework는 Unity 프로젝트에서 Scene 전환, 비동기 로딩, Additive Loading, Unload 및 Active Scene 관리를 일관된 방식으로 처리하기 위한 범용 Scene 관리 패키지입니다.

Framework는 게임의 Scene 구성이나 게임 진행 흐름 자체를 결정하지 않습니다.

```text
프로젝트
→ 언제 Scene을 전환할지 결정
→ 어떤 Scene을 Additive로 사용할지 결정
→ Loading UI 연결
→ Scene별 게임 상태 관리

Scene Framework
→ Single Load
→ Additive Load
→ Unload
→ Active Scene 관리
→ Operation 상태 관리
→ Loading Progress
→ Scene Reference
→ Scene Validation
```

특정 게임이나 장르에 종속되지 않는 독립적인 Unity Package Manager 패키지로 사용할 수 있도록 구성했습니다.

---

# 1. Package Information

```text
Package Name : com.chodogyu.scene
Display Name : ChoDogyu Scene & Loading Framework
Version      : 1.0.0
Unity        : 6000.3+
Author       : ChoDogyu
```

개발 및 검증 환경:

```text
Unity 6.3 LTS
6000.3.9f1
```

Assembly:

```text
Runtime      : CDG.Scene
Editor       : CDG.Scene.Editor
Runtime Test : CDG.Scene.Tests.Runtime
Editor Test  : CDG.Scene.Tests.Editor
Sample       : CDG.Scene.Samples.BasicUsage
```

Namespace:

```text
CDG.Scene
CDG.Scene.Editor
CDG.Scene.Samples.BasicUsage
```

---

# 2. Requirements

필수 패키지:

```text
ChoDogyu Core 1.0.0
```

Scene Framework는 ChoDogyu Core의 다음 타입을 사용합니다.

```text
Result
Result<T>
ResultError
```

Scene Framework의 `package.json`은 ChoDogyu Core의 Git URL을 직접 의존성으로 선언하지 않습니다.

따라서 Git UPM으로 사용할 때는 Core를 먼저 설치해야 합니다.

다음 CDG 패키지는 필수 의존성이 아닙니다.

```text
ChoDogyu General Editor Tools
ChoDogyu Object Pooling
ChoDogyu Data Framework
ChoDogyu Save / Load Framework
ChoDogyu UI Framework
ChoDogyu Audio Framework
```

Scene Framework 자체는 UI, Audio, Save 등의 다른 시스템을 직접 제어하지 않습니다.

---

# 3. Installation

## 3.1 ChoDogyu Core v1.0.0 설치

Unity Package Manager에서 다음 Git URL을 사용합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

Unity에서:

```text
Window
→ Package Management
→ Package Manager
→ +
→ Install package from git URL...
```

Core 설치가 완료되고 Console Error가 없는 것을 확인합니다.

---

## 3.2 ChoDogyu Scene & Loading Framework v1.0.0 설치

Core 설치 후 다음 Git URL을 사용합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuScene.git?path=/com.chodogyu.scene#v1.0.0
```

설치 후 Package Manager에서 다음 정보가 표시되는지 확인합니다.

```text
ChoDogyu Scene & Loading Framework
1.0.0
```

---

# 4. Package Structure

패키지의 기본 구조는 다음과 같습니다.

```text
com.chodogyu.scene/
├─ Documentation~/
│  └─ index.md
├─ Editor/
├─ Runtime/
├─ Samples~/
│  └─ BasicUsage/
├─ Tests/
│  ├─ Editor/
│  └─ Runtime/
├─ CHANGELOG.md
├─ README.md
└─ package.json
```

### Runtime

게임 실행 중 사용하는 Scene 관리 기능을 포함합니다.

```text
SceneController
SceneReference
SceneOperation
SceneLoadMode
SceneOperationKind
SceneErrorCodes
UnitySceneRuntime
UnitySceneAsyncOperation
```

### Editor

`SceneReference`의 Inspector 표시 및 Validation 기능을 포함합니다.

### Tests

Runtime 및 Editor 동작을 자동 검증합니다.

### Samples~

Package Manager에서 Import 가능한 Basic Usage Sample을 포함합니다.

### Documentation~

Framework의 상세 사용법과 설계 정책을 설명합니다.

---

# 5. Architecture

Framework의 주요 Runtime 진입점은 `SceneController`입니다.

```text
Game Code
   ↓
SceneController
   ↓
ISceneRuntime
   ↓
UnitySceneRuntime
   ↓
Unity SceneManager
```

비동기 작업은 별도의 `SceneOperation`으로 표현합니다.

```text
SceneController
   ↓
SceneOperation
   ↓
ISceneAsyncOperation
   ↓
UnitySceneAsyncOperation
   ↓
Unity AsyncOperation
```

이 구조를 통해 상위 Scene 관리 규칙과 Unity API 세부 구현을 분리합니다.

---

# 6. SceneController

기본 Controller 생성:

```csharp
using CDG.Scene;

SceneController controller = new SceneController();
```

`SceneController`는 다음 기능을 제공합니다.

```text
LoadAsync
UnloadAsync
SetActiveScene
IsSceneLoaded
ActiveScene
IsBusy
CurrentOperation
TargetScene
OperationStarted
OperationCompleted
```

`SceneController`는 MonoBehaviour가 아닙니다.

필요한 시스템에서 직접 생성하여 사용할 수 있습니다.

---

# 7. SceneReference

Scene은 `SceneReference`를 통해 표현합니다.

```csharp
SceneReference scene =
    new SceneReference("Assets/Scenes/Game.unity");
```

Scene 이름이 아니라 전체 Asset Path를 사용합니다.

```text
Assets/Scenes/Game.unity
```

`Path`:

```csharp
string path = scene.Path;
```

빈 참조 확인:

```csharp
bool empty = scene.IsEmpty;
```

---

# 8. SceneReference 규칙

SceneReference는 생성 시 입력 문자열을 자동으로 수정하지 않습니다.

다음 처리를 수행하지 않습니다.

```text
Trim
경로 자동 수정
확장자 자동 추가
Asset 존재 여부 자동 검사
Build 등록 자동 처리
```

따라서 실제 Scene 사용 가능 여부는 Runtime 또는 Editor Validation 단계에서 확인합니다.

SceneReference 비교는 전체 Asset Path를 기준으로 합니다.

---

# 9. Inspector Scene Reference

Serializable Component 또는 ScriptableObject에서 사용할 수 있습니다.

```csharp
[SerializeField]
private SceneReference gameScene;
```

Inspector에서는 문자열 경로를 직접 입력하는 대신 Scene Asset을 Object Field에서 선택할 수 있습니다.

선택한 Scene의 전체 Asset Path가 내부적으로 저장됩니다.

---

# 10. Single Scene Load

기본 Load Mode는 `Single`입니다.

```csharp
SceneReference scene =
    new SceneReference("Assets/Scenes/Game.unity");

Result<SceneOperation> result =
    controller.LoadAsync(scene);
```

명시적으로 지정할 수도 있습니다.

```csharp
Result<SceneOperation> result =
    controller.LoadAsync(scene, SceneLoadMode.Single);
```

Single Load는 Unity의 기존 Scene을 교체하면서 대상 Scene을 로드합니다.

---

# 11. Additive Scene Load

현재 Scene을 유지하면서 추가 Scene을 로드할 수 있습니다.

```csharp
Result<SceneOperation> result =
    controller.LoadAsync(
        scene,
        SceneLoadMode.Additive);
```

예:

```text
MainScene
+
GameplayArea
+
Environment
```

같은 구조를 구성할 때 사용할 수 있습니다.

Additive Load가 완료되었다고 해서 대상 Scene이 자동으로 Active Scene이 되는 것은 아닙니다.

필요하면 별도로 `SetActiveScene()`을 호출해야 합니다.

---

# 12. SceneOperation

Load 또는 Unload 요청이 성공하면 `SceneOperation`이 반환됩니다.

```csharp
Result<SceneOperation> result =
    controller.LoadAsync(scene);

if (result.IsSuccess)
{
    SceneOperation operation = result.Value;
}
```

주요 정보:

```text
Scene
Kind
Progress
IsDone
Completed
```

---

# 13. Operation Kind

작업 종류는 `SceneOperationKind`로 표현합니다.

```text
SingleLoad
AdditiveLoad
Unload
```

사용 예:

```csharp
SceneOperationKind kind = operation.Kind;
```

---

# 14. Loading Progress

현재 진행률:

```csharp
float progress = operation.Progress;
```

범위:

```text
0.0 ~ 1.0
```

Unity Scene Load의 일반적인 원본 진행률:

```text
0.0 ~ 0.9
```

을 Framework에서:

```text
0.0 ~ 1.0
```

범위로 정규화합니다.

따라서 Loading UI는 Unity의 내부 0.9 규칙을 직접 알 필요 없이 Framework Progress를 사용할 수 있습니다.

---

# 15. Progress와 완료 상태

`Progress == 1`이라고 해서 반드시 Unity 비동기 작업이 완전히 끝났다는 의미는 아닙니다.

실제 완료 여부:

```csharp
bool completed = operation.IsDone;
```

또는:

```csharp
operation.Completed += OnCompleted;
```

를 사용합니다.

즉:

```text
Progress
→ 시각적 진행률

IsDone / Completed
→ 실제 작업 완료
```

로 구분합니다.

---

# 16. Operation Completed

개별 작업 완료 이벤트:

```csharp
operation.Completed += () =>
{
    Debug.Log("Scene 작업 완료");
};
```

이 이벤트는 해당 `SceneOperation`의 완료 시점을 나타냅니다.

---

# 17. Controller Operation Events

Controller 수준에서도 작업 시작과 완료를 받을 수 있습니다.

```csharp
controller.OperationStarted += operation =>
{
    Debug.Log($"시작: {operation.Kind}");
};

controller.OperationCompleted += operation =>
{
    Debug.Log($"완료: {operation.Kind}");
};
```

---

# 18. Busy State

현재 Scene 작업 진행 여부:

```csharp
bool busy = controller.IsBusy;
```

현재 작업:

```csharp
SceneOperation operation =
    controller.CurrentOperation;
```

현재 대상 Scene:

```csharp
SceneReference scene =
    controller.TargetScene;
```

작업이 없다면:

```text
IsBusy = false
CurrentOperation = null
TargetScene = Empty SceneReference
```

입니다.

---

# 19. Concurrent Operation Policy

Scene Framework v1은 하나의 `SceneController`에서 동시에 여러 Scene 작업을 실행하지 않습니다.

예:

```text
Scene A Load 진행 중
→ Scene B Load 요청
→ 실패
```

오류 코드:

```text
SCENE_OPERATION_IN_PROGRESS
```

이 정책을 통해 Controller의 현재 작업 상태를 명확하게 유지합니다.

---

# 20. Duplicate Request Policy

동일한 Scene과 동일한 작업이 이미 진행 중인 경우 새로운 Unity 작업을 시작하지 않습니다.

예:

```text
Scene A Single Load 진행 중
→ Scene A Single Load 다시 요청
```

결과:

```text
Success
→ 기존 SceneOperation 반환
```

Unload도 동일합니다.

```text
Scene A Unload 진행 중
→ Scene A Unload 다시 요청
→ 기존 Operation 반환
```

---

# 21. Already Loaded Scene

이미 로드된 Scene을 다시 Load하려 하면 실패합니다.

```text
SCENE_ALREADY_LOADED
```

Single 또는 Additive 요청 모두 동일한 기본 Validation을 사용합니다.

---

# 22. Build Registration

Load 대상 Scene은 현재 Build에 등록되어 있어야 합니다.

등록되지 않은 경우:

```text
SCENE_NOT_IN_BUILD
```

을 반환합니다.

Unity 6 환경에서는 현재 Build Profile에 Scene이 포함되어 있어야 합니다.

---

# 23. Scene Unload

로드된 Scene:

```csharp
Result<SceneOperation> result =
    controller.UnloadAsync(scene);
```

성공하면 비동기 Unload 작업을 나타내는 `SceneOperation`을 반환합니다.

---

# 24. Active Scene Unload

현재 Active Scene은 직접 Unload할 수 없습니다.

```text
Active Scene
→ Unload 요청
→ Failure
```

오류 코드:

```text
SCENE_CANNOT_UNLOAD_ACTIVE
```

먼저 다른 Scene을 Active로 변경합니다.

```csharp
controller.SetActiveScene(otherScene);
```

이후:

```csharp
controller.UnloadAsync(scene);
```

을 호출합니다.

---

# 25. Not Loaded Scene

로드되어 있지 않은 Scene을 Unload하려 하면:

```text
SCENE_NOT_LOADED
```

을 반환합니다.

Active Scene 변경도 대상 Scene이 로드되어 있어야 합니다.

---

# 26. Active Scene

현재 Active Scene:

```csharp
SceneReference activeScene =
    controller.ActiveScene;
```

이미 로드된 Scene을 Active로 변경:

```csharp
Result result =
    controller.SetActiveScene(scene);
```

---

# 27. SetActiveScene Rules

다음 경우 실패합니다.

빈 SceneReference:

```text
SCENE_INVALID_REFERENCE
```

로드되지 않은 Scene:

```text
SCENE_NOT_LOADED
```

Unity Runtime에서 Active Scene 변경 실패:

```text
SCENE_SET_ACTIVE_FAILED
```

---

# 28. IsSceneLoaded

Scene 로드 여부:

```csharp
bool loaded =
    controller.IsSceneLoaded(scene);
```

빈 SceneReference는:

```text
false
```

를 반환합니다.

---

# 29. Error Handling

Scene Framework의 작업 API는 ChoDogyu Core의 `Result` 계열을 사용합니다.

반환값이 없는 작업:

```csharp
Result
```

SceneOperation이 필요한 작업:

```csharp
Result<SceneOperation>
```

예:

```csharp
Result<SceneOperation> result =
    controller.LoadAsync(scene);

if (result.IsFailure)
{
    Debug.LogWarning(result.Error.Code);
    return;
}
```

---

# 30. Error Codes

지원 오류 코드:

```text
SCENE_INVALID_REFERENCE
SCENE_NOT_IN_BUILD
SCENE_ALREADY_LOADED
SCENE_NOT_LOADED
SCENE_OPERATION_IN_PROGRESS
SCENE_INVALID_LOAD_MODE
SCENE_LOAD_START_FAILED
SCENE_UNLOAD_START_FAILED
SCENE_CANNOT_UNLOAD_ACTIVE
SCENE_SET_ACTIVE_FAILED
```

---

# 31. SCENE_INVALID_REFERENCE

SceneReference가 비어 있거나 Scene을 정상적으로 식별할 수 없는 경우입니다.

---

# 32. SCENE_NOT_IN_BUILD

대상 Scene이 현재 Build에 등록되어 있지 않은 경우입니다.

---

# 33. SCENE_ALREADY_LOADED

이미 로드된 Scene을 다시 Load하려는 경우입니다.

---

# 34. SCENE_NOT_LOADED

로드되지 않은 Scene을 Unload하거나 Active Scene으로 지정하려는 경우입니다.

---

# 35. SCENE_OPERATION_IN_PROGRESS

다른 Scene 작업이 이미 실행 중인 경우입니다.

---

# 36. SCENE_INVALID_LOAD_MODE

지원하지 않는 `SceneLoadMode` 값이 전달된 경우입니다.

지원 값:

```text
Single
Additive
```

---

# 37. SCENE_LOAD_START_FAILED

Unity Runtime에서 Scene Load 비동기 작업 자체를 시작하지 못한 경우입니다.

---

# 38. SCENE_UNLOAD_START_FAILED

Unity Runtime에서 Scene Unload 비동기 작업을 시작하지 못한 경우입니다.

---

# 39. SCENE_CANNOT_UNLOAD_ACTIVE

현재 Active Scene을 직접 Unload하려는 경우입니다.

---

# 40. SCENE_SET_ACTIVE_FAILED

Unity Runtime이 Active Scene 변경을 수행하지 못한 경우입니다.

---

# 41. SceneReference Validation

Editor에서는 다음 API를 사용할 수 있습니다.

```csharp
SceneReferenceValidator.Validate(scene);
```

Validation 항목:

```text
SceneReference가 비어 있는지
Scene Asset이 실제 존재하는지
Build Settings에서 활성화되어 있는지
```

정상이면:

```text
Result.Success
```

문제가 있다면 해당 오류 코드의 Failure를 반환합니다.

---

# 42. Validation Does Not Modify Data

SceneReferenceValidator는 검증만 수행합니다.

다음을 자동으로 수행하지 않습니다.

```text
Build Settings 등록
Scene 경로 변경
Scene Asset 생성
SceneReference 자동 수정
```

---

# 43. Loading UI Integration

Scene Framework는 Loading UI 자체를 제공하지 않습니다.

게임의 UI 시스템에서 다음 값을 사용할 수 있습니다.

```csharp
SceneOperation operation =
    controller.CurrentOperation;

if (operation != null)
{
    float progress = operation.Progress;
}
```

예:

```text
Scene Framework
→ Progress 0~1
→ Game UI
→ Slider / Loading Bar
```

즉 Scene Framework와 UI Framework는 서로 독립적으로 사용할 수 있습니다.

---

# 44. Audio Integration

Scene Framework는 Scene 전환 시 Audio를 자동으로 정지하거나 변경하지 않습니다.

필요하다면 프로젝트에서 직접 연결합니다.

```text
Scene Load 요청
→ Game Flow
→ Audio Fade
→ Scene Framework
```

또는:

```text
Scene Operation Completed
→ 새로운 BGM 재생
```

같은 흐름을 게임에서 구성할 수 있습니다.

---

# 45. Save Integration

Scene Framework는 Scene 전환 시 자동 Save를 수행하지 않습니다.

예:

```text
Stage 종료
→ Game Save
→ Save 완료
→ Scene Load
```

같은 정책은 게임에서 결정합니다.

---

# 46. Additive Loading Use Cases

Additive Loading은 예를 들어 다음 구조에서 사용할 수 있습니다.

```text
Persistent Scene
+
Gameplay Scene
```

또는:

```text
World
+
Area A
+
Area B
```

처럼 여러 Scene을 동시에 유지하는 구조에서 사용할 수 있습니다.

Framework가 Streaming 정책 자체를 자동으로 결정하지는 않습니다.

---

# 47. Open World / Streaming

Scene Framework의 Additive Load 및 Unload API를 이용하여 Scene Streaming 구조의 기반으로 사용할 수 있습니다.

하지만 v1에서는 다음을 자동 처리하지 않습니다.

```text
Player 위치 기반 Area Load
거리 기반 Streaming
Chunk 자동 선택
Background Streaming Queue
Memory Budget
LOD
Addressables
World Partition
```

이 정책들은 상위 게임 시스템의 책임입니다.

---

# 48. Runtime Abstraction

SceneController는 Unity `SceneManager`를 직접 사용하는 대신 내부 Runtime 추상화를 사용합니다.

```text
SceneController
↓
ISceneRuntime
↓
UnitySceneRuntime
```

비동기 작업 역시:

```text
ISceneAsyncOperation
↓
UnitySceneAsyncOperation
```

구조로 분리되어 있습니다.

이를 통해 Controller의 Scene 정책을 Unity API 세부 구현과 분리하여 테스트할 수 있습니다.

---

# 49. Basic Usage Sample

Package Manager에서:

```text
Basic Usage
```

Sample을 Import할 수 있습니다.

포함 Scene:

```text
SceneA.unity
SceneB.unity
AdditiveScene.unity
```

포함 Script:

```text
BasicSceneUsage.cs
```

---

# 50. Sample Setup

Sample Import 후 다음 Scene을 현재 Build Profile에 등록하고 Enabled 상태로 설정합니다.

```text
SceneA.unity
SceneB.unity
AdditiveScene.unity
```

이후:

```text
SceneA.unity
```

를 열고 Play Mode에 진입합니다.

---

# 51. Sample Features

Sample에서 다음 기능을 확인할 수 있습니다.

```text
SceneA Single Load
SceneB Single Load
AdditiveScene Additive Load
SceneB Active Scene 설정
AdditiveScene Active Scene 설정
Active Scene Unload 차단
AdditiveScene Unload
현재 Active Scene
Busy 상태
현재 Operation
Target Scene
Progress
```

---

# 52. Sample DontDestroyOnLoad

Sample의 Controller GameObject는 Scene 전환 후에도 예제를 계속 조작할 수 있도록 Sample 코드에서:

```csharp
DontDestroyOnLoad(gameObject);
```

를 사용합니다.

이는 Sample 전용 동작입니다.

Scene Framework 자체가 사용자의 GameObject를 자동으로 `DontDestroyOnLoad` 처리하지 않습니다.

---

# 53. Tests

Unity Test Framework 기반으로 Runtime 및 Editor 테스트를 제공합니다.

Runtime 검증 영역:

```text
SceneReference
SceneOperation
Progress
Completion
SceneController
Single Load
Additive Load
Unload
Active Scene
중복 요청
동시 작업
재진입
Unity Runtime Adapter
실제 Scene 통합 흐름
```

Editor 검증 영역:

```text
SceneReference Validation
Scene Asset 존재 여부
Build 등록 여부
```

---

# 54. Independent UPM Usage

Framework는 새로운 Unity 프로젝트에서 독립 UPM 패키지로 사용할 수 있도록 구성했습니다.

기본 설치 관계:

```text
ChoDogyu Core v1.0.0
↓
ChoDogyu Scene & Loading Framework v1.0.0
```

다른 CDG 패키지를 설치하지 않아도 Scene Framework 자체 기능을 사용할 수 있습니다.

---

# 55. Responsibilities

Scene Framework가 담당합니다.

```text
Scene Load
Scene Unload
Single / Additive
Active Scene
Scene Operation
Loading Progress
Scene 작업 상태
Scene Reference
Scene Validation
Scene 오류 표현
```

Framework가 담당하지 않습니다.

```text
Loading UI
BGM 전환
Save 실행
Game Flow
Player Spawn
Scene별 초기화 순서
Dependency Injection
Addressables
Open World Streaming 정책
Scene Queue
Scene Transition Animation
Fade Screen
Network Scene Synchronization
게임별 Scene 규칙
```

---

# 56. Design Summary

Scene Framework v1.0.0의 핵심 구조:

```text
SceneReference
+
SceneController
+
SceneOperation
+
Runtime Abstraction
+
Result Error Handling
+
Editor Validation
```

Framework는 Scene 전환의 기술적 책임을 담당합니다.

어떤 Scene으로 이동해야 하는지, 언제 이동해야 하는지, Scene 전환 전후에 어떤 게임 로직을 실행해야 하는지는 상위 게임 시스템이 결정합니다.

이를 통해 Scene 관리 기술과 게임 진행 규칙의 책임을 분리하는 것을 기본 원칙으로 합니다.
