using UnityEngine;
using System;

// [주의] UkuleleNoteInfo와 UkuleleSongData는 UkuleleNoteData.cs에 정의되어 있어야 함

public class UkuleleNoteManager : MonoBehaviour
{
    public static UkuleleNoteManager Instance;

    [Header("References")]
    public AudioSource audioSource;    // 음원이 없어도 작동 가능
    public GameObject upNotePrefab;    // ↑ 화살표 프리팹
    public GameObject downNotePrefab;  // ↓ 화살표 프리팹
    public Transform spawnPoint;       // 노트가 나타날 지점
    public Transform hitZonePoint;     // 노트가 도착할 판정 지점

    [Header("Settings")]
    public TextAsset jsonFile;         // 노트 정보가 담긴 JSON
    public float noteAppearDuration = 2.0f; // 노특가 날아오는 시간

    private UkuleleSongData songData;
    private int nextNoteIndex = 0;
    private bool isGameStarted = false;
    private float startTime;           // 오디오가 없을 때의 기준 시간

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 테스트를 위해 씬 시작 시 즉시 실행
        StartGameAuto();
    }

    private void StartGameAuto()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON 파일이 할당되지 않았습니다!");
            return;
        }

        // JSON 데이터 로드
        songData = JsonUtility.FromJson<UkuleleSongData>(jsonFile.text);
        
        nextNoteIndex = 0;
        isGameStarted = true;
        startTime = Time.time;

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
        
        Debug.Log($"게임 로직 시작: {songData.songTitle}");
    }

    void Update()
    {
        if (!isGameStarted || songData == null) return;

        float currentTime = GetCurrentAudioTime();

        // 시간에 맞춰 노트 생성 루프
        if (nextNoteIndex < songData.notes.Length)
        {
            if (currentTime >= songData.notes[nextNoteIndex].time - noteAppearDuration)
            {
                SpawnNote(songData.notes[nextNoteIndex]);
                nextNoteIndex++;
            }
        }
    }

    void SpawnNote(UkuleleNoteInfo info)
    {
        GameObject prefab = (info.type == "Up") ? upNotePrefab : downNotePrefab;
        if (prefab == null) return;

        GameObject note = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        
        // Controller에 타겟 시간과 스트로크 타입(Up/Down)을 함께 전달
        var controller = note.GetComponent<UkuleleNoteController>();
        if (controller != null)
        {
            controller.Setup(info.time, spawnPoint.position, hitZonePoint.position, noteAppearDuration, info.type);
        }
    }

    // 음악 소스 유무에 관계없이 현재 진행 시간을 반환
    public float GetCurrentAudioTime()
    {
        if (audioSource != null && audioSource.clip != null && audioSource.isPlaying)
        {
            return audioSource.time;
        }
        return Time.time - startTime;
    }
}