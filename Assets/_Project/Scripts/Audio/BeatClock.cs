using UnityEngine;

public class BeatClock : MonoBehaviour
{
    public static BeatClock Instance;

    [SerializeField] float bpm = 120f;

    public float BeatLength => 60f / bpm;
    public int CurrentBeat { get; private set; }
    public float BeatPhase { get; private set; } // 0..1

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; } // dodaj guard
        Instance = this;
        DontDestroyOnLoad(gameObject); // dodaj
    }

    void Update()
    {
        float songTime = (float)AudioSettings.dspTime;
        float beatTime = songTime / BeatLength;
        CurrentBeat = Mathf.FloorToInt(beatTime);
        BeatPhase = beatTime - CurrentBeat;
    }
}