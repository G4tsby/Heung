# XrUiDwellClick 사용법

스크립트: `Assets/Scripts/XrUiDwellClick.cs`

## 1. 설치

**Button**이 붙은 오브젝트에 `XrUiDwellClick` 컴포넌트를 추가합니다.

## 2. 인스펙터

| 항목 | 설명 |
| --- | --- |
| **Dwell Duration** | 포인터를 얼마나 올려둘지(초). 예: `1`이면 1초 후 “클릭”. |
| **Trigger Once Per Hover** | 켜두면 **한 번 들어와서** dwell 완료 후 클릭 한 번이면 끝. 끄면 같은 호버 안에서 반복 가능. |
| **Target Button** | 다른 오브젝트의 버튼을 눌러야 하면 여기에 드래그. 보통은 비움. |
| **Progress Image** | (선택) 진행 링/바용 **Image**. 아래 참고. |
| **On Dwell Triggered** | dwell로 클릭이 **실행된 직후** 추가로 호출할 함수 연결 (선택). |

### 진행 표시 (Progress Image)

1. 버튼 자식 등에 **Image**를 둡니다.
2. **Image Type**을 **Filled**로 설정합니다.
3. **Fill Method**는 원하는 모양(Radial 360, Horizontal 등)에 맞게 설정합니다.
4. **Progress Image** 슬롯에 그 **Image**를 넣습니다. dwell 중 `fillAmount`가 0→1로 채워집니다.

### 햅틱 (컨트롤러 진동)

| 항목 | 설명 |
| --- | --- |
| **Play Haptic On Dwell** | 켜면 dwell 클릭 시 진동 (기본 켜짐). |
| **Haptic Amplitude** | 0~1 강도. |
| **Haptic Duration Seconds** | 최소 0.01초, 기본 0.08초. |
| **Haptic Target** | `BothHands` / `RightHand` / `LeftHand`. 한 손만 쓰면 해당 손으로 맞추면 됩니다. |

## 3. 동작 연결

**Button** 컴포넌트의 **On Click ()**에 평소처럼 원하는 함수를 연결합니다.

---

**전제:** 씬에 **EventSystem**과 XR UI용 입력 모듈이 있고, UI에 레이캐스트가 되도록 **Raycast Target** 등이 설정되어 있어야 합니다.
