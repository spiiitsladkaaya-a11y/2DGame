using NeonDrift.Core;
using UnityEngine;

namespace NeonDrift.Effects
{
    public sealed class ParallaxBackground : MonoBehaviour
    {
        private Camera targetCamera;
        private Transform[] layers;
        private float[] speeds;

        public void Configure(Camera camera)
        {
            targetCamera = camera;
            Build();
        }

        private void Build()
        {
            layers = new Transform[3];
            speeds = new[] { 0.12f, 0.22f, 0.38f };

            CreateLayer(0, GamePalette.Panel.WithAlpha(0.5f), 20, 0.35f, 14);
            CreateLayer(1, GamePalette.Cyan.WithAlpha(0.13f), 12, 0.18f, 8);
            CreateLayer(2, GamePalette.Mint.WithAlpha(0.12f), 9, 0.11f, 5);
        }

        private void CreateLayer(int index, Color color, int count, float scale, int order)
        {
            var layer = new GameObject($"Parallax Layer {index + 1}").transform;
            layer.SetParent(transform, false);
            layers[index] = layer;

            for (var i = 0; i < count; i++)
            {
                var point = new GameObject("Light Point");
                point.transform.SetParent(layer, false);
                point.transform.position = new Vector3(Random.Range(-9f, 9f), Random.Range(-6f, 6f), 4f + index);
                point.transform.localScale = Vector3.one * Random.Range(scale * 0.55f, scale * 1.4f);

                var renderer = point.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteFactory.Circle(64, color, 0.22f);
                renderer.sortingLayerName = "Background";
                renderer.sortingOrder = order;
            }
        }

        private void Update()
        {
            if (targetCamera == null || layers == null)
                return;

            for (var i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                var y = Mathf.Repeat(Time.time * speeds[i], 12f);
                layer.position = new Vector3(targetCamera.transform.position.x * speeds[i] * 0.3f, -y, 0f);

                foreach (Transform child in layer)
                {
                    if (child.position.y < -6.5f)
                        child.position += Vector3.up * 12f;
                }
            }
        }
    }
}
