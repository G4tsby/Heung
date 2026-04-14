using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>
/// XR UI pointer hover를 일정 시간 유지하면 자동 클릭을 실행합니다.
/// XR UI Input Module이 보내는 Pointer Enter/Exit 이벤트를 사용합니다.
/// </summary>
[DisallowMultipleComponent]
public class XrUiDwellClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum HapticTarget
    {
        BothHands,
        RightHand,
        LeftHand
    }

    [Header("Dwell Settings")]
    [SerializeField, Min(0.1f)]
    private float dwellDuration = 1f;

    [SerializeField]
    private bool triggerOncePerHover = true;

    [Header("Optional References")]
    [SerializeField]
    private Button targetButton;

    [SerializeField]
    private Image progressImage;

    [Header("Haptics")]
    [SerializeField]
    private bool playHapticOnDwell = true;

    [SerializeField, Range(0f, 1f)]
    private float hapticAmplitude = 0.6f;

    [SerializeField, Min(0.01f)]
    private float hapticDurationSeconds = 0.08f;

    [SerializeField]
    private HapticTarget hapticTarget = HapticTarget.BothHands;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onDwellTriggered;

    private Coroutine dwellRoutine;
    private PointerEventData cachedPointerEventData;
    private int hoverCount;

    private void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        ResetProgress();
    }

    private void OnDisable()
    {
        StopDwell();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cachedPointerEventData = eventData;
        hoverCount++;

        if (dwellRoutine == null)
            dwellRoutine = StartCoroutine(DwellRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);

        if (hoverCount == 0)
            StopDwell();
    }

    private IEnumerator DwellRoutine()
    {
        while (hoverCount > 0)
        {
            float elapsed = 0f;
            SetProgress(0f);

            while (elapsed < dwellDuration && hoverCount > 0)
            {
                elapsed += Time.unscaledDeltaTime;
                SetProgress(Mathf.Clamp01(elapsed / dwellDuration));
                yield return null;
            }

            if (hoverCount <= 0)
                break;

            TriggerClick();

            if (triggerOncePerHover)
                break;
        }

        ResetProgress();
        dwellRoutine = null;
    }

    private void TriggerClick()
    {
        if (!isActiveAndEnabled)
            return;

        if (targetButton != null && !targetButton.interactable)
            return;

        var eventSystem = EventSystem.current;
        if (eventSystem == null)
            return;

        var clickEventData = cachedPointerEventData ?? new PointerEventData(eventSystem);
        if (playHapticOnDwell)
            PlayDwellHaptic();
        ExecuteEvents.Execute(gameObject, clickEventData, ExecuteEvents.pointerClickHandler);
        onDwellTriggered?.Invoke();
    }

    private void PlayDwellHaptic()
    {
        if (hapticTarget is HapticTarget.BothHands or HapticTarget.RightHand)
            TryHapticImpulse(XRNode.RightHand, hapticAmplitude, hapticDurationSeconds);
        if (hapticTarget is HapticTarget.BothHands or HapticTarget.LeftHand)
            TryHapticImpulse(XRNode.LeftHand, hapticAmplitude, hapticDurationSeconds);
    }

    private static void TryHapticImpulse(XRNode node, float amplitude, float durationSeconds)
    {
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid)
            return;
        if (!device.TryGetHapticCapabilities(out var caps) || !caps.supportsImpulse)
            return;
        device.SendHapticImpulse(0u, amplitude, durationSeconds);
    }

    private void StopDwell()
    {
        hoverCount = 0;

        if (dwellRoutine != null)
        {
            StopCoroutine(dwellRoutine);
            dwellRoutine = null;
        }

        ResetProgress();
    }

    private void SetProgress(float value)
    {
        if (progressImage != null)
            progressImage.fillAmount = value;
    }

    private void ResetProgress()
    {
        SetProgress(0f);
    }
}
