using System;
using NeonDrift.Audio;
using NeonDrift.Effects;
using UnityEngine;

namespace NeonDrift.Gameplay
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public sealed class GameManager : MonoBehaviour
    {
        private const string HighScoreKey = "NeonDrift.HighScore";

        private AudioManager audioManager;
        private EffectsFactory effectsFactory;
        private ScreenShake screenShake;
        private PlayerController player;
        private float scorePulse;
        private float survivalScoreTimer;

        public event Action<int, int> ScoreChanged;
        public event Action GameOverStarted;
        public event Action RunRestarted;
        public event Action<bool> PauseChanged;

        public GameState State { get; private set; }
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public float Difficulty { get; private set; } = 1f;

        public void Configure(AudioManager audio, EffectsFactory effects, ScreenShake shake)
        {
            audioManager = audio;
            effectsFactory = effects;
            screenShake = shake;
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        public void RegisterPlayer(PlayerController controller)
        {
            player = controller;
            player.Configure(this, effectsFactory);
        }

        public void ShowMainMenu()
        {
            Time.timeScale = 1f;
            Score = 0;
            Difficulty = 1f;
            State = GameState.MainMenu;
            scorePulse = 0f;
            survivalScoreTimer = 0f;
            ScoreChanged?.Invoke(Score, HighScore);

            player.ResetPlayer();
            player.gameObject.SetActive(false);
            audioManager.PlayMusic();
        }

        public void StartRun()
        {
            Time.timeScale = 1f;
            Score = 0;
            Difficulty = 1f;
            State = GameState.Playing;
            scorePulse = 0f;
            survivalScoreTimer = 0f;
            ScoreChanged?.Invoke(Score, HighScore);
            RunRestarted?.Invoke();
            audioManager.PlayMusic();
            player.ResetPlayer();
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            foreach (var spawned in FindObjectsByType<SpawnedObject>())
                Destroy(spawned.gameObject);

            StartRun();
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
                Pause();
            else if (State == GameState.Paused)
                Resume();
        }

        public void Pause()
        {
            if (State != GameState.Playing)
                return;

            State = GameState.Paused;
            Time.timeScale = 0f;
            PauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (State != GameState.Paused)
                return;

            State = GameState.Playing;
            Time.timeScale = 1f;
            PauseChanged?.Invoke(false);
        }

        public void Collect(Vector3 position)
        {
            if (State != GameState.Playing)
                return;

            AddScore(10 + Mathf.RoundToInt(Difficulty * 2f));
            effectsFactory.CollectBurst(position);
            screenShake.Shake(0.06f, 0.055f);
            audioManager.PlayCollect();
        }

        public void Hit(Vector3 position)
        {
            if (State != GameState.Playing)
                return;

            State = GameState.GameOver;
            Time.timeScale = 1f;
            SaveHighScore();
            effectsFactory.HitBurst(position);
            screenShake.Shake(0.32f, 0.22f);
            audioManager.PlayHit();
            audioManager.PlayGameOver();
            GameOverStarted?.Invoke();
        }

        private void Update()
        {
            if (State != GameState.Playing)
                return;

            survivalScoreTimer += Time.deltaTime;
            if (survivalScoreTimer >= 0.35f)
            {
                survivalScoreTimer = 0f;
                AddScore(1);
            }

            scorePulse = Mathf.MoveTowards(scorePulse, 0f, Time.deltaTime * 2.8f);
            if (scorePulse > 0f)
                player.SetEnergyPulse(scorePulse);
        }

        private void AddScore(int amount)
        {
            Score += amount;
            Difficulty = 1f + Mathf.Clamp01(Score / 650f) * 1.65f;
            scorePulse = 1f;
            SaveHighScore();
            ScoreChanged?.Invoke(Score, HighScore);
        }

        private void SaveHighScore()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }
        }
    }
}
