using NeonDrift.Core;
using NeonDrift.Effects;
using UnityEngine;

namespace NeonDrift.Gameplay
{
    public sealed class ObjectSpawner : MonoBehaviour
    {
        private GameManager manager;
        private EffectsFactory effects;
        private float collectibleTimer;
        private float hazardTimer;

        public void Configure(GameManager gameManager, EffectsFactory effectsFactory)
        {
            manager = gameManager;
            effects = effectsFactory;
            collectibleTimer = 0.4f;
            hazardTimer = 1.1f;
            manager.RunRestarted += ResetTimers;
        }

        private void OnDestroy()
        {
            if (manager != null)
                manager.RunRestarted -= ResetTimers;
        }

        private void Update()
        {
            if (manager == null || manager.State != GameState.Playing)
                return;

            collectibleTimer -= Time.deltaTime;
            hazardTimer -= Time.deltaTime;

            if (collectibleTimer <= 0f)
            {
                SpawnCollectible();
                collectibleTimer = Random.Range(0.48f, 0.84f) / manager.Difficulty;
            }

            if (hazardTimer <= 0f)
            {
                SpawnHazard();
                hazardTimer = Random.Range(0.7f, 1.15f) / manager.Difficulty;
            }
        }

        private void ResetTimers()
        {
            collectibleTimer = 0.35f;
            hazardTimer = 1.05f;
        }

        private void SpawnCollectible()
        {
            var item = CreateSpawnedObject<Collectible>("Signal Shard", GamePalette.Gold, false);
            item.transform.localScale = Vector3.one * Random.Range(0.42f, 0.56f);
        }

        private void SpawnHazard()
        {
            var item = CreateSpawnedObject<Hazard>("Static Bloom", GamePalette.Coral, true);
            item.transform.localScale = Vector3.one * Random.Range(0.58f, 0.82f);
        }

        private T CreateSpawnedObject<T>(string objectName, Color color, bool hazard) where T : SpawnedObject
        {
            var camera = Camera.main;
            var halfHeight = camera.orthographicSize;
            var halfWidth = halfHeight * camera.aspect;
            var position = new Vector3(Random.Range(-halfWidth + 0.7f, halfWidth - 0.7f), halfHeight + 0.9f, 0f);

            var gameObject = new GameObject(objectName);
            gameObject.transform.position = position;

            var glow = new GameObject("Glow");
            glow.transform.SetParent(gameObject.transform, false);
            glow.transform.localScale = Vector3.one * (hazard ? 2.4f : 2f);
            var glowRenderer = glow.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = SpriteFactory.Circle(128, color.WithAlpha(hazard ? 0.2f : 0.28f), 0.4f);
            glowRenderer.sortingLayerName = "VFX";
            glowRenderer.sortingOrder = hazard ? 2 : 3;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(gameObject.transform, false);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = hazard ? SpriteFactory.Circle(96, color, 0.16f) : SpriteFactory.Diamond(96, color);
            renderer.sortingLayerName = "Gameplay";
            renderer.sortingOrder = hazard ? 2 : 5;

            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = hazard ? 0.36f : 0.3f;

            var body = gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var spawned = gameObject.AddComponent<T>();
            var fallSpeed = Random.Range(hazard ? 2.6f : 2.0f, hazard ? 4.3f : 3.1f) * manager.Difficulty;
            var drift = Random.Range(-0.85f, 0.85f);
            spawned.Configure(new Vector2(drift, -fallSpeed), hazard ? 90f : 150f, effects);

            return spawned;
        }
    }
}
