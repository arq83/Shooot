using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource shootSource;
    private AudioSource musicSource;

    //[Range(0f, 1f)] public float sfxVolume = 1f;
    //[Range(0f, 1f)] public float musicVolume = 0.5f;
    [SerializeField]private float sfxVolume = 1f;
    [SerializeField]private float musicVolume = 0.5f;

    private float lastShotTime = 0f;
    //private float musicFadeDelay = 0.5f;   // po ilu sekundach bez strzału muzyka zaczyna cichnąć
    private float musicFadeSpeed = 3.0f;   // jak szybko cicnie (units/s)
    private float baseMusicVolume;
    private float targetMusicVolume = -1f;
    private float gameStartTime;

    private int comboCount = 0;
    private float lastComboTime = 0f;
    private float comboWindow = 1.5f;

    public float SfxVolume => sfxVolume;
    public float MusicVolume => musicVolume;

    private Coroutine heartbeatCoroutine;
    private float heartbeatInterval = 0.8f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

         

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        shootSource = gameObject.AddComponent<AudioSource>();
        shootSource.playOnAwake = false;
        shootSource.maxDistance = 1f; // nie stackuje

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.clip = GenerateMusic();
        musicSource.Play();
        musicSource.timeSamples = 0;
    }

    void Start()
    {
        baseMusicVolume = musicVolume;
        targetMusicVolume = musicVolume;
        gameStartTime = Time.time;
    }

    void Update()
    {
        if (targetMusicVolume < 0f) return;

        float timeSinceShot = Time.time - lastShotTime;

        // duck tuż po strzale, potem normalna głośność — zawsze
        float target = (timeSinceShot < 0.05f)
            ? baseMusicVolume * 0.6f
            : baseMusicVolume;

        targetMusicVolume = target;

        musicSource.volume = Mathf.MoveTowards(
            musicSource.volume,
            targetMusicVolume,
            musicFadeSpeed * Time.deltaTime);
    }

    // --- PUBLIC API ---

    //public void PlayShoot() => sfxSource.PlayOneShot(GenerateShoot(), sfxVolume);
    //public void PlayShoot()
    //{
    //    var clip = GenerateShoot();
    //    Debug.Log($"PlayShoot — clip: {clip != null}, sfxSource: {sfxSource != null}, volume: {sfxVolume}");
    //    sfxSource.PlayOneShot(clip, sfxVolume);
    //}
    //public void PlayHit() => sfxSource.PlayOneShot(GenerateHit(), sfxVolume);
    //public void PlayDie() => sfxSource.PlayOneShot(GenerateDie(), sfxVolume);
    //public void PlayPickup() => sfxSource.PlayOneShot(GeneratePickup(), sfxVolume);
    //public void PlayClick() => sfxSource.PlayOneShot(GenerateClick(), sfxVolume);

    public void StartHeartbeat(float healthPercent)
    {
        if (heartbeatCoroutine != null) StopCoroutine(heartbeatCoroutine);
        heartbeatCoroutine = StartCoroutine(HeartbeatLoop(healthPercent));
    }

    public void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    private IEnumerator HeartbeatLoop(float healthPercent)
    {
        // im mniej HP tym szybszy puls
        heartbeatInterval = Mathf.Lerp(0.4f, 1.0f, healthPercent);

        while (true)
        {
            sfxSource.PlayOneShot(GenerateHeartbeat(), sfxVolume * 0.7f);
            yield return new WaitForSeconds(heartbeatInterval);
        }
    }

    private void RegisterCombo()
    {
        if (Time.time - lastComboTime < comboWindow)
            comboCount = Mathf.Min(comboCount + 1, 8);
        else
            comboCount = 0;

        lastComboTime = Time.time;
        shootSource.pitch = 1f + comboCount * 0.08f;

        Debug.Log($"[Combo] count={comboCount} pitch={shootSource.pitch:F2}");

        // feedback przy combo milestone
        if (comboCount == 5)
            sfxSource.PlayOneShot(GenerateComboAlert(), sfxVolume * 0.5f);
    }

    private AudioClip GenerateComboAlert()
    {
        int sample = 4410;
        float[] data = new float[sample];
        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;
            float freq = Mathf.Lerp(600f, 1000f, t); // sweep w górę
            float tone = Mathf.Sin(2 * Mathf.PI * freq * t);
            float env = Mathf.Sin(Mathf.PI * t);
            data[i] = tone * env * 0.4f;
        }
        return MakeClip(data, sample);
    }

    public void PlayShoot()
    {
        lastShotTime = Time.time;
        RegisterCombo();
        shootSource.volume = sfxVolume;
        shootSource.clip = GenerateShoot();
        shootSource.Stop();
        shootSource.Play();
        MarkovBassSequencer.Instance?.TriggerFromShot();
        targetMusicVolume = baseMusicVolume * 0.6f;
    }

    public void PlayShootQuantized()
    {
        lastShotTime = Time.time;
        RegisterCombo(); // ← dodaj
        double dspNow = AudioSettings.dspTime;
        float beat = BeatClock.Instance.BeatLength;

        double nextBeatTime = Mathf.Ceil((float)(dspNow / beat)) * beat;

        shootSource.pitch = 1f + comboCount * 0.04f; //  combo pitch
        shootSource.volume = sfxVolume;
        shootSource.clip = GenerateShoot();
        shootSource.Stop();
        shootSource.PlayScheduled(nextBeatTime); //  shootSource zamiast sfxSource

        MarkovBassSequencer.Instance?.TriggerOnBeat();
        targetMusicVolume = baseMusicVolume * 0.6f; //  dodaj duck
    }

    public void PlayShootGroove()
    {
        // NIE rejestruj combo tutaj — deleguje do PlayShoot/Quantized
        lastShotTime = Time.time;
        float phase = BeatClock.Instance.BeatPhase;

        if (phase < 0.15f || phase > 0.85f)
            PlayShootQuantized();
        else
            PlayShoot();
    }

    public void PlayShootRhythmic()
    {
        lastShotTime = Time.time;
        RegisterCombo();
        // mnożenie — rytm i combo współpracują
        shootSource.pitch = GetShootPitch() * (1f + comboCount * 0.08f);
        shootSource.volume = sfxVolume;
        shootSource.clip = GenerateShoot();
        shootSource.Stop();
        shootSource.Play();
        MarkovBassSequencer.Instance?.TriggerFromShot();
        targetMusicVolume = baseMusicVolume * 0.6f;
    }

    float GetShootPitch()
    {
        float p = BeatClock.Instance.BeatPhase;

        if (p < 0.1f) return 1.2f; // kick
        if (p < 0.5f) return 0.9f; // off
        return 1.0f;
    }


    public void PlayHit()
    {
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(GenerateHit());
    }

    public void PlayDie()
    {
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(GenerateDie());
    }

    public void PlayPickup()
    {
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(GeneratePickup());
    }

    public void PlayClick()
    {
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(GenerateClick());
    } 

    public void SetSfxVolume(float v)
    {
        sfxVolume = v;
        sfxSource.volume = v;
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        musicSource.volume = v;
    }

    public void PlayLowAmmo()
    {
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(GenerateLowAmmo());
    }

    public void PlayRespawn()
    {
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(GenerateRespawn());
    }

    public void PlayKillConfirm()
    {
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(GenerateKillConfirm());
    }

    public void PlayBulletWhizz()
    {
        sfxSource.volume = sfxVolume * 0.4f;
        sfxSource.PlayOneShot(GenerateWhizz());
    }


    // --- GENERATORY ---

    private AudioClip GenerateHit()
    {
        int sample = 4410; // ~100ms
        float[] data = new float[sample];

        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // tępy uderzenie w metal
            float thud = Mathf.Sin(2 * Mathf.PI * 120f * t)
                * Mathf.Exp(-t * 15f);

            // krótki szum uderzenia
            float noise = (UnityEngine.Random.value * 2f - 1f)
                * Mathf.Pow(1f - t, 4f);

            // metaliczne echo
            float ring = Mathf.Sin(2 * Mathf.PI * 800f * t)
                * Mathf.Exp(-t * 25f) * 0.3f;

            float env = Mathf.Pow(1f - t, 1.2f);
            data[i] = (thud * 0.6f + noise * 0.3f + ring) * env * 0.8f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateShoot()
    {
        int sample = 6615; // ~150ms
        float[] data = new float[sample];

        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // metaliczny trzask — railgun feeling
            float noise = (UnityEngine.Random.value * 2f - 1f);
            float freq = Mathf.Lerp(800f, 60f, Mathf.Pow(t, 0.4f)); // szybki zjazd
            float tone = Mathf.Sin(2 * Mathf.PI * freq * t);

            // metaliczny ring
            float metal = Mathf.Sin(2 * Mathf.PI * 1200f * t)
                * Mathf.Exp(-t * 30f); // szybko zanika

            float noiseMix = Mathf.Pow(1f - t, 2f);
            float toneMix = 1f - noiseMix;
            float env = Mathf.Pow(1f - t, 1.5f);

            data[i] = (noise * noiseMix * 0.6f
                    + tone * toneMix * 0.5f
                    + metal * 0.4f) * env;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateDie()
    {
        int sample = 66150; // 1.5 sekundy
        float[] data = new float[sample];

        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // === FAZA 1: IMPACT (pierwsze 100ms) ===
            // głuche uderzenie — potwierdza kill
            float impactEnv = Mathf.Exp(-t * 40f);
            float impact = Mathf.Sin(2 * Mathf.PI * 80f * t) * impactEnv * 1.2f;

            // metaliczny trzask na samym początku
            float crunch = (UnityEngine.Random.value * 2f - 1f)
                * Mathf.Exp(-t * 80f) * 0.8f;

            // wysoki metaliczny ping — "ding" potwierdzenia
            float ping = Mathf.Sin(2 * Mathf.PI * 1400f * t)
                * Mathf.Exp(-t * 25f) * 0.5f;

            // === FAZA 2: ZJAZD CHROMATYCZNY ===
            float[] freqs = { 164.81f, 155.56f, 146.83f, 130.81f, 123.47f, 110.00f, 82.41f };
            int step = Mathf.Min((int)(t * freqs.Length), freqs.Length - 1);
            float freq = freqs[step];
            float detune = 1f - t * 0.06f;

            float sine = Mathf.Sin(2 * Mathf.PI * freq * detune * t);
            float sub = Mathf.Sin(2 * Mathf.PI * freq * 0.5f * detune * t) * 0.5f;

            float phase = (freq * detune * t) % 1f;
            float saw = (2f * phase - 1f) * 0.3f;

            // tremolo przyspiesza przy końcu — chaos
            float tremoloRate = 8f + t * 16f;
            float tremolo = 1f - 0.4f
                * Mathf.Abs(Mathf.Sin(2 * Mathf.PI * tremoloRate * t));

            float melodicEnv = Mathf.Pow(1f - t, 0.5f) * Mathf.Max(0f, t * 8f - 0.05f);

            // === FAZA 3: NOISE SWEEP — elektryczny rozpad ===
            float noiseEnv = Mathf.Exp(-t * 6f) * Mathf.Sin(Mathf.PI * t * 2f);
            float noise = (UnityEngine.Random.value * 2f - 1f) * noiseEnv * 0.35f;

            // === MIX ===
            data[i] = Mathf.Clamp(
                impact
                + crunch
                + ping
                + (sine + sub + saw) * melodicEnv * tremolo * 0.4f
                + noise,
                -1f, 1f
            );
        }
        return MakeClip(data, sample);
    }

    private AudioClip GeneratePickup()
    {
        int sample = 8820; // ~200ms
        float[] data = new float[sample];

        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // metaliczny ding — power-up w stylu Quake
            float freq1 = 523.25f; // C5
            float freq2 = 783.99f; // G5 — kwinta
            float freq3 = 1046.5f; // C6 — oktawa

            float tone1 = Mathf.Sin(2 * Mathf.PI * freq1 * t) * Mathf.Exp(-t * 8f);
            float tone2 = Mathf.Sin(2 * Mathf.PI * freq2 * t) * Mathf.Exp(-t * 10f) * 0.6f;
            float tone3 = Mathf.Sin(2 * Mathf.PI * freq3 * t) * Mathf.Exp(-t * 14f) * 0.3f;

            // krótki szum na ataku
            float noise = (UnityEngine.Random.value * 2f - 1f)
                * Mathf.Exp(-t * 60f) * 0.3f;

            data[i] = (tone1 + tone2 + tone3 + noise) * 0.6f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateClick()
    {
        int sample = 2205; // ~50ms
        float[] data = new float[sample];

        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // metaliczne kliknięcie UI
            float tone = Mathf.Sin(2 * Mathf.PI * 440f * t) * Mathf.Exp(-t * 20f);
            float high = Mathf.Sin(2 * Mathf.PI * 880f * t) * Mathf.Exp(-t * 30f) * 0.4f;
            float noise = (UnityEngine.Random.value * 2f - 1f)
                * Mathf.Exp(-t * 80f) * 0.2f;

            data[i] = (tone + high + noise) * 0.5f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateMusic()
    {
        int sampleRate = 44100;
        int duration = sampleRate * 8; // 8 sekund pętli
        float[] data = new float[duration];

        // Dron industrialny — E2 + kwinta + dysonanse
        float baseFreq = 82.41f; // E2

        for (int i = 0; i < duration; i++)
        {
            float t = (float)i / sampleRate;

            // powolne pulsowanie (LFO ~0.3Hz)
            float lfo = 0.7f + 0.3f * Mathf.Sin(2 * Mathf.PI * 0.3f * t);

            // dron — fundament
            float drone = Mathf.Sin(2 * Mathf.PI * baseFreq * t) * 0.4f;

            // kwinta (B2) — dodaje przestrzeni
            float fifth = Mathf.Sin(2 * Mathf.PI * baseFreq * 1.5f * t) * 0.2f;

            // dysonans — F2 (pół tonu wyżej) wchodzi powoli
            float dissonance = Mathf.Sin(2 * Mathf.PI * 87.31f * t)
                * 0.15f
                * Mathf.Sin(2 * Mathf.PI * 0.1f * t); // bardzo wolne LFO

            // subharmonic — E1
            float sub = Mathf.Sin(2 * Mathf.PI * baseFreq * 0.5f * t) * 0.3f;

            // metaliczny szum w tle
            float noise = (UnityEngine.Random.value * 2f - 1f)
                * 0.04f
                * Mathf.Abs(Mathf.Sin(2 * Mathf.PI * 0.7f * t));

            // fade na końcu pętli
            float loopFade = (i > duration - 8820)
                ? (float)(duration - i) / 8820f
                : 1f;

            data[i] = (drone + fifth + dissonance + sub + noise) * lfo * loopFade * 0.5f;
        }
        return MakeClip(data, duration);
    }

    private AudioClip GenerateLowAmmo()
    {
        int sample = 11025; // ~250ms
        float[] data = new float[sample];

        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // ostrzegawczy ping — malejący
            float freq = Mathf.Lerp(300f, 200f, t);
            float tone = Mathf.Sin(2 * Mathf.PI * freq * t);

            // drugi ping z opóźnieniem
            float tone2 = (t > 0.5f)
                ? Mathf.Sin(2 * Mathf.PI * 180f * t) * (t - 0.5f) * 2f
                : 0f;

            float env = Mathf.Exp(-t * 5f);
            data[i] = (tone * 0.5f + tone2 * 0.3f) * env * 0.6f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateRespawn()
    {
        int sample = 22050; // ~500ms
        float[] data = new float[sample];

        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // wznoszący się sweep — odrodzenie
            float freq = Mathf.Lerp(82.41f, 329.63f, t * t); // E2 → E4
            float sweep = Mathf.Sin(2 * Mathf.PI * freq * t);

            // sub
            float sub = Mathf.Sin(2 * Mathf.PI * freq * 0.5f * t) * 0.4f;

            // szum elektryczny
            float noise = (UnityEngine.Random.value * 2f - 1f)
                * Mathf.Exp(-t * 8f) * 0.2f;

            // narastające LFO
            float lfo = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 6f * t);
            float env = t * t; // narasta

            data[i] = (sweep + sub + noise) * env * lfo * 0.4f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateKillConfirm()
    {
        int sample = 6615; // 150ms
        float[] data = new float[sample];
        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // dwa pingi wznoszące — "tak, kill"
            float ping1 = Mathf.Sin(2 * Mathf.PI * 800f * t)
                * Mathf.Exp(-t * 25f);
            float ping2 = Mathf.Sin(2 * Mathf.PI * 1200f * t)
                * Mathf.Exp(-t * 35f) * 0.7f;

            // metaliczne uderzenie potwierdzające
            float thud = Mathf.Sin(2 * Mathf.PI * 120f * t)
                * Mathf.Exp(-t * 30f) * 0.5f;

            data[i] = (ping1 + ping2 + thud) * 0.45f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateHeartbeat()
    {
        int sample = 11025; // 250ms
        float[] data = new float[sample];

        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // pierwsze uderzenie — mocne
            float beat1 = Mathf.Sin(2 * Mathf.PI * 60f * t)
                * Mathf.Exp(-t * 20f);

            // drugie uderzenie — słabsze, po 150ms
            float beat2 = (t > 0.15f)
                ? Mathf.Sin(2 * Mathf.PI * 50f * (t - 0.15f))
                  * Mathf.Exp(-(t - 0.15f) * 30f) * 0.6f
                : 0f;

            // subbasowy oddech w tle
            float sub = Mathf.Sin(2 * Mathf.PI * 30f * t)
                * Mathf.Exp(-t * 8f) * 0.3f;

            data[i] = (beat1 + beat2 + sub) * 0.7f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateWhizz()
    {
        int sample = 6615; // 150ms
        float[] data = new float[sample];
        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;
            float freq = Mathf.Lerp(1200f, 400f, t); // Doppler w dół
            float tone = Mathf.Sin(2 * Mathf.PI * freq * t);
            float noise = (UnityEngine.Random.value * 2f - 1f) * 0.3f;
            float env = Mathf.Sin(Mathf.PI * t);
            data[i] = (tone * 0.6f + noise * 0.4f) * env * 0.5f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip MakeClip(float[] data, int samples)
    {
        var clip = AudioClip.Create("sfx", samples, 1, 44100, false);
        clip.SetData(data, 0);
        return clip;
    }
}