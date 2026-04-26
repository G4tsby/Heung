using UnityEngine;
using TMPro; // 피드백 출력을 위해 필요

public class UkuleleStrokeDetector : MonoBehaviour
{
    private float entryY; 
    public TMP_Text debugText; // 화면에 "올리기", "내리기"를 띄워줄 텍스트

    private void OnTriggerEnter(Collider other)
    {
        //StickHitSource가 붙은 물체나 컨트롤러 태그 확인
        if (other.CompareTag("GameController") || other.GetComponent<StickHitSource>() != null)
        {
            entryY = other.transform.position.y;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GameController") || other.GetComponent<StickHitSource>() != null)
        {
            float exitY = other.transform.position.y;
            float movement = exitY - entryY;

            // 1cm 이상 움직였을 때만 스트로크로 인정
            if (Mathf.Abs(movement) > 0.01f)
            {
                if (movement < 0) OnDownStroke();
                else OnUpStroke();
            }
        }
    }

    void OnDownStroke()
    {
        Debug.Log("Down Stroke!");
        if (debugText != null) debugText.text = "↓↓ Down ↓↓";
    }

    void OnUpStroke()
    {
        Debug.Log("Up Stroke!");
        if (debugText != null) debugText.text = "↑↑ Up ↑↑";
    }
}