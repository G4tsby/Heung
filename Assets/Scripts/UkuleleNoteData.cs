using System;

[Serializable]
public class UkuleleNoteInfo
{
    public float time;    // 판정선에 도착해야 하는 시간 (초)
    public string type;   // "Up" 또는 "Down"
}

[Serializable]
public class UkuleleSongData
{
    public string songTitle;
    public float bpm;
    public UkuleleNoteInfo[] notes;
}