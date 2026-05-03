# 장구 리듬(임시 JSON) 퀵스타트

이 문서는 아래 3가지를 한 번에 붙이는 방법을 설명합니다.

1. JSON 채보 로드
2. 3개 라인(왼/오/같이)으로 노트가 내려오기
3. 장구 입력으로 판정 + 콤보

---

## 0) 이번에 추가된 파일

- `Assets/Scripts/JangguRhythmChartPlayer.cs`
- `Assets/Resources/Charts/JangguTempChart.json`
- `Assets/Scripts/JangguGameManager.cs` (리듬 입력 이벤트 추가)

---

## 1) Hierarchy 오브젝트 만들기

씬에서 아래 순서대로 만듭니다.

### 1-1. 노트 루트

1. 빈 오브젝트 생성: `RhythmRoot`
2. 빈 오브젝트 생성: `RhythmRoot/NoteVisualRoot`
   - 생성되는 노트가 이 밑에 모입니다.

### 1-2. 라인별 Spawn/Judge 포인트

아래처럼 총 6개의 빈 오브젝트를 만듭니다.

- `RhythmRoot/LaneLeftSpawn`
- `RhythmRoot/LaneLeftJudge`
- `RhythmRoot/LaneRightSpawn`
- `RhythmRoot/LaneRightJudge`
- `RhythmRoot/LaneBothSpawn`
- `RhythmRoot/LaneBothJudge`

배치 팁:

- Spawn는 위쪽(노트 시작 위치)
- Judge는 장구를 치는 위치 근처(아래쪽)
- 각 라인은 좌/중/우로 보기 좋게 분리

---

## 2) UI 만들기

Canvas 안에 TMP 텍스트 2개를 만듭니다.

- `JudgeText` (PERFECT/GOOD/MISS 표시)
- `ComboText` (콤보 표시)

기존 `JangguGameManager.resultText`는 입력 결과(KUNG/DEOK/DEONG) 표시용으로 그대로 사용해도 됩니다.

---

## 3) 차트 플레이어 붙이기

1. `RhythmRoot` 선택
2. `JangguRhythmChartPlayer` 컴포넌트 추가
3. Inspector를 아래처럼 연결

### 3-1. References

- **Janggu Game Manager**: `JangguGameManager`가 붙은 오브젝트
- **Chart Json**: `Assets/Resources/Charts/JangguTempChart.json`
- **Music Source**: (선택) AudioSource
- **Note Visual Root**: `NoteVisualRoot`
- **Note Visual Prefab**: 비워도 됨
  - 비우면 자동으로 구체(Sphere)를 생성해서 테스트합니다.

### 3-2. Lane Paths

`Lane Paths` 배열 사이즈를 **3**으로 설정하고 각각 입력합니다.

1. Element 0
   - lane: `Left`
   - spawnPoint: `LaneLeftSpawn`
   - judgePoint: `LaneLeftJudge`
2. Element 1
   - lane: `Right`
   - spawnPoint: `LaneRightSpawn`
   - judgePoint: `LaneRightJudge`
3. Element 2
   - lane: `Both`
   - spawnPoint: `LaneBothSpawn`
   - judgePoint: `LaneBothJudge`

### 3-3. Timing 추천값

- `travelTime`: 1.5
- `perfectWindow`: 0.08
- `goodWindow`: 0.16

### 3-4. Timing 값 상세 설명 (중요)

아래 값들은 "노트가 언제 보이고", "입력을 얼마나 엄격하게 맞출지"를 결정합니다.

- `timing.bpm` (JSON 안에 있음)
  - 의미: 곡의 기본 박자 속도입니다.
  - 영향: `beat` 값을 실제 시간(초)으로 바꿀 때 사용됩니다.
  - 예시: BPM 120이면 1박 = 0.5초
  - 팁: 노트가 음악보다 계속 빠르거나 느리면 먼저 BPM을 확인하세요.

- `timing.offsetSec` (JSON 안에 있음)
  - 의미: 차트 전체를 앞/뒤로 통째로 밀어주는 시간 보정값입니다.
  - 영향: 모든 노트 판정 시각에 동일하게 더해집니다.
  - 사용법:
    - 입력이 항상 늦게 맞는 느낌이면 값을 조금 음수로 (예: `-0.02`)
    - 입력이 항상 너무 빠르게 맞는 느낌이면 값을 조금 양수로 (예: `0.02`)
  - 팁: 한 번에 크게 바꾸지 말고 `0.01 ~ 0.03` 단위로 조정하세요.

- `travelTime` (Inspector 값)
  - 의미: 노트가 Spawn에서 Judge까지 도착하는 데 걸리는 시간(초)입니다.
  - 영향:
    - 클수록 노트가 천천히 내려옴
    - 작을수록 노트가 빠르게 내려옴
  - 주의: 판정 타이밍 자체(히트 시각)는 안 바뀌고, "보여지는 속도"가 바뀝니다.

- `perfectWindow` (Inspector 값)
  - 의미: PERFECT로 인정하는 오차 허용 범위(초)입니다.
  - 예시: `0.08`이면 목표 시각 기준 ±0.08초 안에 입력 시 PERFECT
  - 팁: 처음에는 `0.08 ~ 0.10` 정도로 시작하면 좋습니다.

- `goodWindow` (Inspector 값)
  - 의미: GOOD으로 인정하는 오차 허용 범위(초)입니다.
  - 동작: `perfectWindow` 밖이지만 `goodWindow` 안이면 GOOD
  - 주의: 이 범위를 지나면 MISS 처리됩니다.
  - 팁: 처음에는 `0.14 ~ 0.20`으로 넉넉하게 시작 후 줄이세요.

권장 관계:

- 항상 `goodWindow >= perfectWindow`
- 시작 튜닝 예시:
  - `perfectWindow = 0.08`
  - `goodWindow = 0.16`
  - `offsetSec = 0.00`에서 시작해서 필요 시 미세 조정

---

## 4) 입력 연결 확인 (중요)

`JangguGameManager`는 이미 아래처럼 변환해서 이벤트를 보냅니다.

- 왼손 타격: `KUNG`
- 오른손 타격: `DEOK`
- 양손 근접 동시: `DEONG`

`JangguRhythmChartPlayer`는 이 값을 받아 다음 라인으로 판정합니다.

- `KUNG` -> Left
- `DEOK` -> Right
- `DEONG` -> Both

즉, 기존 장구 입력 세팅(`StickHitSource`, `JangguHitZone`, `JangguHitReceiver`)이 먼저 정상이어야 합니다.

---

## 5) 테스트 순서

1. Play 버튼 누르기
2. 위에서 노트가 내려오는지 확인
3. Judge 위치에 노트가 들어올 때 장구를 타격
4. `JudgeText`가 PERFECT/GOOD으로 뜨는지 확인
5. `ComboText` 숫자가 올라가는지 확인
6. 놓치면 MISS가 뜨고 콤보가 0으로 리셋되는지 확인

---

## 6) JSON 수정하는 법

파일: `Assets/Resources/Charts/JangguTempChart.json`

예시:

```json
{ "beat": 8.0, "type": "Both" }
```

- `beat`: 박자 위치
- `type`: `Left` / `Right` / `Both`
- BPM은 `timing.bpm`에서 변경
- 전체 앞/뒤 싱크는 `timing.offsetSec`으로 미세 조정

---

## 7) 자주 막히는 포인트

### 노트가 안 보임

- `Lane Paths` 연결 누락 확인
- Spawn/Judge 오브젝트 위치가 카메라 밖인지 확인
- `Chart Json` 슬롯이 비어있는지 확인

### 쳤는데 판정이 안 뜸

- 기존 입력이 `KUNG/DEOK/DEONG`으로 나오는지 먼저 확인
- `goodWindow`를 일시적으로 0.2~0.25로 늘려서 테스트

### 콤보가 계속 0

- 타이밍이 너무 늦거나 빨라 MISS가 나는 경우가 대부분
- `travelTime`과 `goodWindow`를 완화해서 먼저 성공 경험을 만든 뒤 줄이는 것이 좋습니다.
