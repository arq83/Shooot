using UnityEngine;

public class DSPBassVoice : MonoBehaviour
{
    private double phase = 0.0;
    private double frequency = 0.0;

    private float minTimeBetweenTriggers = 0.15f;
    private float lastTriggerTime = -999f;

    private double noteStartDsp = -1.0;
    private double attackEndDsp = -1.0;

    private float attackTime = 0.008f;  // trochê wolniejszy atak
    private float decayTime = 0.4f;     // d³ugi decay — dronowy efekt
    private float punchDecay = 0.05f;   // mocniejszy punch

    private bool isPlaying = false;

    private float sawMix = 0.8f;        // wiêcej saw — bardziej agresywny
    private float sineMix = 0.2f;

    private float punchEnvelope = 0f;
    

    public float volume = 0.5f;
    private int sampleRate;

    void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0) sampleRate = 44100;
    }

    public void TriggerNote(float freq)
    {
        float now = Time.time;
        if (now - lastTriggerTime < minTimeBetweenTriggers) return;
        lastTriggerTime = now;

        frequency = freq;
        punchEnvelope = 1f;
        isPlaying = true;
        phase = 0.0;

        noteStartDsp = AudioSettings.dspTime;
        attackEndDsp = noteStartDsp + attackTime;

        Debug.Log($"[Bass] TriggerNote freq={freq} dsp={noteStartDsp:F4}");
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying || frequency <= 0) return;

        double phaseIncrement = frequency / sampleRate;
        double dspStep = 1.0 / sampleRate;
        double dspTime = AudioSettings.dspTime;

        for (int i = 0; i < data.Length; i += channels)
        {
            double elapsed = dspTime - noteStartDsp;

            float envelope;
            if (dspTime < attackEndDsp)
            {
                envelope = (float)((dspTime - noteStartDsp) / attackTime);
                envelope = Mathf.Clamp01(envelope);
            }
            else
            {
                float decayElapsed = (float)(dspTime - attackEndDsp);
                envelope = Mathf.Exp(-decayElapsed / decayTime);

                if (envelope < 0.01f)
                {
                    isPlaying = false;
                    Debug.Log($"[Bass] STOP elapsed={elapsed:F4}");
                    return;
                }
            }

            punchEnvelope = Mathf.Max(0f,
                punchEnvelope - (float)(dspStep / punchDecay));

            float punchSine = Mathf.Sin((float)(2.0 * Mathf.PI * phase * 0.5f));
            float punch = punchSine * punchEnvelope * 0.6f;

            float saw = (float)(2.0 * (phase - Mathf.Floor((float)phase + 0.5f)));
            float sine = Mathf.Sin((float)(2.0 * Mathf.PI * phase));

            float sample = (saw * sawMix + sine * sineMix + punch) * envelope * volume;

            for (int c = 0; c < channels; c++)
                data[i + c] += sample;

            phase += phaseIncrement;
            if (phase >= 1.0) phase -= 1.0;

            dspTime += dspStep;
        }
    }
}