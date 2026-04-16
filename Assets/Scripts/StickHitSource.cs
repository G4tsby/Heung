using UnityEngine;

/// <summary>
/// 장구를 타격하는 주체(Stick) 식별용 컴포넌트입니다.
/// JangguHitReceiver가 이 컴포넌트를 기준으로 유효 타격을 판단합니다.
/// </summary>
[DisallowMultipleComponent]
public class StickHitSource : MonoBehaviour
{
    [SerializeField]
    private string stickId = "RightStick";

    public string StickId => stickId;
}
