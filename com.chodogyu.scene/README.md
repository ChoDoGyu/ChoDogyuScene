# ChoDogyu Scene & Loading Framework

Unity 프로젝트에서 Scene 전환, 비동기 로딩, Additive Loading, Unload 및 Active Scene 관리를 일관된 방식으로 사용할 수 있도록 구성한 범용 Scene & Loading Framework입니다.

Unity의 `SceneManager`와 `AsyncOperation`을 직접 사용하는 대신, Scene 작업의 시작·진행·완료·실패를 하나의 흐름으로 관리할 수 있도록 구성했습니다.

## 주요 기능

* Single Scene 비동기 로딩
* Additive Scene 비동기 로딩
* 비동기 Scene Unload
* Active Scene 조회 및 변경
* 현재 Scene 작업 상태 관리
* Scene 작업 진행률 조회
* Scene 작업 시작/완료 이벤트
* 동일 Scene 작업 중복 요청 처리
* 동시 Scene 작업 방지
* SceneReference 기반 Scene 참조
* 안정적인 오류 코드 제공
* Inspector에서 Scene Asset 직접 선택
* Scene Asset 및 Build 등록 상태 검증
* Basic Usage Sample 제공

## 패키지 정보

* Package Name: `com.chodogyu.scene`
* Runtime Assembly: `CDG.Scene`
* Editor Assembly: `CDG.Scene.Editor`
* Namespace: `CDG.Scene`
* Unity: `6000.3` 이상
* Dependency: `com.chodogyu.core`

## 설치

먼저 ChoDogyu Core 패키지가 설치되어 있어야 합니다.

### ChoDogyu Core v1.0.0

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

### ChoDogyu Scene & Loading Framework v1.0.0

```text
https://github.com/ChoDoGyu/ChoDogyuScene.git?path=/com.chodogyu.scene#v1.0.0
```

Unity Package Manager의 **Install package from git URL...**을 사용하여 설치합니다.

## 기본 사용 흐름

Scene 작업은 `SceneController`를 중심으로 수행합니다.

```csharp
using CDG.Scene;

SceneController sceneController = new SceneController();

SceneReference scene = new SceneReference("Assets/Scenes/Game.unity");

var result = sceneController.LoadAsync(scene);

if (result.IsSuccess)
{
    SceneOperation operation = result.Value;

    operation.Completed += () =>
    {
        // Scene 로딩 완료
    };
}
```

## Single Loading

기본 Load Mode는 `Single`입니다.

```csharp
SceneReference scene = new SceneReference("Assets/Scenes/Game.unity");

var result = sceneController.LoadAsync(scene);
```

또는 명시적으로 지정할 수 있습니다.

```csharp
var result = sceneController.LoadAsync(
    scene,
    SceneLoadMode.Single);
```

Single Loading은 기존 Scene을 교체하면서 대상 Scene을 로드합니다.

## Additive Loading

기존 Scene을 유지한 상태에서 추가 Scene을 비동기로 로드할 수 있습니다.

```csharp
var result = sceneController.LoadAsync(
    scene,
    SceneLoadMode.Additive);
```

오픈 월드의 구역 분할, 게임 플레이 Scene과 UI Scene 분리 등 여러 Scene을 동시에 유지해야 하는 구조에서 사용할 수 있습니다.

Framework 자체가 위치 기반 Streaming이나 Chunk 자동 로딩 정책을 제공하는 것은 아닙니다.

## Scene Unload

로드된 Scene은 비동기로 제거할 수 있습니다.

```csharp
var result = sceneController.UnloadAsync(scene);
```

현재 Active Scene은 직접 Unload할 수 없습니다.

필요한 경우 먼저 다른 Scene을 Active Scene으로 변경해야 합니다.

## Active Scene

현재 Active Scene을 확인할 수 있습니다.

```csharp
SceneReference activeScene = sceneController.ActiveScene;
```

이미 로드된 Scene을 Active Scene으로 변경할 수도 있습니다.

```csharp
var result = sceneController.SetActiveScene(scene);
```

## Scene 작업 상태

`SceneController`에서는 현재 작업 상태를 조회할 수 있습니다.

```csharp
bool isBusy = sceneController.IsBusy;

SceneOperation currentOperation = sceneController.CurrentOperation;

SceneReference targetScene = sceneController.TargetScene;
```

Scene 작업이 진행 중인 동안 다른 종류의 Scene 작업을 새로 시작하면 실패 결과를 반환합니다.

동일한 Scene과 동일한 작업을 다시 요청한 경우에는 기존 `SceneOperation`을 반환합니다.

## 진행률

`SceneOperation.Progress`를 통해 작업 진행률을 `0 ~ 1` 범위로 확인할 수 있습니다.

```csharp
float progress = operation.Progress;
```

Unity Scene Loading의 내부 진행률 `0 ~ 0.9` 범위는 Framework에서 `0 ~ 1` 범위로 정규화됩니다.

실제 작업 완료 여부는 `IsDone`으로 확인할 수 있습니다.

```csharp
bool isDone = operation.IsDone;
```

## 작업 이벤트

Controller 수준에서 Scene 작업의 시작과 완료를 확인할 수 있습니다.

```csharp
sceneController.OperationStarted += operation =>
{
    // Scene 작업 시작
};

sceneController.OperationCompleted += operation =>
{
    // Scene 작업 완료
};
```

개별 `SceneOperation`에서도 완료 이벤트를 받을 수 있습니다.

```csharp
operation.Completed += () =>
{
    // 해당 Scene 작업 완료
};
```

## SceneReference

`SceneReference`는 Scene 이름 대신 Scene Asset의 전체 경로를 저장합니다.

```csharp
SceneReference scene =
    new SceneReference("Assets/Scenes/Game.unity");
```

Inspector에서는 Scene Asset을 직접 선택할 수 있으며, 선택한 Scene의 Asset Path가 내부에 저장됩니다.

문자열 Scene 이름에 직접 의존하는 방식을 줄이고 Scene 참조 방식을 일관되게 유지하기 위한 타입입니다.

## Editor Validation

Editor에서는 `SceneReferenceValidator`를 통해 SceneReference의 상태를 검사할 수 있습니다.

검사 항목은 다음과 같습니다.

* SceneReference가 비어 있는지
* 실제 Scene Asset이 존재하는지
* 현재 Build Settings에 활성화된 Scene으로 등록되어 있는지

검증 과정에서 저장된 SceneReference 값을 자동으로 수정하지 않습니다.

## 오류 처리

Scene 작업은 ChoDogyu Core의 `Result` 및 `Result<T>`를 사용하여 성공과 실패를 명시적으로 반환합니다.

대표적인 오류 코드는 다음과 같습니다.

* `SCENE_INVALID_REFERENCE`
* `SCENE_NOT_IN_BUILD`
* `SCENE_ALREADY_LOADED`
* `SCENE_NOT_LOADED`
* `SCENE_OPERATION_IN_PROGRESS`
* `SCENE_INVALID_LOAD_MODE`
* `SCENE_LOAD_START_FAILED`
* `SCENE_UNLOAD_START_FAILED`
* `SCENE_CANNOT_UNLOAD_ACTIVE`
* `SCENE_SET_ACTIVE_FAILED`

외부 코드에서는 오류 메시지 문자열보다 오류 코드를 기준으로 실패 원인을 구분할 수 있습니다.

## Sample

Package Manager에서 다음 Sample을 Import할 수 있습니다.

### Basic Usage

다음 Scene 관리 흐름을 직접 확인할 수 있습니다.

* Single Load
* Additive Load
* Active Scene 변경
* Scene Unload

Sample은 Framework의 기본 사용 흐름을 확인하기 위한 최소 예제로 구성되어 있습니다.

## 설계 방향

이 패키지는 특정 게임이나 프로젝트 구조에 종속되지 않는 Scene 관리 계층을 목표로 합니다.

Unity Scene API의 세부 구현과 상위 Scene 관리 로직을 분리하고, 비동기 Scene 작업을 `SceneOperation`으로 일관되게 표현합니다.

UI Framework, Audio Framework, Save Framework 등의 다른 시스템을 필수 의존성으로 요구하지 않으며 Scene 관리 자체의 책임에 집중합니다.
