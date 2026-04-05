# Heung

Unity 프로젝트입니다.

## 요구 사항

| 항목 | 버전 |
|------|------|
| **Unity Editor** | `6000.3.12f1` (Unity 6) |

[Unity Hub](https://unity.com/download)에서 동일한 에디터 버전을 설치한 뒤, 이 폴더를 프로젝트로 열면 됩니다.

## 주요 패키지 (`Packages/manifest.json` 기준)

에디터에서 **Window → Package Manager**로도 확인할 수 있습니다.

| 패키지 | 버전 | 설명 |
|--------|------|------|
| Universal RP | 17.3.0 | URP 렌더 파이프라인 |
| Input System | 1.19.0 | 새 입력 시스템 |
| XR Interaction Toolkit | 3.3.1 | XR 상호작용 |
| XR Management | 4.5.4 | XR 로더/설정 |
| OpenXR Plugin | 1.16.1 | OpenXR |
| AI Navigation | 2.0.11 | 내비메시/에이전트 |
| Timeline | 1.8.11 | 타임라인 |
| Visual Scripting | 1.9.11 | 비주얼 스크립팅 |
| Unity UI (uGUI) | 2.0.0 | UI |
| Test Framework | 1.6.0 | 에디터 테스트 |
| Multiplayer Center | 1.0.1 | 멀티플레이 관련 허브 |
| Collab Proxy | 2.11.4 | Unity 서비스 연동용 |
| Rider / Visual Studio | 각각 최신 호환 | IDE 연동 |

`com.unity.modules.*` 항목은 Unity에 포함된 **내장 모듈**이라 별도 설치 목록에 넣지 않았습니다.

## 클론 후 처음 열 때

1. Unity Hub에서 **Add**로 이 저장소를 연 폴더를 추가합니다.
2. 위 표와 같은 **에디터 버전**으로 프로젝트를 엽니다.
3. 첫 실행 시 `Library` 등이 다시 생성될 수 있어 잠시 걸릴 수 있습니다.

## Git에 올리지 않는 것

`.gitignore`에 `Library/`, `Temp/`, `Logs/`, `UserSettings/` 등 로컬·생성 파일이 제외되어 있습니다. 용량과 불필요한 충돌을 줄이기 위함입니다.
