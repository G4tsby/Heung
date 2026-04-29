using UnityEngine;

public class UkuleleNoteController : MonoBehaviour
{
    public float targetTime;      // 판정선에 도착해야 하는 목표 시간
    public string noteType;       // "Up" 또는 "Down"
    public Vector3 startPos;      // 생성 지점
    public Vector3 endPos;        // 판정 지점
    public float noteSpeedTime;   // 이동에 걸리는 총 시간

    // Manager에서 생성 시 정보를 넘겨주는 함수
    public void Setup(float tTime, Vector3 sPos, Vector3 ePos, float sTime, string type)
    {
        targetTime = tTime;
        startPos = sPos;
        endPos = ePos;
        noteSpeedTime = sTime;
        noteType = type;
    }

    void Update()
    {
        // Manager의 싱글톤 인스턴스를 통해 현재 게임 시간을 가져옴
        float currentTime = UkuleleNoteManager.Instance.GetCurrentAudioTime();
        
        // 이동 비율 계산 (보간법)
        float t = 1f - (targetTime - currentTime) / noteSpeedTime;

        if (t >= 0)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t);
        }

        // 판정선을 과하게 지나치면(1.5초 이상) 자동으로 제거 (Miss 판정 대용)
        if (t > 1.5f) 
        {
            Destroy(gameObject);
        }
    }
}