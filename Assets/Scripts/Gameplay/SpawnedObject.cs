using NeonDrift.Effects;
using UnityEngine;

namespace NeonDrift.Gameplay
{
    public class SpawnedObject : MonoBehaviour
    {
        private Rigidbody2D body;
        private EffectsFactory effects;
        private Vector2 velocity;
        private float spinSpeed;
        private float lifeTimer = 8f;

        public void Configure(Vector2 startVelocity, float spin, EffectsFactory effectsFactory)
        {
            velocity = startVelocity;
            spinSpeed = Random.value > 0.5f ? spin : -spin;
            effects = effectsFactory;
        }

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        protected virtual void FixedUpdate()
        {
            body.linearVelocity = velocity + new Vector2(Mathf.Sin(Time.time * 1.7f + transform.position.y) * 0.35f, 0f);
            transform.Rotate(0f, 0f, spinSpeed * Time.fixedDeltaTime);
        }

        protected virtual void Update()
        {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f || transform.position.y < -7f)
                Destroy(gameObject);
        }

        protected void PopAndDestroy(Color color)
        {
            effects.Pop(transform.position, color);
            Destroy(gameObject);
        }
    }
}
