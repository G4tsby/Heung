using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class JangguRhythmChartPlayer : MonoBehaviour
{
    private enum NoteLane
    {
        Left,
        Right,
        Both
    }

    private enum JudgeResult
    {
        Perfect,
        Good,
        Miss
    }

    [Serializable]
    private struct LanePath
    {
        public NoteLane lane;
        public Transform spawnPoint;
        public Transform judgePoint;
    }

    [Serializable]
    private class ChartMeta
    {
        public string title = "TempChart";
        public string audioResource = string.Empty;
    }

    [Serializable]
    private class ChartTiming
    {
        public float bpm = 120f;
        public float offsetSec = 0f;
    }

    [Serializable]
    private class ChartNote
    {
        public float beat;
        public string type = "Left";
    }

    [Serializable]
    private class ChartData
    {
        public ChartMeta meta = new ChartMeta();
        public ChartTiming timing = new ChartTiming();
        public ChartNote[] notes = Array.Empty<ChartNote>();
    }

    private class RuntimeNote
    {
        public int id;
        public NoteLane lane;
        public float hitTimeSec;
        public bool isJudged;
        public Transform view;
    }

    [Header("References")]
    [SerializeField]
    private JangguGameManager jangguGameManager;

    [SerializeField]
    private TextAsset chartJson;

    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private Transform noteVisualRoot;

    [SerializeField]
    private GameObject noteVisualPrefab;

    [Header("Lane Paths")]
    [SerializeField]
    private LanePath[] lanePaths = Array.Empty<LanePath>();

    [Header("Timing")]
    [SerializeField, Min(0.1f)]
    private float travelTime = 1.5f;

    [SerializeField, Min(0.01f)]
    private float perfectWindow = 0.08f;

    [SerializeField, Min(0.01f)]
    private float goodWindow = 0.16f;

    [Header("UI")]
    [SerializeField]
    private TMP_Text judgeText;

    [SerializeField]
    private TMP_Text comboText;

    private readonly Dictionary<NoteLane, LanePath> _pathByLane = new Dictionary<NoteLane, LanePath>();
    private readonly List<RuntimeNote> _allNotes = new List<RuntimeNote>();
    private readonly List<RuntimeNote> _activeNotes = new List<RuntimeNote>();

    private int _nextSpawnIndex;
    private int _combo;
    private float _songStartTime;
    private bool _isPlaying;

    private void Awake()
    {
        if (jangguGameManager == null)
            jangguGameManager = FindFirstObjectByType<JangguGameManager>();
    }

    private void OnEnable()
    {
        if (jangguGameManager != null)
            jangguGameManager.RhythmEmitted += OnRhythmInput;
    }

    private void OnDisable()
    {
        if (jangguGameManager != null)
            jangguGameManager.RhythmEmitted -= OnRhythmInput;
    }

    private void Start()
    {
        BuildLaneDictionary();
        LoadChart();
        BeginSong();
        RefreshComboUi();
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        var songTime = GetSongTime();
        SpawnDueNotes(songTime);
        UpdateNotePositions(songTime);
        AutoMissPassedNotes(songTime);
    }

    private void BuildLaneDictionary()
    {
        _pathByLane.Clear();
        foreach (var lanePath in lanePaths)
        {
            if (lanePath.spawnPoint == null || lanePath.judgePoint == null)
                continue;

            _pathByLane[lanePath.lane] = lanePath;
        }
    }

    private void LoadChart()
    {
        _allNotes.Clear();
        _activeNotes.Clear();
        _nextSpawnIndex = 0;

        if (chartJson == null || string.IsNullOrWhiteSpace(chartJson.text))
        {
            Debug.LogWarning("[JangguChart] chartJson is missing or empty.", this);
            return;
        }

        var chart = JsonUtility.FromJson<ChartData>(chartJson.text);
        if (chart == null || chart.notes == null || chart.notes.Length == 0)
        {
            Debug.LogWarning("[JangguChart] Parsed chart has no notes.", this);
            return;
        }

        var secondsPerBeat = 60f / Mathf.Max(1f, chart.timing.bpm);
        var noteId = 0;
        foreach (var note in chart.notes)
        {
            if (!TryParseLane(note.type, out var lane))
                continue;

            var hitTime = chart.timing.offsetSec + (note.beat * secondsPerBeat);
            _allNotes.Add(new RuntimeNote
            {
                id = noteId++,
                lane = lane,
                hitTimeSec = hitTime,
                isJudged = false
            });
        }

        _allNotes.Sort((a, b) => a.hitTimeSec.CompareTo(b.hitTimeSec));
        Debug.Log($"[JangguChart] Loaded notes: {_allNotes.Count}", this);
    }

    private void BeginSong()
    {
        _songStartTime = Time.time;
        _isPlaying = true;

        if (musicSource != null)
            musicSource.Play();
    }

    private float GetSongTime()
    {
        if (musicSource != null && musicSource.clip != null && musicSource.isPlaying)
            return musicSource.time;

        return Time.time - _songStartTime;
    }

    private void SpawnDueNotes(float songTime)
    {
        while (_nextSpawnIndex < _allNotes.Count)
        {
            var note = _allNotes[_nextSpawnIndex];
            if (songTime < note.hitTimeSec - travelTime)
                break;

            if (_pathByLane.TryGetValue(note.lane, out var lanePath))
            {
                note.view = CreateNoteView(note.lane, lanePath.spawnPoint.position);
                _activeNotes.Add(note);
            }

            _nextSpawnIndex++;
        }
    }

    private Transform CreateNoteView(NoteLane lane, Vector3 spawnPosition)
    {
        GameObject go;
        if (noteVisualPrefab != null)
        {
            go = Instantiate(noteVisualPrefab, spawnPosition, Quaternion.identity, noteVisualRoot);
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = spawnPosition;
            go.transform.localScale = Vector3.one * 0.12f;
            go.transform.SetParent(noteVisualRoot, true);
            var col = go.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
        }

        go.name = $"Note_{lane}";
        if (go.TryGetComponent<Renderer>(out var renderer))
            renderer.material.color = GetLaneColor(lane);

        return go.transform;
    }

    private void UpdateNotePositions(float songTime)
    {
        for (var i = _activeNotes.Count - 1; i >= 0; i--)
        {
            var note = _activeNotes[i];
            if (note.isJudged || note.view == null)
                continue;

            if (!_pathByLane.TryGetValue(note.lane, out var lanePath))
                continue;

            var spawnTime = note.hitTimeSec - travelTime;
            var t = Mathf.InverseLerp(spawnTime, note.hitTimeSec, songTime);
            note.view.position = Vector3.Lerp(lanePath.spawnPoint.position, lanePath.judgePoint.position, t);
        }
    }

    private void OnRhythmInput(string englishRhythm, float inputWorldTime)
    {
        if (!_isPlaying)
            return;

        if (!TryRhythmToLane(englishRhythm, out var lane))
            return;

        var songTime = ToSongTime(inputWorldTime);
        var note = FindBestNote(lane, songTime);
        if (note == null)
            return;

        var error = Mathf.Abs(songTime - note.hitTimeSec);
        if (error <= perfectWindow)
            ApplyJudge(note, JudgeResult.Perfect);
        else if (error <= goodWindow)
            ApplyJudge(note, JudgeResult.Good);
    }

    private RuntimeNote FindBestNote(NoteLane lane, float songTime)
    {
        RuntimeNote best = null;
        var bestError = float.MaxValue;

        foreach (var note in _activeNotes)
        {
            if (note.isJudged || note.lane != lane)
                continue;

            var error = Mathf.Abs(songTime - note.hitTimeSec);
            if (error > goodWindow || error >= bestError)
                continue;

            best = note;
            bestError = error;
        }

        return best;
    }

    private void AutoMissPassedNotes(float songTime)
    {
        for (var i = _activeNotes.Count - 1; i >= 0; i--)
        {
            var note = _activeNotes[i];
            if (note.isJudged)
                continue;

            if (songTime - note.hitTimeSec > goodWindow)
                ApplyJudge(note, JudgeResult.Miss);
        }
    }

    private void ApplyJudge(RuntimeNote note, JudgeResult judge)
    {
        note.isJudged = true;
        if (note.view != null)
            Destroy(note.view.gameObject);

        if (judge == JudgeResult.Miss)
            _combo = 0;
        else
            _combo++;

        judgeText?.SetText(judge.ToString().ToUpperInvariant());
        RefreshComboUi();
        _activeNotes.Remove(note);
    }

    private void RefreshComboUi()
    {
        if (comboText != null)
            comboText.text = $"COMBO {_combo}";
    }

    private float ToSongTime(float worldTime)
    {
        if (musicSource != null && musicSource.clip != null)
            return musicSource.time;

        return worldTime - _songStartTime;
    }

    private static bool TryParseLane(string text, out NoteLane lane)
    {
        switch (text)
        {
            case "Left":
            case "L":
                lane = NoteLane.Left;
                return true;
            case "Right":
            case "R":
                lane = NoteLane.Right;
                return true;
            case "Both":
            case "B":
                lane = NoteLane.Both;
                return true;
            default:
                lane = NoteLane.Left;
                return false;
        }
    }

    private static bool TryRhythmToLane(string rhythm, out NoteLane lane)
    {
        switch (rhythm)
        {
            case "KUNG":
                lane = NoteLane.Left;
                return true;
            case "DEOK":
                lane = NoteLane.Right;
                return true;
            case "DEONG":
                lane = NoteLane.Both;
                return true;
            default:
                lane = NoteLane.Left;
                return false;
        }
    }

    private static Color GetLaneColor(NoteLane lane)
    {
        switch (lane)
        {
            case NoteLane.Left:
                return Color.blue;
            case NoteLane.Right:
                return Color.red;
            case NoteLane.Both:
                return new Color(0.6f, 0f, 0.8f);
            default:
                return Color.white;
        }
    }
}
