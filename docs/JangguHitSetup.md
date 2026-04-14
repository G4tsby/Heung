# 장구 타격 시스템 설정 가이드 (MVP)

스크립트 기준:

- `Assets/Scripts/StickHitSource.cs`
- `Assets/Scripts/JangguHitZone.cs`
- `Assets/Scripts/JangguHitReceiver.cs`
- `Assets/Scripts/JangguGameManager.cs`

현재 동작:

- 왼손 타격: `쿵` (`KUNG`)
- 오른손 타격: `덕` (`DEOK`)
- 양손 근접 동시 타격: `덩` (`DEONG`)
- 결과 텍스트는 `TextMeshPro` UI에 표시

---

## 1) 오브젝트 구조 추천

예시 계층:

- `GameManager`
  - `JangguGameManager`
- `JangguRoot`
  - `JangguHitReceiver`
  - `JangguBody` (시각용 메쉬)
  - `HitZone_Left`
    - `JangguHitZone` (`zoneId = LeftHead`)
    - `Collider (Is Trigger = On)`
  - `HitZone_Right`
    - `JangguHitZone` (`zoneId = RightHead`)
    - `Collider (Is Trigger = On)`
- `LeftStick`
  - `StickHitSource` (`stickId = LeftStick`)
  - `Collider`
  - `Rigidbody (Is Kinematic=On, Use Gravity=Off)`
  - `Tag = Stick`
- `RightStick`
  - `StickHitSource` (`stickId = RightStick`)
  - `Collider`
  - `Rigidbody (Is Kinematic=On, Use Gravity=Off)`
  - `Tag = Stick`

핵심:

- 실제 판정은 `HitZone_*`에서만 받습니다.
- 장구 본체(`JangguBody`)는 시각/충돌 보조 용도로만 사용합니다.

---

## 2) 채(Stick) 설정

### 2-1. 공통 설정

각 채 오브젝트에 아래를 설정합니다.

1. `StickHitSource` 추가
2. `Collider` 추가 (예: `CapsuleCollider`)
3. `Rigidbody` 추가
   - `Is Kinematic = On`
   - `Use Gravity = Off`
4. 태그를 `Stick`으로 설정

### 2-2. 좌/우 구분

- 왼손 채: `stickId = LeftStick`
- 오른손 채: `stickId = RightStick`

`JangguGameManager`의 `leftStickId/rightStickId`와 문자열이 정확히 일치해야 합니다.

---

## 3) 장구 설정

### 3-1. Receiver 설정

장구 루트 오브젝트(`JangguRoot`)에 `JangguHitReceiver`를 추가합니다.

Inspector:

- `Min Hit Interval`: 시작값 `0.1` 권장
  - 너무 작으면 중복 타격이 많아짐
  - 너무 크면 빠른 타격이 씹힐 수 있음
- `Stick Tag`: `Stick`

### 3-2. HitZone 설정 (가장 중요)

타격을 받고 싶은 위치에 자식 오브젝트를 만들고 설정합니다.

각 HitZone에:

1. `Collider` 추가
2. `Is Trigger = On`
3. `JangguHitZone` 추가
4. `zoneId` 입력 (예: `LeftHead`, `RightHead`)

`receiver` 슬롯:

- 일반적으로 비워둬도 됩니다.
- `Awake()`에서 부모의 `JangguHitReceiver`를 자동으로 찾습니다.

---

## 4) GameManager + TMP UI 설정

1. 빈 오브젝트에 `JangguGameManager` 추가
2. `jangguHitReceiver`에 장구 루트의 `JangguHitReceiver` 연결
3. Canvas의 `TextMeshPro - Text (UI)`를 `resultText` 슬롯에 연결
4. `bothHandWindow` 조정 (시작값 `0.08`)

권장 초기값:

- `bothHandWindow = 0.08 ~ 0.12`
- `minHitInterval = 0.08 ~ 0.12`

---

## 5) 핵심 파라미터 상세 설명

### `bothHandWindow` (양손 동시 판정 창)

의미:
- 첫 번째 손 타격이 들어온 뒤, 반대손을 기다리는 시간(초)입니다.

동작:
- 대기 시간 안에 반대손 타격이 오면 `DEONG(덩)`으로 판정
- 반대손이 안 오면 단타(`KUNG` 또는 `DEOK`)로 확정

튜닝 팁:
- `DEONG`이 잘 안 나오면 값을 올립니다. (예: `0.08 -> 0.12`)
- `DEONG`이 너무 쉽게 나오면 값을 내립니다. (예: `0.12 -> 0.06`)

### `minHitInterval` (중복 타격 방지 간격)

의미:
- 같은 Stick(`stickId`)의 연속 타격을 무시하는 최소 시간(초)입니다.

동작:
- 같은 손이 너무 짧은 간격으로 들어오면 두 번째 입력을 무시
- 현재 구현은 손별 쿨타임으로 동작하므로, 왼손 입력 때문에 오른손 입력이 막히지 않습니다.

튜닝 팁:
- 떨림으로 중복 입력이 많으면 값을 올립니다. (예: `0.08 -> 0.12`)
- 빠른 연타가 씹히면 값을 내립니다. (예: `0.12 -> 0.07`)

### 두 값의 관계

- `bothHandWindow`는 "양손을 묶어 `DEONG`으로 볼지"를 결정
- `minHitInterval`는 "같은 손의 노이즈/중복을 막을지"를 결정
- 보통 두 값을 비슷하게 두고(`0.08~0.12`) 플레이 테스트로 손맛을 맞추는 것이 좋습니다.

---

## 6) 태그 생성 방법 (`Stick`)

1. 오브젝트 선택
2. Inspector 상단 `Tag` 드롭다운 > `Add Tag...`
3. `+`로 `Stick` 추가
4. `LeftStick`, `RightStick`에 `Stick` 태그 적용

---

## 7) 정상 동작 체크리스트

- Stick 태그가 `Stick`인지
- Stick마다 `StickHitSource.stickId`가 다른지
- Stick에 `Rigidbody(kinematic)`가 있는지
- HitZone Collider가 `Is Trigger=On`인지
- `JangguGameManager.resultText`에 TMP가 연결됐는지

콘솔/텍스트 결과:

- 왼손만: `KUNG`
- 오른손만: `DEOK`
- 양손 근접 동시: `DEONG`

---

## 8) 자주 발생하는 문제와 해결

### 문제 1: 타격이 전혀 안 잡힘

- 원인: Stick에 Rigidbody 없음 / 태그 불일치
- 해결: Stick에 Rigidbody 추가, 태그를 `Stick`으로 맞춤

### 문제 2: 장구 아무 데나 닿아도 입력됨

- 원인: 본체 Collider를 판정으로 같이 사용
- 해결: 판정은 `HitZone`만 사용하고, 본체는 판정 레이어에서 제외

### 문제 3: `DEONG`이 잘 안 나옴

- 원인: 동시 판정 윈도우가 너무 짧음
- 해결: `bothHandWindow`를 `0.12`까지 올려 테스트

### 문제 4: 연타가 씹히거나 너무 많이 찍힘

- 원인: `minHitInterval` 값 부적절
- 해결: 손맛에 맞게 `0.08 ~ 0.15` 범위에서 조정

---

## 9) 확장 포인트

- `JangguGameManager.EmitRhythm()`에서 사운드 재생 연결
- `zoneId` 기반으로 좌/우 면 점수 가중치 적용
- `HitInfo.hitTime`으로 콤보/정확도 판정 추가
