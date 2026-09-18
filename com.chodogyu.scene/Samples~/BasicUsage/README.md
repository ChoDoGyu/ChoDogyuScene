# ChoDogyu Scene & Loading Framework - Basic Usage

Scene & Loading Framework의 기본 Runtime 사용 흐름을 실제 Scene에서 확인할 수 있는 Sample입니다.

다음 기능을 확인할 수 있습니다.

- 비동기 Single Scene Load
- Additive Scene Load
- Active Scene 변경
- Active Scene Unload 차단
- Additive Scene Unload
- 현재 Scene Operation 상태 확인
- 정규화된 Loading Progress 확인

## 포함 내용

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

## 실행 준비

Unity Package Manager에서 `Basic Usage` Sample을 Import합니다.

Import된 Sample의 다음 세 Scene을 현재 Build Profile의 Scene 목록에 추가하고 모두 Enabled 상태로 설정합니다.

```text
SceneA.unity
SceneB.unity
AdditiveScene.unity
```

Scene & Loading Framework는 전체 Scene Asset Path와 Build 등록 상태를 기준으로 Scene을 검증하므로, Sample Scene도 실행 전에 Build에 등록되어 있어야 합니다.

## 실행 방법

다음 Scene을 엽니다.

```text
Scenes/SceneA.unity
```

Play Mode에 진입하면 Game View 왼쪽 상단에 Sample 조작 UI가 표시됩니다.

Sample은 실행 중 자신의 Scene 경로를 기준으로 `SceneA`, `SceneB`, `AdditiveScene`의 전체 Asset Path를 구성합니다.

따라서 Package Manager가 Sample을 다른 경로에 Import하더라도 별도의 SceneReference 경로 설정이 필요하지 않습니다.

## Single Load

다음 버튼을 사용할 수 있습니다.

```text
Load Scene A
Load Scene B
```

`Load Scene B`를 실행하면 SceneA가 제거되고 SceneB가 Single 방식으로 로드됩니다.

Sample Controller 오브젝트는 예제 흐름을 계속 조작할 수 있도록 Sample 코드에서 명시적으로 `DontDestroyOnLoad`를 사용합니다.

Scene & Loading Framework 자체가 자동으로 오브젝트를 유지하는 것은 아닙니다.

## Additive Load

다음 버튼을 실행합니다.

```text
Load Additive Scene
```

정상적으로 로드되면 다음 두 Scene이 동시에 존재합니다.

```text
SceneB
AdditiveScene
```

Additive Load는 기존 Scene을 교체하지 않고 새로운 Scene을 추가로 로드합니다.

Additive Load 직후에는 Framework가 Active Scene을 자동으로 변경하지 않습니다.

## Active Scene

다음 버튼을 사용할 수 있습니다.

```text
Set Scene B Active
Set Additive Active
```

`Set Additive Active`를 실행하면 AdditiveScene이 Active Scene이 됩니다.

`Set Scene B Active`를 실행하면 SceneB가 다시 Active Scene이 됩니다.

## Active Scene Unload 정책

AdditiveScene이 Active Scene인 상태에서:

```text
Unload Additive Scene
```

을 실행하면 다음 오류 코드로 실패하는 것이 정상입니다.

```text
SCENE_CANNOT_UNLOAD_ACTIVE
```

Framework는 현재 Active Scene을 직접 Unload하지 않습니다.

먼저 다른 로드된 Scene을 Active로 변경한 뒤 다시 Unload해야 합니다.

## Additive Scene Unload

다음 순서로 실행합니다.

```text
Set Scene B Active
Unload Additive Scene
```

그러면 AdditiveScene이 정상적으로 Unload되고 SceneB는 계속 유지됩니다.

## Runtime State

Sample UI에서는 다음 상태를 확인할 수 있습니다.

```text
Active Scene
Busy
Additive Loaded
Current Operation
Target Scene
Progress
```

### Progress

Load 작업의 Unity 진행률 `0.0 ~ 0.9`는 Framework에서 `0.0 ~ 1.0`으로 정규화됩니다.

Progress가 `1.0`이라고 해서 반드시 작업이 완료된 것은 아닙니다.

실제 완료 여부는 `SceneOperation.IsDone` 또는 `OperationCompleted` 이벤트를 기준으로 판단합니다.

## Notes

이 Sample의 `OnGUI`는 Scene Framework의 동작을 별도 UI 패키지 의존성 없이 확인하기 위한 Sample 전용 인터페이스입니다.

Scene & Loading Framework 자체는 UI, Audio, Save 또는 특정 게임 시스템에 의존하지 않습니다.

Sample의 `DontDestroyOnLoad` 사용 역시 Sample Controller를 Scene 전환 후에도 유지하기 위한 예제 코드이며 Framework의 자동 동작이 아닙니다.