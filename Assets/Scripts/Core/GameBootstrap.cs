using NeonDrift.Audio;
using NeonDrift.Effects;
using NeonDrift.Gameplay;
using NeonDrift.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonDrift.Core
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildScene()
        {
            if (Object.FindAnyObjectByType<GameManager>() != null)
                return;

            Application.targetFrameRate = 120;
            Time.timeScale = 1f;

            var camera = CreateCamera();
            var shake = camera.gameObject.AddComponent<ScreenShake>();

            CreateEventSystem();
            CreateBackground(camera);

            var audio = new GameObject("Audio Manager").AddComponent<AudioManager>();
            var effects = new GameObject("Effects Factory").AddComponent<EffectsFactory>();
            var manager = new GameObject("Game Manager").AddComponent<GameManager>();

            manager.Configure(audio, effects, shake);

            var player = CreatePlayer();
            manager.RegisterPlayer(player);

            var spawner = new GameObject("Object Spawner").AddComponent<ObjectSpawner>();
            spawner.Configure(manager, effects);

            var ui = new GameObject("UI Manager").AddComponent<UIManager>();
            ui.Configure(manager, audio);

            manager.ShowMainMenu();
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5.6f;
            camera.backgroundColor = GamePalette.DeepSpace;
            camera.clearFlags = CameraClearFlags.SolidColor;

            var listener = cameraObject.AddComponent<AudioListener>();
            listener.enabled = true;
            return camera;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static PlayerController CreatePlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, -2.4f, 0f);

            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = 3.5f;
            body.angularDamping = 8f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = player.AddComponent<CircleCollider2D>();
            collider.radius = 0.34f;
            collider.isTrigger = true;

            var glow = new GameObject("Soft Glow");
            glow.transform.SetParent(player.transform, false);
            glow.transform.localScale = Vector3.one * 1.9f;
            var glowRenderer = glow.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = SpriteFactory.Circle(128, GamePalette.Cyan.WithAlpha(0.24f), 0.32f);
            glowRenderer.sortingLayerName = "VFX";
            glowRenderer.sortingOrder = 1;

            var core = new GameObject("Core");
            core.transform.SetParent(player.transform, false);
            var coreRenderer = core.AddComponent<SpriteRenderer>();
            coreRenderer.sprite = SpriteFactory.Circle(96, GamePalette.Text, 0.12f);
            coreRenderer.sortingLayerName = "Gameplay";
            coreRenderer.sortingOrder = 4;
            core.transform.localScale = Vector3.one * 0.72f;

            var ring = new GameObject("Energy Ring");
            ring.transform.SetParent(player.transform, false);
            var ringRenderer = ring.AddComponent<SpriteRenderer>();
            ringRenderer.sprite = SpriteFactory.Circle(128, GamePalette.Mint.WithAlpha(0.52f), 0.2f);
            ringRenderer.sortingLayerName = "Gameplay";
            ringRenderer.sortingOrder = 3;
            ring.transform.localScale = new Vector3(1.05f, 1.05f, 1f);

            return player.AddComponent<PlayerController>();
        }

        private static void CreateBackground(Camera camera)
        {
            var background = new GameObject("Parallax Background").AddComponent<ParallaxBackground>();
            background.Configure(camera);

            var vignette = new GameObject("Vignette");
            var renderer = vignette.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Circle(256, Color.black.WithAlpha(0.28f), 0.65f);
            renderer.sortingLayerName = "Background";
            renderer.sortingOrder = 20;
            vignette.transform.position = new Vector3(0f, 0f, 8f);
            vignette.transform.localScale = new Vector3(13f, 13f, 1f);
        }
    }
}
