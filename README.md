# ChoDogyu Scene & Loading Framework

Unity 프로젝트에서 Scene 전환, 비동기 로딩, Additive Loading, Unload 및 Active Scene 관리를 일관된 방식으로 처리하기 위한 범용 Scene & Loading Framework입니다.

게임에서 어떤 Scene으로 언제 이동할지는 사용하는 프로젝트가 결정하고, Framework는 Scene Load / Unload, 비동기 작업 상태, Loading Progress, Active Scene 및 SceneReference Validation을 담당합니다.

특정 게임이나 장르에 종속되지 않으며 Unity Package Manager를 통해 독립적으로 설치할 수 있도록 구성했습니다.

---

## 주요 기능

### Scene Loading

* Single Scene 비동기 로딩
* Additive Scene 비동기 로딩
* 비동기 Scene Unload
* Active Scene 조회 및 변경
* 현재 Scene Load 상태 조회

### Scene Operation

* `SceneOperation` 기반 비동기 작업 표현
* `0 ~ 1` 범위의 정규화된 Loading Progress
* 실제 작업 완료 상태 조회
* 작업 시작 이벤트
* 작업 완료 이벤트
* 동일 작업 중복 요청 처리
* 동시 Scene 작업 방지

### SceneReference

* 전체 Scene Asset Path 기반 참조
* Serializable 값 형식
* Inspector에서 Scene Asset 직접 선택
* 문자열 Scene 이름 직접 사용 최소화

### Validation

* 빈 SceneReference 검사
* Scene Asset 존재 여부 검사
* Build 등록 여부 검사
* Result 기반 Validation 결과

### Error Handling

* ChoDogyu Core `Result` / `Result<T>` 사용
* 안정적인 Scene 오류 코드 제공
* 오류 메시지와 오류 코드 분리

### Sample

Package Manager용 `Basic Usage` Sample을 제공합니다.

```text
SceneA
SceneB
AdditiveScene
BasicSceneUsage
```

Sample을 통해 다음 기능을 직접 확인할 수 있습니다.

```text
Single Load
Additive Load
Active Scene 변경
Active Scene Unload 차단
Additive Scene Unload
Busy 상태
현재 Scene Operation
Loading Progress
```

---

## 핵심 구조

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

비동기 Scene 작업:

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

외부에서는 `SceneController`를 Scene 관리의 중심 진입점으로 사용합니다.

Unity `SceneManager` 및 `AsyncOperation`의 세부 구현은 내부 Runtime 계층으로 분리했습니다.

---

## SceneController

기본 생성:

```csharp
using CDG.Scene;

SceneController sceneController = new SceneController();
```

주요 API:

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

사용하는 게임이나 상위 시스템에서 생명주기를 직접 결정합니다.

---

## Single Scene Load

```csharp
SceneReference scene =
    new SceneReference("Assets/Scenes/Game.unity");

var result =
    sceneController.LoadAsync(scene);
```

기본 Load Mode는:

```text
Single
```

입니다.

명시적으로 지정할 수도 있습니다.

```csharp
var result = sceneController.LoadAsync(
    scene,
    SceneLoadMode.Single);
```

Single Load는 기존 Scene을 교체하면서 새로운 Scene을 로드합니다.

---

## Additive Scene Load

현재 Scene을 유지한 상태에서 추가 Scene을 로드할 수 있습니다.

```csharp
var result = sceneController.LoadAsync(
    scene,
    SceneLoadMode.Additive);
```

예:

```text
Persistent Scene
+
Gameplay Scene
+
Environment Scene
```

Additive Load 완료 후 대상 Scene을 자동으로 Active Scene으로 변경하지는 않습니다.

필요한 경우 별도로:

```csharp
sceneController.SetActiveScene(scene);
```

을 호출합니다.

---

## Scene Unload

로드된 Scene:

```csharp
var result =
    sceneController.UnloadAsync(scene);
```

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

다른 Scene을 먼저 Active로 변경한 뒤 Unload해야 합니다.

---

## Loading Progress

Load 또는 Unload가 시작되면 `SceneOperation`을 통해 진행 상태를 확인할 수 있습니다.

```csharp
SceneOperation operation = result.Value;

float progress = operation.Progress;
bool completed = operation.IsDone;
```

Load Progress는:

```text
0.0 ~ 1.0
```

범위로 제공됩니다.

Unity Scene Load의 내부 `0.0 ~ 0.9` 진행률은 Framework에서 `0.0 ~ 1.0`으로 정규화합니다.

따라서 게임의 Loading UI는 Unity의 내부 진행률 규칙을 직접 처리할 필요가 없습니다.

---

## Progress와 실제 완료 상태

```text
Progress
= Loading UI 표시용 진행률

IsDone / Completed
= 실제 비동기 작업 완료
```

Progress가 `1.0`이라고 해서 반드시 작업 자체가 완전히 종료된 것은 아닐 수 있습니다.

실제 완료 여부는:

```csharp
operation.IsDone
```

또는:

```csharp
operation.Completed += () =>
{
    // 실제 Scene 작업 완료
};
```

를 기준으로 판단합니다.

---

## Scene 작업 상태

```csharp
bool busy =
    sceneController.IsBusy;

SceneOperation operation =
    sceneController.CurrentOperation;

SceneReference target =
    sceneController.TargetScene;
```

작업이 없는 경우:

```text
IsBusy = false
CurrentOperation = null
TargetScene = Empty SceneReference
```

입니다.

---

## 중복 요청

동일한 Scene과 동일한 작업이 이미 진행 중이면 새로운 Unity 작업을 생성하지 않습니다.

예:

```text
SceneA Single Load 진행 중
↓
SceneA Single Load 다시 요청
```

결과:

```text
Success
→ 기존 SceneOperation 반환
```

Unload 작업도 동일하게 처리합니다.

---

## 동시 작업

하나의 `SceneController`에서는 동시에 여러 Scene 작업을 실행하지 않습니다.

예:

```text
SceneA Load 진행 중
↓
SceneB Load 요청
↓
Failure
```

오류 코드:

```text
SCENE_OPERATION_IN_PROGRESS
```

이를 통해 현재 Scene Operation 상태를 명확하게 유지합니다.

---

## SceneReference

Scene은 전체 Asset Path를 사용하는 `SceneReference`로 표현합니다.

```csharp
SceneReference scene =
    new SceneReference("Assets/Scenes/Game.unity");
```

Scene 이름만 저장하지 않고 전체 경로를 사용합니다.

```text
Assets/Scenes/Game.unity
```

SceneReference는 입력 문자열을 자동으로 변경하지 않습니다.

다음 처리를 하지 않습니다.

```text
Trim
대소문자 변경
경로 자동 수정
.unity 확장자 자동 추가
Build 자동 등록
```

---

## Inspector Scene 선택

Serializable Component 또는 ScriptableObject에서 사용할 수 있습니다.

```csharp
[SerializeField]
private SceneReference gameScene;
```

Inspector에서는 Scene Asset을 직접 선택할 수 있습니다.

선택된 Scene의 전체 Asset Path가 SceneReference 내부 값으로 저장됩니다.

---

## Scene Validation

Editor에서는:

```csharp
SceneReferenceValidator.Validate(scene);
```

를 사용할 수 있습니다.

검사 항목:

```text
빈 SceneReference
실제 Scene Asset 존재 여부
Build 등록 여부
```

Validation은 값을 자동으로 수정하지 않습니다.

다음을 수행하지 않습니다.

```text
SceneReference 자동 수정
Scene Asset 생성
Build Settings 자동 등록
Scene 경로 자동 변경
```

---

## Runtime Error Codes

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

외부 코드에서는 오류 메시지 문자열보다 `Error.Code`를 기준으로 실패 원인을 구분할 수 있습니다.

---

## 저장소 구조

```text
ChoDogyuScene/
├─ SceneDevelopment/
│  └─ 패키지 개발 및 검증용 Unity 프로젝트
│
├─ com.chodogyu.scene/
│  ├─ Runtime/
│  ├─ Editor/
│  ├─ Tests/
│  │  ├─ Runtime/
│  │  └─ Editor/
│  ├─ Samples~/
│  │  └─ BasicUsage/
│  ├─ Documentation~/
│  │  └─ index.md
│  ├─ package.json
│  ├─ README.md
│  └─ CHANGELOG.md
│
├─ .gitattributes
├─ .gitignore
└─ README.md
```

### SceneDevelopment

Scene & Loading Framework 개발, 테스트 및 통합 검증에 사용하는 Unity 프로젝트입니다.

실제 UPM 배포 대상에는 포함되지 않습니다.

### com.chodogyu.scene

실제 Unity Package Manager 패키지입니다.

다른 Unity 프로젝트에서는 이 폴더를 Git UPM 패키지로 설치합니다.

---

## 요구 사항

* Unity 6.3 이상
* ChoDogyu Core 1.0.0

개발 및 검증 환경:

```text
Unity 6.3 LTS
6000.3.9f1
```

Scene Framework는 ChoDogyu Core의 Result 계열 타입을 사용합니다.

따라서 Core를 먼저 설치해야 합니다.

다음 패키지는 필수 의존성이 아닙니다.

```text
ChoDogyu General Editor Tools
ChoDogyu Object Pooling
ChoDogyu Data Framework
ChoDogyu Save / Load Framework
ChoDogyu UI Framework
ChoDogyu Audio Framework
```

---

## 설치

### 1. ChoDogyu Core

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

### 2. ChoDogyu Scene & Loading Framework

```text
https://github.com/ChoDoGyu/ChoDogyuScene.git?path=/com.chodogyu.scene#v1.0.0
```

Unity:

```text
Window
→ Package Management
→ Package Manager
→ +
→ Install package from git URL...
```

Core를 먼저 설치한 뒤 Scene Framework를 설치합니다.

---

## 기본 사용

```csharp
using CDG.Core.Results;
using CDG.Scene;

SceneController controller =
    new SceneController();

SceneReference scene =
    new SceneReference("Assets/Scenes/Game.unity");

Result<SceneOperation> result =
    controller.LoadAsync(scene);

if (result.IsFailure)
{
    return;
}

SceneOperation operation =
    result.Value;

operation.Completed += () =>
{
    // Scene Load 완료
};
```

---

## Basic Usage Sample

Package Manager에서:

```text
ChoDogyu Scene & Loading Framework
→ Samples
→ Basic Usage
→ Import
```

Sample 구성:

```text
BasicUsage/
├─ Scenes/
│  ├─ SceneA.unity
│  ├─ SceneB.unity
│  └─ AdditiveScene.unity
├─ Scripts/
│  ├─ BasicSceneUsage.cs
│  └─ CDG.Scene.Samples.BasicUsage.asmdef
└─ README.md
```

Import 후 다음 Scene을 현재 Build Profile에 등록하고 Enabled 상태로 설정합니다.

```text
SceneA.unity
SceneB.unity
AdditiveScene.unity
```

그다음:

```text
SceneA.unity
```

를 열고 Play Mode에 진입합니다.

---

## Sample에서 확인 가능한 기능

```text
SceneA Single Load
SceneB Single Load
Additive Scene Load
SceneB Active Scene 설정
Additive Scene Active 설정
Active Scene Unload 차단
Additive Scene Unload
현재 Active Scene
Busy 상태
현재 Operation
Target Scene
Loading Progress
```

Sample의 `DontDestroyOnLoad` 사용은 Sample Controller를 Scene 전환 후에도 유지하기 위한 예제 코드입니다.

Framework 자체가 사용자의 GameObject를 자동으로 유지하지는 않습니다.

---

## Loading UI와의 관계

Framework는 Loading UI 자체를 제공하지 않습니다.

대신:

```csharp
float progress =
    sceneController.CurrentOperation.Progress;
```

같이 진행률을 제공하여 게임 UI에서 사용할 수 있습니다.

```text
Scene Framework
→ Loading Progress
→ Game UI
→ Loading Bar
```

UI Framework는 필수 의존성이 아닙니다.

---

## Audio와의 관계

Scene Framework는 Scene 전환 시 BGM이나 SFX를 자동으로 변경하지 않습니다.

필요하다면 게임에서:

```text
Scene 전환 요청
→ Audio Fade
→ Scene Load
```

또는:

```text
Scene Load 완료
→ 새로운 BGM 재생
```

같은 흐름을 직접 구성할 수 있습니다.

Audio Framework는 필수 의존성이 아닙니다.

---

## Save와의 관계

Scene 전환 전 자동 Save를 수행하지 않습니다.

예:

```text
Stage 완료
→ Save
→ Scene 전환
```

같은 게임 정책은 상위 시스템에서 결정합니다.

Save Framework는 필수 의존성이 아닙니다.

---

## Open World / Streaming

Additive Load와 Unload를 이용하면 Scene Streaming 구조의 기반으로 사용할 수 있습니다.

예:

```text
Persistent Scene
+
World Area A
+
World Area B
```

하지만 Framework가 다음을 자동 처리하지는 않습니다.

```text
Player 위치 기반 Streaming
거리 기반 Area 선택
Chunk 자동 Load
Streaming Queue
Memory Budget
World Partition
Addressables
LOD
```

이러한 정책은 사용하는 게임의 상위 시스템 책임입니다.

---

## 책임 범위

Framework가 담당:

```text
Scene Load
Scene Unload
Single Load
Additive Load
Active Scene 관리
Scene Operation 상태
Loading Progress
중복 요청 처리
동시 작업 방지
SceneReference
Scene Validation
Result 기반 오류 처리
```

Framework가 담당하지 않음:

```text
게임 진행 흐름
Loading UI
Screen Fade
BGM 전환
자동 Save
Player Spawn
Scene 초기화 순서
Dependency Injection
Addressables
Open World Streaming 정책
Scene Queue
Network Scene Synchronization
Runtime Singleton
Service Locator
자동 DontDestroyOnLoad
```

---

## 테스트

Unity Test Framework 기반 Runtime / Editor 테스트를 구성했습니다.

주요 Runtime 검증:

```text
SceneReference
SceneOperation
Progress
Completion
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

주요 Editor 검증:

```text
SceneReference Validation
Scene Asset 존재 여부
Build 등록 여부
```

---

## UPM 독립 설치 검증

새로운 Unity 6.3 프로젝트에서 실제 Git UPM 설치 및 사용 흐름을 검증했습니다.

```text
ChoDogyu Core v1.0.0 설치
→ 성공

ChoDogyu Scene & Loading Framework 설치
→ 성공

Runtime / Editor Compile
→ 성공

Basic Usage Sample Import
→ 성공

Sample Scene 실행
→ 성공

Single Load
→ 정상

Additive Load
→ 정상

Active Scene 변경
→ 정상

Scene Unload
→ 정상
```

개발 프로젝트의 Local Package 경로에 의존하지 않는 독립 사용 흐름을 확인했습니다.

---

## 다른 CDG 패키지와의 관계

```text
Core
→ 공통 Result 기반

Pooling
→ Runtime Object 재사용

Data
→ 정적 데이터 및 Import

Save
→ 플레이 상태 저장

UI
→ 화면 흐름

Audio
→ BGM / SFX

Scene
→ Scene Load / Unload 및 Loading 상태 관리
```

각 패키지는 자신의 책임을 독립적으로 유지합니다.

Scene Framework는 Core 외 다른 CDG 패키지에 직접 의존하지 않습니다.

---

## 설계 방향

```text
게임이 Scene 전환 시점을 결정
Framework가 Scene 전환 방법을 관리

Scene 이름 대신 SceneReference 사용
비동기 작업을 SceneOperation으로 표현
Loading Progress 정규화
동시 Scene 작업 명시적 제한
중복 요청 재사용
Active Scene Unload 방지
Result 기반 오류 처리
Unity Scene API 구현 세부 사항 분리
최소 패키지 의존성
게임별 Scene 정책 비강제
독립 설치 가능한 UPM 구조
```

---

## 문서

패키지 기본 사용법:

```text
ChoDogyuScene/com.chodogyu.scene/README.md
```

상세 설계 및 사용 규칙:

```text
ChoDogyuScene/com.chodogyu.scene/Documentation~/index.md
```

버전 변경 사항:

```text
ChoDogyuScene/com.chodogyu.scene/CHANGELOG.md
```

Basic Usage Sample 안내:

```text
ChoDogyuScene/com.chodogyu.scene/Samples~/BasicUsage/README.md
```

---

## 버전

현재 Package Version:

```text
1.0.0
```

Package:

```text
com.chodogyu.scene
```

Runtime Assembly:

```text
CDG.Scene
```

Editor Assembly:

```text
CDG.Scene.Editor
```
