using UnityEngine;

public class DSPBassVoice : MonoBehaviour
{
    private double phase = 0.0;
    private double frequency = 0.0;
    private float amplitude = 0f;
    private float targetAmplitude = 0f;

    // envelope
    private float envelope = 0f;
    private float attackTime = 0.005f;  // 5ms attack
    private float decayTime = 0.18f;    // 180ms decay — pluck
    private float decayTimer = 0f;
    private bool isPlaying = false;

    // mix saw + sine
    private float sawMix = 0.7f;
    private float sineMix = 0.3f;

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
        decayTimer = 0f;
        isPlaying = true;
        phase = 0.0; // reset fazy dla czystego ataku
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying || frequency <= 0) return;

        double phaseIncrement = frequency / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            // envelope — attack
            if (envelope < 1f)
            {
                envelope += (1f / (attackTime * sampleRate)) * channels;
                envelope = Mathf.Min(envelope, 1f);
            }
            else
            {
                // decay — exponential dla pluck feel
                decayTimer += (float)channels / sampleRate;
                envelope = Mathf.Exp(-decayTimer / decayTime);

                if (envelope < 0.001f)
                {
                    isPlaying = false;
                    envelope = 0f;
                    break;
                }
            }

            // generuj próbkê
            float saw = (float)(2.0 * (phase - Mathf.Floor((float)phase + 0.5f)));
            float sine = Mathf.Sin((float)(2.0 * Mathf.PI * phase));

            float sample = (saw * sawMix + sine * sineMix) * envelope * volume;

            // zapisz do wszystkich kana³ów
            for (int c = 0; c < channels; c++)
            {
                data[i + c] += sample;
            }

            phase += phaseIncrement;
            if (phase >= 1.0) phase -= 1.0;
        }
    }
}