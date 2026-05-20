using NeonDrift.Core;
using NeonDrift.Effects;
using UnityEngine;

namespace NeonDrift.Gameplay
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float acceleration = 30f;
        [SerializeField] private float maxSpeed = 6.4f;
        [SerializeField] private float tiltAmount = 12f;

        private Rigidbody2D body;
        private GameManager manager;
        private EffectsFactory effects;
        private Vector2 input;
        private Vector3 startPosition;
        private float pulse;
        private float idleBob;

        public void Configure(GameManager gameManager, EffectsFactory effectsFactory)
        {
            manager = gameManager;
            effects = effectsFactory;
        }

        public void ResetPlayer()
        {
            transform.position = startPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            gameObject.SetActive(true);
        }

        public void SetEnergyPulse(float amount)
        {
            pulse = Mathf.Max(pulse, amount);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            startPosition = transform.position;
        }

        private void Update()
        {
            ReadInput();
            Animate();
        }

        private void FixedUpdate()
        {
            if (manager != null && manager.State != GameState.Playing)
            {
                body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 7f);
                return;
            }

            body.AddForce(input * acceleration, ForceMode2D.Force);
            body.linearVelocity = Vector2.ClampMagnitude(body.linearVelocity, maxSpeed);
            ClampToView();
        }

        private void ReadInput()
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (Input.GetMouseButton(0))
            {
                var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var direction = (Vector2)(target - transform.position);
                input = direction.sqrMagnitude > 0.08f ? direction.normalized : Vector2.zero;
            }

            input = Vector2.ClampMagnitude(input, 1f);
        }

        private void Animate()
        {
            idleBob += Time.deltaTime * 5f;
            pulse = Mathf.MoveTowards(pulse, 0f, Time.deltaTime * 5f);

            var speed01 = Mathf.InverseLerp(0f, maxSpeed, body.linearVelocity.magnitude);
            var squash = 1f + Mathf.Sin(idleBob) * 0.025f + pulse * 0.16f;
            var stretch = 1f - pulse * 0.08f;
            transform.localScale = new Vector3(squash, stretch, 1f);
            transform.rotation = Quaternion.Euler(0f, 0f, -body.linearVelocity.x * tiltAmount * 0.12f);

            if (speed01 > 0.2f)
                effects.PlayerTrail(transform.position - (Vector3)(body.linearVelocity.normalized * 0.28f), GamePalette.Cyan.WithAlpha(0.25f + speed01 * 0.3f));
        }

        private void ClampToView()
        {
            var camera = Camera.main;
            var halfHeight = camera.orthographicSize;
            var halfWidth = halfHeight * camera.aspect;
            var clamped = transform.position;
            clamped.x = Mathf.Clamp(clamped.x, -halfWidth + 0.45f, halfWidth - 0.45f);
            clamped.y = Mathf.Clamp(clamped.y, -halfHeight + 0.45f, halfHeight - 0.45f);
            transform.position = clamped;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out Collectible collectible))
            {
                collectible.Collect();
                manager.Collect(transform.position);
            }
            else if (other.TryGetComponent(out Hazard hazard))
            {
                hazard.Impact();
                manager.Hit(transform.position);
            }
        }
    }
}
