using System.Collections.Generic;
using UnityEngine;

namespace NeonDrift.Core
{
    public static class SpriteFactory
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Circle(int size, Color color, float softEdge = 0.08f)
        {
            var key = $"circle-{size}-{ColorUtility.ToHtmlStringRGBA(color)}-{softEdge}";
            if (Cache.TryGetValue(key, out var cached))
                return cached;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var radius = size * 0.5f - 1f;
            var edge = Mathf.Max(1f, radius * softEdge);
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    var alpha = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(radius - edge, radius, distance));
                    texture.SetPixel(x, y, color.WithAlpha(color.a * alpha));
                }
            }

            texture.Apply();
            return Cache[key] = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }

        public static Sprite Diamond(int size, Color color)
        {
            var key = $"diamond-{size}-{ColorUtility.ToHtmlStringRGBA(color)}";
            if (Cache.TryGetValue(key, out var cached))
                return cached;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Mathf.Abs(x - center) + Mathf.Abs(y - center);
                    var alpha = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(center * 0.82f, center, distance));
                    texture.SetPixel(x, y, color.WithAlpha(color.a * alpha));
                }
            }

            texture.Apply();
            return Cache[key] = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }

        public static Sprite RoundedRect(int width, int height, int radius, Color color)
        {
            var key = $"rounded-{width}-{height}-{radius}-{ColorUtility.ToHtmlStringRGBA(color)}";
            if (Cache.TryGetValue(key, out var cached))
                return cached;

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var dx = Mathf.Max(radius - x, 0, x - (width - radius - 1));
                    var dy = Mathf.Max(radius - y, 0, y - (height - radius - 1));
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    var alpha = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(radius - 1f, radius + 1f, distance));
                    texture.SetPixel(x, y, color.WithAlpha(color.a * alpha));
                }
            }

            texture.Apply();
            var border = new Vector4(radius, radius, radius, radius);
            return Cache[key] = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f, 100f, 0, SpriteMeshType.FullRect, border);
        }
    }
}
