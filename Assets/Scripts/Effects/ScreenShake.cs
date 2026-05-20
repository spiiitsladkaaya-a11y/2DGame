using UnityEngine;

namespace NeonDrift.Effects
{
    public sealed class ScreenShake : MonoBehaviour
    {
        private Vector3 basePosition;
        private float trauma;
        private float timer;

        private void Awake()
        {
            basePosition = transform.position;
        }

        public void Shake(float strength, float duration)
        {
            trauma = Mathf.Max(trauma, strength);
            timer = Mathf.Max(timer, duration);
        }

        private void LateUpdate()
        {
            if (timer <= 0f)
            {
                transform.position = Vector3.Lerp(transform.position, basePosition, Time.deltaTime * 12f);
                trauma = Mathf.MoveTowards(trauma, 0f, Time.deltaTime * 2.5f);
                return;
            }

            timer -= Time.deltaTime;
            var amount = trauma * trauma;
            var offset = new Vector3(
                (Mathf.PerlinNoise(Time.time * 34f, 0.2f) - 0.5f) * amount,
                (Mathf.PerlinNoise(0.8f, Time.time * 38f) - 0.5f) * amount,
                0f);

            transform.position = basePosition + offset;
        }
    }
}
