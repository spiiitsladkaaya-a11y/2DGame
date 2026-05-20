using System.Collections;
using NeonDrift.Audio;
using NeonDrift.Core;
using NeonDrift.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace NeonDrift.UI
{
    public sealed class UIManager : MonoBehaviour
    {
        private GameManager manager;
        private AudioManager audioManager;
        private Text scoreText;
        private Text highScoreText;
        private Text finalScoreText;
        private Text finalBestText;
        private CanvasGroup startGroup;
        private RectTransform startPanel;
        private CanvasGroup pauseGroup;
        private RectTransform pausePanel;
        private CanvasGroup gameOverGroup;
        private RectTransform scoreChip;
        private RectTransform gameOverPanel;
        private Coroutine gameOverRoutine;
        private float scorePunch;
        private Font font;

        public void Configure(GameManager gameManager, AudioManager audio)
        {
            manager = gameManager;
            audioManager = audio;
            font = Font.CreateDynamicFontFromOSFont("Arial", 18);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            BuildCanvas();

            manager.ScoreChanged += UpdateScore;
            manager.GameOverStarted += ShowGameOver;
            manager.RunRestarted += HideGameOver;
            manager.PauseChanged += SetPauseVisible;
        }

        private void OnDestroy()
        {
            if (manager == null)
                return;

            manager.ScoreChanged -= UpdateScore;
            manager.GameOverStarted -= ShowGameOver;
            manager.RunRestarted -= HideGameOver;
            manager.PauseChanged -= SetPauseVisible;
        }

        private void Update()
        {
            scorePunch = Mathf.MoveTowards(scorePunch, 0f, Time.unscaledDeltaTime * 5f);
            if (scoreChip != null)
                scoreChip.localScale = Vector3.one * (1f + scorePunch * 0.08f);

            if (manager != null && manager.State == GameState.MainMenu && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                StartGameFromButton();

            if (manager != null && (manager.State == GameState.Playing || manager.State == GameState.Paused) && Input.GetKeyDown(KeyCode.Escape))
            {
                audioManager.PlayButton();
                manager.TogglePause();
            }

            if (manager != null && manager.State == GameState.GameOver && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R)))
                RestartFromButton();
        }

        private void BuildCanvas()
        {
            var canvasObject = new GameObject("HUD Canvas");
            canvasObject.transform.SetParent(transform, false);
            canvasObject.layer = LayerMask.NameToLayer("UI");

            var canvasRect = canvasObject.AddComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            BuildHud(canvasObject.transform);
            BuildStartScreen(canvasObject.transform);
            BuildPauseScreen(canvasObject.transform);
            BuildGameOver(canvasObject.transform);
        }

        private void BuildHud(Transform parent)
        {
            scoreChip = CreatePanel("HUD - Score Panel", parent, new Vector2(18f, -18f), new Vector2(260f, 76f), new Vector2(0f, 1f), GamePalette.Panel.WithAlpha(0.96f));

            var accent = CreateImage("Accent", scoreChip, GamePalette.Mint.WithAlpha(0.95f));
            var accentRect = accent.rectTransform;
            accentRect.anchorMin = new Vector2(0f, 0.5f);
            accentRect.anchorMax = new Vector2(0f, 0.5f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.anchoredPosition = new Vector2(13f, 0f);
            accentRect.sizeDelta = new Vector2(5f, 46f);

            var scoreLabel = CreateText("Score Label", scoreChip, "SCORE", 12, FontStyle.Bold, TextAnchor.UpperLeft, GamePalette.MutedText);
            scoreLabel.rectTransform.anchorMin = Vector2.zero;
            scoreLabel.rectTransform.anchorMax = Vector2.one;
            scoreLabel.rectTransform.offsetMin = new Vector2(34f, 10f);
            scoreLabel.rectTransform.offsetMax = new Vector2(-14f, -10f);

            scoreText = CreateText("Score Text", scoreChip, "0000", 36, FontStyle.Bold, TextAnchor.LowerLeft, GamePalette.Text);
            scoreText.rectTransform.anchorMin = Vector2.zero;
            scoreText.rectTransform.anchorMax = Vector2.one;
            scoreText.rectTransform.offsetMin = new Vector2(34f, 8f);
            scoreText.rectTransform.offsetMax = new Vector2(-14f, -9f);

            highScoreText = CreateText("HUD - High Score", parent, "HIGH SCORE 0000", 17, FontStyle.Bold, TextAnchor.UpperRight, GamePalette.Gold);
            highScoreText.rectTransform.anchorMin = new Vector2(1f, 1f);
            highScoreText.rectTransform.anchorMax = new Vector2(1f, 1f);
            highScoreText.rectTransform.pivot = new Vector2(1f, 1f);
            highScoreText.rectTransform.anchoredPosition = new Vector2(-22f, -22f);
            highScoreText.rectTransform.sizeDelta = new Vector2(320f, 34f);

            BuildControlsPanel(parent);
        }

        private void BuildControlsPanel(Transform parent)
        {
            var panel = CreatePanel("HUD - Controls Panel", parent, new Vector2(18f, 18f), new Vector2(332f, 132f), new Vector2(0f, 0f), GamePalette.Panel.WithAlpha(0.88f));

            var title = CreateText("Controls Title", panel, "CONTROLS", 12, FontStyle.Bold, TextAnchor.UpperLeft, GamePalette.MutedText);
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(14f, 100f);
            title.rectTransform.offsetMax = new Vector2(-14f, -10f);

            CreateControlRow(panel, 14f, 74f, new[] { "W", "A", "S", "D" }, "move");
            CreateControlRow(panel, 14f, 48f, new[] { "^", "<", "v", ">" }, "move");
            CreateControlRow(panel, 14f, 22f, new[] { "MOUSE" }, "steer");
            CreateControlRow(panel, 168f, 22f, new[] { "SPACE", "R" }, "restart");
        }

        private void CreateControlRow(Transform parent, float x, float y, string[] keys, string label)
        {
            var offset = 0f;
            foreach (var key in keys)
            {
                var width = key.Length > 1 ? 52f : 24f;
                var keycap = CreatePanel($"Key {key}", parent, new Vector2(x + offset, y), new Vector2(width, 21f), new Vector2(0f, 0f), GamePalette.PanelSoft.WithAlpha(0.96f));
                var keyText = CreateText($"Key {key} Text", keycap, key, 11, FontStyle.Bold, TextAnchor.MiddleCenter, GamePalette.Text);
                keyText.rectTransform.anchorMin = Vector2.zero;
                keyText.rectTransform.anchorMax = Vector2.one;
                keyText.rectTransform.offsetMin = Vector2.zero;
                keyText.rectTransform.offsetMax = Vector2.zero;
                offset += width + 6f;
            }

            var description = CreateText($"Control {label}", parent, label, 11, FontStyle.Normal, TextAnchor.MiddleLeft, GamePalette.MutedText);
            description.rectTransform.anchorMin = new Vector2(0f, 0f);
            description.rectTransform.anchorMax = new Vector2(0f, 0f);
            description.rectTransform.pivot = new Vector2(0f, 0f);
            description.rectTransform.anchoredPosition = new Vector2(x + offset + 5f, y);
            description.rectTransform.sizeDelta = new Vector2(105f, 21f);
        }

        private void BuildStartScreen(Transform parent)
        {
            var root = new GameObject("Start Screen");
            root.transform.SetParent(parent, false);
            root.layer = LayerMask.NameToLayer("UI");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            startGroup = root.AddComponent<CanvasGroup>();
            startGroup.alpha = 1f;
            startGroup.interactable = true;
            startGroup.blocksRaycasts = true;

            var overlay = CreateImage("Start Overlay", root.transform, Color.black.WithAlpha(0.42f));
            overlay.rectTransform.anchorMin = Vector2.zero;
            overlay.rectTransform.anchorMax = Vector2.one;
            overlay.rectTransform.offsetMin = Vector2.zero;
            overlay.rectTransform.offsetMax = Vector2.zero;

            startPanel = CreatePanel("Start Panel", root.transform, Vector2.zero, new Vector2(560f, 310f), new Vector2(0.5f, 0.5f), GamePalette.Panel.WithAlpha(0.96f));

            var title = CreateText("Start Title", startPanel, "NEON DRIFT", 44, FontStyle.Bold, TextAnchor.MiddleCenter, GamePalette.Text);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -28f);
            title.rectTransform.sizeDelta = new Vector2(-52f, 58f);

            var greeting = CreateText("Start Greeting", startPanel, "Привет! Чтобы начать игру нажмите -", 22, FontStyle.Bold, TextAnchor.MiddleCenter, GamePalette.Text);
            greeting.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            greeting.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            greeting.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            greeting.rectTransform.anchoredPosition = new Vector2(0f, 48f);
            greeting.rectTransform.sizeDelta = new Vector2(-48f, 44f);

            var playButton = CreateButton("Play Button", startPanel, "Играть", StartGameFromButton);
            var playRect = playButton.GetComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0.5f, 0.5f);
            playRect.anchorMax = new Vector2(0.5f, 0.5f);
            playRect.pivot = new Vector2(0.5f, 0.5f);
            playRect.anchoredPosition = new Vector2(0f, -18f);
            playRect.sizeDelta = new Vector2(190f, 58f);

            var thanks = CreateText("Start Thanks", startPanel, "спасибо что скачали нашу игру!", 14, FontStyle.Normal, TextAnchor.MiddleCenter, GamePalette.MutedText);
            thanks.rectTransform.anchorMin = new Vector2(0f, 0f);
            thanks.rectTransform.anchorMax = new Vector2(1f, 0f);
            thanks.rectTransform.pivot = new Vector2(0.5f, 0f);
            thanks.rectTransform.anchoredPosition = new Vector2(0f, 24f);
            thanks.rectTransform.sizeDelta = new Vector2(-48f, 28f);
        }

        private void BuildPauseScreen(Transform parent)
        {
            var root = new GameObject("Pause Screen");
            root.transform.SetParent(parent, false);
            root.layer = LayerMask.NameToLayer("UI");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            pauseGroup = root.AddComponent<CanvasGroup>();
            pauseGroup.alpha = 0f;
            pauseGroup.interactable = false;
            pauseGroup.blocksRaycasts = false;

            var overlay = CreateImage("Pause Overlay", root.transform, Color.black.WithAlpha(0.34f));
            overlay.rectTransform.anchorMin = Vector2.zero;
            overlay.rectTransform.anchorMax = Vector2.one;
            overlay.rectTransform.offsetMin = Vector2.zero;
            overlay.rectTransform.offsetMax = Vector2.zero;

            pausePanel = CreatePanel("Pause Panel", root.transform, Vector2.zero, new Vector2(420f, 230f), new Vector2(0.5f, 0.5f), GamePalette.Panel.WithAlpha(0.96f));

            var title = CreateText("Pause Title", pausePanel, "ПАУЗА", 42, FontStyle.Bold, TextAnchor.MiddleCenter, GamePalette.Text);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -28f);
            title.rectTransform.sizeDelta = new Vector2(-48f, 56f);

            var subtitle = CreateText("Pause Subtitle", pausePanel, "нажмите Esc, чтобы продолжить", 17, FontStyle.Normal, TextAnchor.MiddleCenter, GamePalette.MutedText);
            subtitle.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            subtitle.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, 18f);
            subtitle.rectTransform.sizeDelta = new Vector2(-48f, 34f);

            var resumeButton = CreateButton("Resume Button", pausePanel, "ПРОДОЛЖИТЬ", ResumeFromButton);
            var resumeRect = resumeButton.GetComponent<RectTransform>();
            resumeRect.anchorMin = new Vector2(0.5f, 0f);
            resumeRect.anchorMax = new Vector2(0.5f, 0f);
            resumeRect.pivot = new Vector2(0.5f, 0f);
            resumeRect.anchoredPosition = new Vector2(0f, 30f);
            resumeRect.sizeDelta = new Vector2(220f, 52f);
        }

        private void BuildGameOver(Transform parent)
        {
            var root = new GameObject("Game Over Root");
            root.transform.SetParent(parent, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            gameOverGroup = root.AddComponent<CanvasGroup>();
            gameOverGroup.alpha = 0f;
            gameOverGroup.interactable = false;
            gameOverGroup.blocksRaycasts = false;

            var overlay = CreateImage("Game Over Overlay", root.transform, Color.black.WithAlpha(0.28f));
            overlay.rectTransform.anchorMin = Vector2.zero;
            overlay.rectTransform.anchorMax = Vector2.one;
            overlay.rectTransform.offsetMin = Vector2.zero;
            overlay.rectTransform.offsetMax = Vector2.zero;

            gameOverPanel = CreatePanel("Game Over Panel", root.transform, Vector2.zero, new Vector2(470f, 330f), new Vector2(0.5f, 0.5f), GamePalette.Panel.WithAlpha(0.95f));

            var title = CreateText("Title", gameOverPanel, "GAME OVER", 42, FontStyle.Bold, TextAnchor.MiddleCenter, GamePalette.Text);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -34f);
            title.rectTransform.sizeDelta = new Vector2(-56f, 58f);

            var subtitle = CreateText("Subtitle", gameOverPanel, "нажмите Space / R или кнопку, чтобы начать заново", 18, FontStyle.Normal, TextAnchor.MiddleCenter, GamePalette.MutedText);
            subtitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            subtitle.rectTransform.anchorMax = new Vector2(1f, 1f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, -90f);
            subtitle.rectTransform.sizeDelta = new Vector2(-56f, 36f);

            finalScoreText = CreateText("Final Score", gameOverPanel, "0", 58, FontStyle.Bold, TextAnchor.MiddleCenter, GamePalette.Gold);
            finalScoreText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            finalScoreText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            finalScoreText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            finalScoreText.rectTransform.anchoredPosition = new Vector2(0f, 16f);
            finalScoreText.rectTransform.sizeDelta = new Vector2(-56f, 82f);

            finalBestText = CreateText("Final Best", gameOverPanel, "HIGH SCORE 0000", 18, FontStyle.Bold, TextAnchor.MiddleCenter, GamePalette.MutedText);
            finalBestText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            finalBestText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            finalBestText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            finalBestText.rectTransform.anchoredPosition = new Vector2(0f, -44f);
            finalBestText.rectTransform.sizeDelta = new Vector2(-56f, 34f);

            var restartButton = CreateButton("Restart Button", gameOverPanel, "RESTART", RestartFromButton);
            var restartButtonRect = restartButton.GetComponent<RectTransform>();
            restartButtonRect.anchorMin = new Vector2(0.5f, 0f);
            restartButtonRect.anchorMax = new Vector2(0.5f, 0f);
            restartButtonRect.pivot = new Vector2(0.5f, 0f);
            restartButtonRect.anchoredPosition = new Vector2(0f, 36f);
            restartButtonRect.sizeDelta = new Vector2(210f, 58f);
        }

        private void UpdateScore(int score, int highScore)
        {
            scoreText.text = score.ToString("0000");
            highScoreText.text = $"HIGH SCORE {highScore:0000}";
            finalScoreText.text = score.ToString("0000");
            finalBestText.text = $"HIGH SCORE {highScore:0000}";
            scorePunch = 1f;
        }

        private void ShowGameOver()
        {
            if (gameOverRoutine != null)
                StopCoroutine(gameOverRoutine);

            gameOverRoutine = StartCoroutine(FadeGameOver(true));
        }

        private void HideGameOver()
        {
            if (gameOverRoutine != null)
                StopCoroutine(gameOverRoutine);

            SetPauseVisible(false);
            gameOverRoutine = StartCoroutine(FadeGameOver(false));
        }

        private void SetPauseVisible(bool visible)
        {
            if (pauseGroup == null)
                return;

            pauseGroup.alpha = visible ? 1f : 0f;
            pauseGroup.blocksRaycasts = visible;
            pauseGroup.interactable = visible;
            pausePanel.localScale = Vector3.one * (visible ? 1f : 0.97f);
        }

        private void HideStartScreen()
        {
            startGroup.blocksRaycasts = false;
            startGroup.interactable = false;
            StartCoroutine(FadeStartScreen());
        }

        private IEnumerator FadeGameOver(bool visible)
        {
            gameOverGroup.blocksRaycasts = visible;
            gameOverGroup.interactable = visible;

            var from = gameOverGroup.alpha;
            var to = visible ? 1f : 0f;
            var duration = visible ? 0.32f : 0.18f;
            var age = 0f;

            while (age < duration)
            {
                age += Time.unscaledDeltaTime;
                var t = EaseOut(Mathf.Clamp01(age / duration));
                gameOverGroup.alpha = Mathf.Lerp(from, to, t);
                gameOverPanel.localScale = Vector3.one * Mathf.Lerp(visible ? 0.94f : 1f, visible ? 1f : 0.98f, t);
                yield return null;
            }

            gameOverGroup.alpha = to;
        }

        private IEnumerator FadeStartScreen()
        {
            var from = startGroup.alpha;
            var age = 0f;
            const float duration = 0.22f;

            while (age < duration)
            {
                age += Time.unscaledDeltaTime;
                var t = EaseOut(Mathf.Clamp01(age / duration));
                startGroup.alpha = Mathf.Lerp(from, 0f, t);
                startPanel.localScale = Vector3.one * Mathf.Lerp(1f, 0.97f, t);
                yield return null;
            }

            startGroup.alpha = 0f;
        }

        private void StartGameFromButton()
        {
            audioManager.PlayButton();
            HideStartScreen();
            manager.StartRun();
        }

        private void ResumeFromButton()
        {
            audioManager.PlayButton();
            manager.Resume();
        }

        private void RestartFromButton()
        {
            audioManager.PlayButton();
            manager.Restart();
        }

        private RectTransform CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Color color)
        {
            var image = CreateImage(name, parent, color);
            image.sprite = SpriteFactory.RoundedRect(128, 128, 18, Color.white);
            image.type = Image.Type.Sliced;

            var rect = image.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.layer = LayerMask.NameToLayer("UI");
            var image = obj.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(string name, Transform parent, string value, int size, FontStyle style, TextAnchor anchor, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.layer = LayerMask.NameToLayer("UI");
            var text = obj.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, size - 8);
            text.resizeTextMaxSize = size;
            return text;
        }

        private Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var image = CreateImage(name, parent, GamePalette.Mint);
            image.sprite = SpriteFactory.RoundedRect(128, 128, 18, Color.white);
            image.type = Image.Type.Sliced;

            var button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);

            var text = CreateText("Label", image.transform, label, 19, FontStyle.Bold, TextAnchor.MiddleCenter, GamePalette.Ink);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            image.gameObject.AddComponent<JuicyButton>().Configure(image, audioManager, GamePalette.Mint, GamePalette.Cyan);
            return button;
        }

        private static float EaseOut(float value)
        {
            return 1f - Mathf.Pow(1f - value, 3f);
        }
    }
}
