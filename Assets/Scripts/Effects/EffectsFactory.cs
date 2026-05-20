using NeonDrift.Core;
using UnityEngine;

namespace NeonDrift.Effects
{
    public sealed class EffectsFactory : MonoBehaviour
    {
        private Sprite glowSprite;

        private void Awake()
        {
            glowSprite = SpriteFactory.Circle(64, Color.white, 0.2f);
        }

        public void CollectBurst(Vector3 position)
        {
            Burst(position, GamePalette.Gold, 22, 0.62f, 2.8f);
            Ring(position, GamePalette.Gold.WithAlpha(0.34f), 0.25f, 1.3f);
        }

        public void HitBurst(Vector3 position)
        {
            Burst(position, GamePalette.Coral, 46, 0.9f, 4.2f);
            Ring(position, GamePalette.Coral.WithAlpha(0.42f), 0.45f, 2.8f);
        }

        public void Pop(Vector3 position, Color color)
        {
            Burst(position, color, 12, 0.35f, 2f);
        }

        public void PlayerTrail(Vector3 position, Color color)
        {
            if (Random.value > 0.28f)
                return;

            var trail = new GameObject("Player Trail");
            trail.transform.position = position;
            trail.transform.localScale = Vector3.one * Random.Range(0.18f, 0.28f);
            var renderer = trail.AddComponent<SpriteRenderer>();
            renderer.sprite = glowSprite;
            renderer.color = color;
            renderer.sortingLayerName = "VFX";
            renderer.sortingOrder = 0;
            trail.AddComponent<FadingSprite>().Configure(0.22f, 0.45f);
        }

        private void Burst(Vector3 position, Color color, int count, float lifetime, float speed)
        {
            var burst = new GameObject("Particle Burst");
            burst.SetActive(false);
            burst.transform.position = position;

            var particles = burst.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.duration = 0.08f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.55f, lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.35f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.13f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;
            shape.radiusThickness = 0.2f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.white, 0.2f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = "VFX";
            renderer.sortingOrder = 12;

            burst.SetActive(true);
            particles.Play();
            Destroy(burst, lifetime + 0.4f);
        }

        private void Ring(Vector3 position, Color color, float startScale, float endScale)
        {
            var ring = new GameObject("Impact Ring");
            ring.transform.position = position;
            ring.transform.localScale = Vector3.one * startScale;

            var renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Circle(128, color, 0.48f);
            renderer.sortingLayerName = "VFX";
            renderer.sortingOrder = 11;

            ring.AddComponent<FadingSprite>().Configure(0.35f, endScale);
        }
    }
}
