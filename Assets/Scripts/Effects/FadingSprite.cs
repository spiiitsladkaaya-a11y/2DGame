using UnityEngine;

namespace NeonDrift.Effects
{
    public sealed class FadingSprite : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private float duration = 0.35f;
        private float targetScale = 1f;
        private float age;
        private Color startColor;
        private Vector3 startScale;

        public void Configure(float life, float scale)
        {
            duration = life;
            targetScale = scale;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startColor = spriteRenderer.color;
            startScale = transform.localScale;
        }

        private void Update()
        {
            age += Time.deltaTime;
            var t = Mathf.Clamp01(age / duration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.one * targetScale, EaseOutCubic(t));
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));

            if (t >= 1f)
                Destroy(gameObject);
        }

        private static float EaseOutCubic(float value)
        {
            return 1f - Mathf.Pow(1f - value, 3f);
        }
    }
}
