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