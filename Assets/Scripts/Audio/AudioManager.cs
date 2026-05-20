using UnityEngine;

namespace NeonDrift.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        private AudioSource sfxSource;
        private AudioSource musicSource;
        private AudioClip collect;
        private AudioClip hit;
        private AudioClip gameOver;
        private AudioClip button;
        private AudioClip music;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.75f;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = 0.22f;

            collect = Tone("Collect", 880f, 1320f, 0.12f, 0.2f, Wave.Sine);
            hit = Tone("Hit", 110f, 58f, 0.28f, 0.36f, Wave.Square);
            gameOver = Tone("Game Over", 440f, 150f, 0.72f, 0.32f, Wave.Sine);
            button = Tone("Button", 580f, 760f, 0.09f, 0.15f, Wave.Triangle);
            music = MusicLoop();
        }

        public void PlayMusic()
        {
            if (musicSource.isPlaying)
                return;

            musicSource.clip = music;
            musicSource.Play();
        }

        public void PlayCollect() => Play(collect, 0.8f, Random.Range(0.94f, 1.08f));
        public void PlayHit() => Play(hit, 0.75f, Random.Range(0.92f, 1.02f));
        public void PlayGameOver() => Play(gameOver, 0.65f, 1f);
        public void PlayButton() => Play(button, 0.55f, 1f);

        private void Play(AudioClip clip, float volume, float pitch)
        {
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, volume);
        }

        private static AudioClip Tone(string name, float startFrequency, float endFrequency, float seconds, float volume, Wave wave)
        {
            const int sampleRate = 44100;
            var samples = Mathf.CeilToInt(sampleRate * seconds);
            var data = new float[samples];
            var phase = 0f;

            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)samples;
                var frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                phase += frequency / sampleRate;
                var envelope = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * Mathf.Pow(1f - t, 0.35f);
                data[i] = Oscillate(phase, wave) * envelope * volume;
            }

            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MusicLoop()
        {
            const int sampleRate = 44100;
            const float seconds = 12f;
            var samples = Mathf.CeilToInt(sampleRate * seconds);
            var data = new float[samples];
            var notes = new[] { 220f, 277.18f, 329.63f, 415.3f, 329.63f, 277.18f, 246.94f, 329.63f };

            for (var i = 0; i < samples; i++)
            {
                var time = i / (float)sampleRate;
                var beat = Mathf.FloorToInt(time * 2f) % notes.Length;
                var note = notes[beat];
                var local = Mathf.Repeat(time * 2f, 1f);
                var pulse = Mathf.SmoothStep(0f, 1f, 1f - local);
                var pad = Mathf.Sin(time * Mathf.PI * 2f * note * 0.5f) * 0.08f;
                var pluck = Mathf.Sin(time * Mathf.PI * 2f * note) * pulse * 0.11f;
                var shimmer = Mathf.Sin(time * Mathf.PI * 2f * note * 2f) * pulse * 0.035f;
                data[i] = (pad + pluck + shimmer) * 0.6f;
            }

            var clip = AudioClip.Create("Neon Drift Loop", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Oscillate(float phase, Wave wave)
        {
            phase = Mathf.Repeat(phase, 1f);
            switch (wave)
            {
                case Wave.Square:
                    return phase < 0.5f ? 1f : -1f;
                case Wave.Triangle:
                    return 1f - 4f * Mathf.Abs(Mathf.Round(phase - 0.25f) - (phase - 0.25f));
                default:
                    return Mathf.Sin(phase * Mathf.PI * 2f);
            }
        }

        private enum Wave
        {
            Sine,
            Square,
            Triangle
        }
    }
}
