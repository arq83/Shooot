using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    //[Range(0f, 1f)] public float sfxVolume = 1f;
    //[Range(0f, 1f)] public float musicVolume = 0.5f;
    [SerializeField]private float sfxVolume = 1f;
    [SerializeField]private float musicVolume = 0.5f;

    public float SfxVolume => sfxVolume;
    public float MusicVolume => musicVolume;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

         

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.clip = GenerateMusic();
        musicSource.Play();
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

    public void PlayShoot()
    {
        sfxSource.volume = sfxVolume;
        //var clip = GenerateShoot();
        //Debug.Log($"PlayShoot — clip: {clip != null}, sfxSource: {sfxSource != null}, volume: {sfxVolume}");
        sfxSource.PlayOneShot(GenerateShoot());
        MarkovBassSequencer.Instance?.TriggerFromShot();
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

    // --- GENERATORY ---

    private AudioClip GenerateHit()
    {
        int sample = 2205;
        float[] data = new float[sample];
        System.Random rng = new System.Random();
        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;
            data[i] = ((float)rng.NextDouble() * 2f - 1f) * (1f - t);
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateShoot()
    {
        int sample = 4410; // ~100ms - krótszy, bardziej agresywny
        float[] data = new float[sample];
        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // "zap" - zaczyna od szumu który szybko przechodzi w ton
            float noise = (UnityEngine.Random.value * 2f - 1f);
            float freq = Mathf.Lerp(600f, 80f, t * t * t); // gwałtowny zjazd
            float tone = Mathf.Sin(2 * Mathf.PI * freq * t);

            // na początku dominuje szum, potem ton
            float noiseMix = Mathf.Pow(1f - t, 3f);
            float toneMix = 1f - noiseMix;

            float env = Mathf.Pow(1f - t, 2f);
            data[i] = (noise * noiseMix + tone * toneMix) * env * 0.7f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateDie()
    {
        int sample = 22050; // ~500ms
        float[] data = new float[sample];
        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;

            // dramatyczny zjazd przez Am pentatonikę w dół
            float[] freqs = { 329.63f, 261.63f, 220f, 164.81f, 110f }; // E4→C4→A3→E3→A2
            int step = Mathf.Min((int)(t * freqs.Length), freqs.Length - 1);
            float freq = freqs[step];

            // lekkie detune dla dramatyzmu
            float detune = 1f - t * 0.04f;
            float sine = Mathf.Sin(2 * Mathf.PI * freq * detune * t);

            // subharmonic — dodaje ciężkości
            float sub = Mathf.Sin(2 * Mathf.PI * freq * 0.5f * detune * t) * 0.4f;

            // noise sweep — elektryczny trzask na początku
            float noise = (UnityEngine.Random.value * 2f - 1f) * Mathf.Pow(1f - t, 3f) * 0.3f;

            float env = Mathf.Pow(1f - t, 0.6f); // wolne zamieranie
            data[i] = (sine + sub + noise) * env * 0.5f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GeneratePickup()
    {
        int sample = 4410;
        float[] data = new float[sample];
        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;
            float freq = Mathf.Lerp(440f, 880f, t);
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * (1f - t * 0.5f);
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateClick()
    {
        int sample = 2205;
        float[] data = new float[sample];
        for (int i = 0; i < sample; i++)
        {
            float t = (float)i / sample;
            data[i] = Mathf.Sin(2 * Mathf.PI * 660f * t) * (1f - t) * 0.5f;
        }
        return MakeClip(data, sample);
    }

    private AudioClip GenerateMusic()
    {
        int sampleRate = 44100;
        int duration = sampleRate * 4;
        float[] data = new float[duration];

        // Am pentatonika — te same nuty co bas, oktawa wyżej
        float[] notes = { 220f, 261.63f, 293.66f, 329.63f, 392.00f };
        //                 A3     C4       D4       E4       G4

        for (int i = 0; i < duration; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = (int)(t * 2) % notes.Length;
            float freq = notes[noteIndex];
            float env = Mathf.Sin(Mathf.PI * ((t * 2) % 1f));
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.15f;
        }
        return MakeClip(data, duration);
    }

    private AudioClip MakeClip(float[] data, int samples)
    {
        var clip = AudioClip.Create("sfx", samples, 1, 44100, false);
        clip.SetData(data, 0);
        return clip;
    }
}