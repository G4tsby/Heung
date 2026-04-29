using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 장구 타격 이벤트를 받아 게임 로직을 처리하는 진입점입니다.
/// MVP 단계에서는 로그만 출력합니다.
/// </summary>
[DisallowMultipleComponent]
public class JangguGameManager : MonoBehaviour
{
    private enum PendingHand
    {
        None,
        Left,
        Right
    }

    [SerializeField]
    private JangguHitReceiver jangguHitReceiver;

    [Header("Stick Id Mapping")]
    [SerializeField]
    private string leftStickId = "LeftStick";

    [SerializeField]
    private string rightStickId = "RightStick";

    [Header("Rhythm Decision")]
    [SerializeField, Min(0.01f)]
    private float bothHandWindow = 0.08f;

    [Header("UI")]
    [SerializeField]
    private TMP_Text resultText;

    public event System.Action<string, float> RhythmEmitted;

    private PendingHand pendingHand = PendingHand.None;
    private Coroutine pendingSingleHitRoutine;

    private void Awake()
    {
        if (jangguHitReceiver == null)
            jangguHitReceiver = FindFirstObjectByType<JangguHitReceiver>();
    }

    private void OnEnable()
    {
        if (jangguHitReceiver == null)
            return;

        // UnityEvent<HitInfo>의 직렬화 이벤트와 별개로 코드 구독을 위해 public API를 사용합니다.
        jangguHitReceiver.Subscribe(OnJangguHit);
    }

    private void OnDisable()
    {
        CancelPendingSingleHit();

        if (jangguHitReceiver == null)
            return;

        jangguHitReceiver.Unsubscribe(OnJangguHit);
    }

    private void OnJangguHit(JangguHitReceiver.HitInfo hitInfo)
    {
        if (IsLeftHit(hitInfo.stickId))
        {
            HandleHandHit(PendingHand.Left);
            return;
        }

        if (IsRightHit(hitInfo.stickId))
        {
            HandleHandHit(PendingHand.Right);
            return;
        }

        Debug.Log($"[JangguHit] Unknown stickId: {hitInfo.stickId}, zone={hitInfo.zoneId}", this);
    }

    private void HandleHandHit(PendingHand incomingHand)
    {
        if (pendingHand == PendingHand.None)
        {
            pendingHand = incomingHand;
            pendingSingleHitRoutine = StartCoroutine(EmitSingleHitAfterWindow(incomingHand));
            return;
        }

        if (pendingHand != incomingHand)
        {
            CancelPendingSingleHit();
            EmitRhythm("덩");
            return;
        }

        // 같은 손이 연속으로 들어온 경우: 판정 윈도우를 갱신해 자연스럽게 단타 출력
        CancelPendingSingleHit();
        pendingHand = incomingHand;
        pendingSingleHitRoutine = StartCoroutine(EmitSingleHitAfterWindow(incomingHand));
    }

    private IEnumerator EmitSingleHitAfterWindow(PendingHand hand)
    {
        yield return new WaitForSeconds(bothHandWindow);

        if (hand == PendingHand.Left)
            EmitRhythm("쿵");
        else if (hand == PendingHand.Right)
            EmitRhythm("덕");

        pendingHand = PendingHand.None;
        pendingSingleHitRoutine = null;
    }

    private void EmitRhythm(string syllable)
    {
        var english = ToEnglishRhythm(syllable);
        if (resultText != null)
            resultText.text = english;

        RhythmEmitted?.Invoke(english, Time.time);
        Debug.Log($"[JangguRhythm] {syllable}", this);
    }

    private static string ToEnglishRhythm(string syllable)
    {
        return syllable switch
        {
            "쿵" => "KUNG",
            "덕" => "DEOK",
            "덩" => "DEONG",
            _ => "UNKNOWN"
        };
    }

    private void CancelPendingSingleHit()
    {
        if (pendingSingleHitRoutine == null)
            return;

        StopCoroutine(pendingSingleHitRoutine);
        pendingSingleHitRoutine = null;
        pendingHand = PendingHand.None;
    }

    private bool IsLeftHit(string stickId)
    {
        return string.Equals(stickId, leftStickId, System.StringComparison.Ordinal);
    }

    private bool IsRightHit(string stickId)
    {
        return string.Equals(stickId, rightStickId, System.StringComparison.Ordinal);
    }
}
