using UnityEngine;

/// <summary>
/// 장구의 지정 타격 영역(히트존)입니다.
/// 이 영역의 Trigger에 Stick이 들어왔을 때만 JangguHitReceiver로 전달합니다.
/// </summary>
[DisallowMultipleComponent]
public class JangguHitZone : MonoBehaviour
{
    [SerializeField]
    private JangguHitReceiver receiver;

    [SerializeField]
    private string zoneId = "Center";

    private void Awake()
    {
        if (receiver == null)
            receiver = GetComponentInParent<JangguHitReceiver>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (receiver == null)
            return;

        var hitPosition = other.ClosestPoint(transform.position);
        receiver.TryHandleZoneHit(other, zoneId, hitPosition);
    }
}
