using UnityEngine;

public class DSPBassVoice : MonoBehaviour
{
    private double phase = 0.0;
    private double frequency = 0.0;
    //private float amplitude = 0f;
    //private float targetAmplitude = 0f;

    // envelope
    private float envelope = 0f;
    private float attackTime = 0.003f;  // 3ms - krótszy, bardziej perkusyjny
    private float decayTime = 0.08f;    // 80ms zamiast 180ms - szybciej zamiera
    private float decayTimer = 0f;
    private bool isPlaying = false;

    // mix saw + sine
    private float sawMix = 0.7f;
    private float sineMix = 0.3f;

    //punch
    private float punchEnvelope = 0f;
    private float punchDecay = 0.04f; // 40ms punch

    // volume
    public float volume = 0.4f;

    private int sampleRate;

    void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    public void TriggerNote(float freq)
    {
        frequency = freq;
        envelope = 0f;
        punchEnvelope = 1f; // zawsze startuje z pe³n¹ si³¹
        decayTimer = 0f;
        isPlaying = true;
        phase = 0.0;
        //Debug.Log($"[Bass] TriggerNote freq={freq}, isPlaying={isPlaying}");
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying || frequency <= 0) return;

        double phaseIncrement = frequency / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            // g³ówny envelope
            if (envelope < 1f)
            {
                envelope += 1f / (attackTime * sampleRate);
                envelope = Mathf.Min(envelope, 1f);
            }
            else
            {
                decayTimer += 1f / sampleRate;
                envelope = Mathf.Exp(-decayTimer / decayTime);

                if (envelope < 0.001f)
                {
                    isPlaying = false;
                    envelope = 0f;
                    break;
                }
            }

            // punch — krótkie subbasowe uderzenie na pocz¹tku
            punchEnvelope = Mathf.Max(0f, punchEnvelope - 1f / (punchDecay * sampleRate));
            float punchSine = Mathf.Sin((float)(2.0 * Mathf.PI * phase * 0.5f)); // suboktawa
            float punch = punchSine * punchEnvelope * 0.6f;

            float saw = (float)(2.0 * (phase - Mathf.Floor((float)phase + 0.5f)));
            float sine = Mathf.Sin((float)(2.0 * Mathf.PI * phase));

            float sample = (saw * sawMix + sine * sineMix + punch) * envelope * volume;

            for (int c = 0; c < channels; c++)
                data[i + c] += sample;

            phase += phaseIncrement;
            if (phase >= 1.0) phase -= 1.0;
        }
    }
}