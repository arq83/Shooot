using Arekntt.Core;
using UnityEngine;

public class MarkovBassSequencer : MonoBehaviour
{
    public static MarkovBassSequencer Instance { get; private set; }

    private MarkovChain<int> markov;
    private DSPBassVoice bassVoice;

    // Am pentatonika — częstotliwości w Hz (oktawa basowa)
    // A2, C3, D3, E3, G3
    //private float[] frequencies = new float[]
    //{
    //    110.00f, // A2
    //    130.81f, // C3
    //    146.83f, // D3
    //    164.81f, // E3
    //    196.00f, // G3
    //    220.00f, // A3 — oktawa wyżej dla urozmaicenia
    //};

    //    private float[] frequencies = new float[]
    //{
    //    110.00f, // A2 — tonika
    //    130.81f, // C3 — tercja mała
    //    164.81f, // E3 — kwinta
    //};

    // Skala frygijska + chromatyka — klimat Quake
    // E2, F2, G#2, A2, B2, C3, D3, Eb3
    private float[] frequencies = new float[]
    {
    82.41f,  // E2  — bardzo niski fundament
    87.31f,  // F2  — pół tonu wyżej, niepokój
    103.83f, // G#2 — blue note, industrialny
    110.00f, // A2  — tonika poboczna
    123.47f, // B2  — napięcie
    130.81f, // C3  — rozwiązanie
    146.83f, // D3  — przejście
    155.56f, // Eb3 — diabelski interwał
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        var bassGo = new GameObject("BassVoice");
        bassGo.transform.SetParent(transform);

        var src = bassGo.AddComponent<AudioSource>();

        // silence clip — wymusza normalne tempo OnAudioFilterRead
        var silenceClip = AudioClip.Create(
            "silence",
            AudioSettings.outputSampleRate,
            1,
            AudioSettings.outputSampleRate,
            false  // ← NIE streaming, zwykły clip
        );
        // wypełnij zerami
        var buf = new float[AudioSettings.outputSampleRate];
        silenceClip.SetData(buf, 0);

        src.clip = silenceClip;
        src.loop = true;
        src.playOnAwake = false;
        src.volume = 0f;  // cichy
        src.Play();       // ← musi grać żeby OnAudioFilterRead działał normalnie

        bassVoice = bassGo.AddComponent<DSPBassVoice>();

        BuildMarkovChain();
    }

    private void BuildMarkovChain()
    {
        // indeksy nut: 0=A2, 1=C3, 2=D3, 3=E3, 4=G3, 5=A3
        markov = new MarkovChain<int>(0);

        // E2 — fundament, zostaje lub schodzi chromatycznie
        markov.AddTransition(0, 0, 5f); // E_E (zostań — dron)
        markov.AddTransition(0, 1, 3f); // E_F (pół tonu — niepokój)
        markov.AddTransition(0, 3, 2f); // E_A
        markov.AddTransition(0, 4, 1f); // E_B (napięcie)

        // F2 — wraca do E lub idzie w górę
        markov.AddTransition(1, 0, 4f); // F_E (rozwiązanie w dół)
        markov.AddTransition(1, 1, 3f); // F_F (zostań)
        markov.AddTransition(1, 2, 2f); // F_G# (skok)
        markov.AddTransition(1, 3, 1f); // F_A

        // G#2 — blue note, wraca do E lub A
        markov.AddTransition(2, 0, 3f); // G#_E
        markov.AddTransition(2, 3, 3f); // G#_A
        markov.AddTransition(2, 2, 2f); // G#_G# (zostań)
        markov.AddTransition(2, 1, 1f); // G#_F

        // A2 — stabilniejszy punkt
        markov.AddTransition(3, 3, 4f); // A_A (zostań)
        markov.AddTransition(3, 0, 2f); // A_E (skok w dół)
        markov.AddTransition(3, 4, 2f); // A_B
        markov.AddTransition(3, 5, 1f); // A_C

        // B2 — napięcie, rozwiązuje do C lub spada do E
        markov.AddTransition(4, 5, 3f); // B_C (rozwiązanie)
        markov.AddTransition(4, 0, 3f); // B_E (dramatyczny skok w dół)
        markov.AddTransition(4, 3, 2f); // B_A
        markov.AddTransition(4, 7, 1f); // B_Eb (diabelski interwał)

        // C3 — chwilowe uspokojenie
        markov.AddTransition(5, 3, 3f); // C_A
        markov.AddTransition(5, 6, 2f); // C_D
        markov.AddTransition(5, 0, 2f); // C_E (skok w dół)
        markov.AddTransition(5, 5, 2f); // C_C (zostań)

        // D3 — przejście
        markov.AddTransition(6, 5, 3f); // D_C
        markov.AddTransition(6, 7, 2f); // D_Eb
        markov.AddTransition(6, 3, 2f); // D_A
        markov.AddTransition(6, 0, 1f); // D_E (skok)

        // Eb3 — diabelski interwał, szybko ucieka
        markov.AddTransition(7, 0, 4f); // Eb_E (ucieczka w dół)
        markov.AddTransition(7, 3, 3f); // Eb_A
        markov.AddTransition(7, 5, 2f); // Eb_C
        markov.AddTransition(7, 6, 1f); // Eb_D


        //markov.AddTransition(0, 0, 3f);
        //markov.AddTransition(0, 1, 2f);
        //markov.AddTransition(0, 2, 1f);

        //markov.AddTransition(1, 0, 2f);
        //markov.AddTransition(1, 2, 2f);
        //markov.AddTransition(1, 1, 1f);

        //markov.AddTransition(2, 0, 3f);
        //markov.AddTransition(2, 1, 1f);


        //// A2 — tonika, wraca do siebie i do C3
        //markov.AddTransition(0, 0, 4f); // A_A (zostań)
        //markov.AddTransition(0, 1, 3f); // A_C
        //markov.AddTransition(0, 3, 1f); // A_E

        //// C3 — wraca do A lub idzie do D
        //markov.AddTransition(1, 0, 3f); // C_A
        //markov.AddTransition(1, 2, 2f); // C_D
        //markov.AddTransition(1, 1, 2f); // C_C (zostań)

        //// D3 — do E lub z powrotem do C
        //markov.AddTransition(2, 3, 3f); // D_E
        //markov.AddTransition(2, 1, 3f); // D_C
        //markov.AddTransition(2, 0, 1f); // D_A

        //// E3 — rozwiązanie do A lub G
        //markov.AddTransition(3, 0, 4f); // E_A (rozwiązanie)
        //markov.AddTransition(3, 4, 2f); // E_G
        //markov.AddTransition(3, 3, 1f); // E_E (zostań)

        //// G3 — do A
        //markov.AddTransition(4, 0, 4f); // G_A
        //markov.AddTransition(4, 3, 2f); // G_E

        //// A3 (wysoka) — używaj rzadko, tylko jako ozdobnik
        //markov.AddTransition(5, 0, 5f); // A3_A2 (zawsze w dół)
        //markov.AddTransition(5, 4, 2f); // A3_G3
    }

    public void TriggerFromShot()
    {
        int noteIndex = markov.Next();
        // unikaj wysokiej nuty przy zwykłym strzale
        //if (noteIndex == 5) noteIndex = 0;
        float freq = frequencies[noteIndex];
        Debug.Log($"[Bass] TriggerFromShot → noteIndex={noteIndex}, freq={freq}");
        bassVoice.TriggerNote(freq);
    }

    public void TriggerOnBeat()
    {
        // wersja wywołana z quantized shot — zawsze silna nuta
        int noteIndex = markov.Next();
        // bias w stronę toniki (A2 = indeks 0) na mocnych beatach
        if (BeatClock.Instance.BeatPhase < 0.1f && Random.value > 0.5f)
            noteIndex = 0;
        float freq = frequencies[noteIndex];
        bassVoice.TriggerNote(freq);
    }
}