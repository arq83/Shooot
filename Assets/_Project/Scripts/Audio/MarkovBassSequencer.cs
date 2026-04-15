using Arekntt.Core;
using UnityEngine;

public class MarkovBassSequencer : MonoBehaviour
{
    public static MarkovBassSequencer Instance { get; private set; }

    private MarkovChain<int> markov;
    private DSPBassVoice bassVoice;

    // Am pentatonika — czêstotliwoœci w Hz (oktawa basowa)
    // A2, C3, D3, E3, G3
    private float[] frequencies = new float[]
    {
        110.00f, // A2
        130.81f, // C3
        146.83f, // D3
        164.81f, // E3
        196.00f, // G3
        220.00f, // A3 — oktawa wy¿ej dla urozmaicenia
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // osobny GameObject dla DSP
        var bassGo = new GameObject("BassVoice");
        bassGo.transform.SetParent(transform);

        // potrzebuje AudioSource ¿eby OnAudioFilterRead dzia³a³
        bassGo.AddComponent<AudioSource>().playOnAwake = false;

        bassVoice = bassGo.AddComponent<DSPBassVoice>();

        BuildMarkovChain();
    }

    private void BuildMarkovChain()
    {
        // indeksy nut: 0=A2, 1=C3, 2=D3, 3=E3, 4=G3, 5=A3
        markov = new MarkovChain<int>(0);

        // A2 _ najczêœciej do C3 lub E3, rzadziej do G3
        markov.AddTransition(0, 1, 3f); // A_C (czêsto)
        markov.AddTransition(0, 3, 2f); // A_E
        markov.AddTransition(0, 4, 1f); // A_G
        markov.AddTransition(0, 5, 1f); // A_A3

        // C3 _ najczêœciej do D3 lub A2
        markov.AddTransition(1, 2, 3f); // C_D
        markov.AddTransition(1, 0, 2f); // C_A2
        markov.AddTransition(1, 3, 1f); // C_E

        // D3 _ E3 lub C3
        markov.AddTransition(2, 3, 3f); // D_E (czêsto)
        markov.AddTransition(2, 1, 2f); // D_C
        markov.AddTransition(2, 4, 1f); // D_G

        // E3 _ G3 lub A2 (rozwi¹zanie)
        markov.AddTransition(3, 4, 3f); // E_G
        markov.AddTransition(3, 0, 2f); // E_A2 (rozwi¹zanie)
        markov.AddTransition(3, 5, 1f); // E_A3

        // G3 _ A2 lub A3
        markov.AddTransition(4, 0, 2f); // G_A2
        markov.AddTransition(4, 5, 2f); // G_A3
        markov.AddTransition(4, 3, 1f); // G_E

        // A3 _ w dó³
        markov.AddTransition(5, 0, 3f); // A3_A2
        markov.AddTransition(5, 4, 2f); // A3_G3
        markov.AddTransition(5, 1, 1f); // A3_C3
    }

    public void TriggerFromShot()
    {
        int noteIndex = markov.Next();
        float freq = frequencies[noteIndex];
        bassVoice.TriggerNote(freq);
    }
}