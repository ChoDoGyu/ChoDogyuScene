# Changelog

이 문서는 ChoDogyu Scene & Loading Framework의 주요 변경 사항을 기록합니다.

## [1.0.0] - 2026-09-19

### Added

* `SceneController` 기반 Scene 관리 진입점
* Single Scene 비동기 로딩
* Additive Scene 비동기 로딩
* 비동기 Scene Unload
* Active Scene 조회 및 변경
* 현재 Scene 작업 상태 관리
* `SceneOperation` 기반 비동기 작업 표현
* Scene 작업 진행률 조회
* Load 진행률 `0 ~ 1` 정규화
* Scene 작업 시작 및 완료 이벤트
* 동일 Scene 작업 중복 요청 시 기존 작업 반환
* 다른 Scene 작업 진행 중 신규 작업 차단
* `SceneReference` 기반 Scene Asset Path 참조
* `SceneLoadMode` 및 `SceneOperationKind` Public API
* 안정적인 Scene 오류 코드 제공
* Unity Scene API와 상위 Scene 관리 로직 분리
* `SceneReference` Inspector Property Drawer
* Scene Asset 존재 여부 검증
* Build Settings 등록 상태 검증
* Runtime 및 Editor 테스트 구성
* 실제 Scene 기반 비동기 로딩 흐름 검증
* Basic Usage Sample

  * Single Load
  * Additive Load
  * Active Scene 변경
  * Scene Unload
* Unity Package Manager용 UPM 패키지 구조
