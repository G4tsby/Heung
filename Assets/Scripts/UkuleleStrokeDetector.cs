using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UkuleleStrokeDetector : MonoBehaviour
{
    private float entryY; 
    public TMP_Text feedbackText; // 판정 결과(PERFECT, MISS 등)를 띄울 텍스트

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Mallet"))
        {
            entryY = other.transform.position.y;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Mallet"))
        {
            float exitY = other.transform.position.y;
            float movement = exitY - entryY;

            if (Mathf.Abs(movement) > 0.01f)
            {
                string strokeType = (movement < 0) ? "Down" : "Up";
                ProcessJudge(strokeType);
            }
        }
    }

    private void ProcessJudge(string playerStroke)
    {
        // 씬에 있는 모든 노트를 탐색
        UkuleleNoteController[] activeNotes = FindObjectsByType<UkuleleNoteController>(FindObjectsSortMode.None);
        UkuleleNoteController closestNote = null;
        float minDiff = float.MaxValue;

        float currentTime = UkuleleNoteManager.Instance.GetCurrentAudioTime();

        // 가장 가까운(타이밍상) 노트를 찾음
        foreach (var note in activeNotes)
        {
            float diff = Mathf.Abs(note.targetTime - currentTime);
            if (diff < minDiff)
            {
                minDiff = diff;
                closestNote = note;
            }
        }

        // 판정 로직 (오차 범위는 테스트 후 조정하세요)
        if (closestNote != null && minDiff < 0.5f) // 0.5초 이내에 노트를 찾은 경우
        {
            if (closestNote.noteType == playerStroke) // 방향 일치 확인
            {
                if (minDiff < 0.15f) DisplayFeedback("PERFECT!", Color.yellow);
                else DisplayFeedback("GOOD", Color.green);

                Destroy(closestNote.gameObject); // 맞춘 노트 제거
            }
            else
            {
                DisplayFeedback("WRONG!", Color.red);
            }
        }
    }

    void DisplayFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            // 1초 뒤에 텍스트 지우기 로직을 추가하면 더 깔끔합니다.
            CancelInvoke("ClearFeedback");
            Invoke("ClearFeedback", 1.0f);
        }
    }

    void ClearFeedback() { if (feedbackText != null) feedbackText.text = ""; }
}