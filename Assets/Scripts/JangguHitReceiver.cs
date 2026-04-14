using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// 장구(피격 대상)에서 Trigger 충돌을 감지하고 타격 이벤트를 발행합니다.
/// </summary>
[DisallowMultipleComponent]
public class JangguHitReceiver : MonoBehaviour
{
    [System.Serializable]
    public class JangguHitUnityEvent : UnityEvent<HitInfo> { }

    [System.Serializable]
    public struct HitInfo
    {
        public string stickId;
        public string zoneId;
        public float hitTime;
        public Vector3 hitPosition;

        public HitInfo(string stickId, string zoneId, float hitTime, Vector3 hitPosition)
        {
            this.stickId = stickId;
            this.zoneId = zoneId;
            this.hitTime = hitTime;
            this.hitPosition = hitPosition;
        }
    }

    [Header("Hit Filter")]
    [SerializeField, Min(0.01f)]
    private float minHitInterval = 0.1f;

    [SerializeField]
    private string stickTag = "Stick";

    [Header("Events")]
    [SerializeField]
    private JangguHitUnityEvent onHit;

    public event System.Action<HitInfo> HitDetected;

    private readonly Dictionary<string, float> _lastHitTimeByStick = new Dictionary<string, float>();

    private bool IsValidStick(Collider other, out StickHitSource stick)
    {
        stick = null;

        if (!other.CompareTag(stickTag))
            return false;

        stick = other.GetComponentInParent<StickHitSource>();
        return stick != null;
    }

    public bool TryHandleZoneHit(Collider other, string zoneId, Vector3 hitPosition)
    {
        if (!IsValidStick(other, out var stick))
            return false;

        if (IsInCooldown(stick.StickId))
            return false;

        var hitTime = Time.time;
        _lastHitTimeByStick[stick.StickId] = hitTime;
        var hitInfo = new HitInfo(stick.StickId, zoneId, hitTime, hitPosition);
        onHit?.Invoke(hitInfo);
        HitDetected?.Invoke(hitInfo);
        return true;
    }

    private bool IsInCooldown(string stickId)
    {
        if (_lastHitTimeByStick.TryGetValue(stickId, out var lastHitTime))
            return Time.time - lastHitTime < minHitInterval;

        return false;
    }

    public void Subscribe(System.Action<HitInfo> listener)
    {
        HitDetected += listener;
    }

    public void Unsubscribe(System.Action<HitInfo> listener)
    {
        HitDetected -= listener;
    }
}
