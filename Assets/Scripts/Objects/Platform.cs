using System.Collections;
using UnityEngine;
using Enemies;

namespace Level
{

    /// <summary>
    /// Plataforma que puede ser destruida temporalmente al ser golpeada (puño del boss Ira).
    /// Implementa IPunchDeactivatable para que BaseBoss pueda llamarla directamente.
    /// </summary>
    public class Platform : MonoBehaviour, IPunchDeactivatable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D platformCollider;
        [SerializeField] private AudioSource breakSound;
        [SerializeField] private AudioSource shakeSound;

        public bool isDeactivated = false;
        public bool isShaking = false;

        private Vector3 originalPosition;
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        /// <summary>
        /// desactiva la plataforma (por colisión con puño)
        /// </summary>
        public void DeactivateByPunch()
        {
            if (isDeactivated || isShaking) return;

            isShaking = true;
            StartCoroutine(ShakeThenBreakRoutine());
        }

        private IEnumerator ShakeThenBreakRoutine()
        {
            yield return StartShakeEffect();

            yield return new WaitForSeconds(0.8f);
            BreakPlatform();

            yield return new WaitForSeconds(2.5f);
            Reactivate();
        }

        /// <summary>
        /// efecto de temblor antes de romperse
        /// </summary>
        private IEnumerator StartShakeEffect()
        {
            originalPosition = transform.position;
            shakeSound?.Play();

            float duration = 0.8f;
            float intensity = 0.04f; // eran 4px, ajusta a tu escala
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float offsetX = Mathf.Sin(t * 60f) * intensity;
                float offsetY = Mathf.Sin(t * 45f) * (intensity / 2f);
                transform.position = originalPosition + new Vector3(offsetX, offsetY, 0f);

                if (spriteRenderer != null)
                {
                    var c = spriteRenderer.color;
                    c.a = Mathf.PingPong(t * 10f, 0.3f) + 0.7f;
                    spriteRenderer.color = c;
                }
                yield return null;
            }

            transform.position = originalPosition;
            if (spriteRenderer != null)
            {
                var c = spriteRenderer.color; c.a = 1f; spriteRenderer.color = c;
            }
        }

        /// <summary>
        /// rompe la plataforma
        /// </summary>
        private void BreakPlatform()
        {
            isDeactivated = true;
            isShaking = false;

            CreateBreakParticles();
            StartCoroutine(BreakRoutine());
            breakSound?.Play();
        }

        private IEnumerator BreakRoutine()
        {
            float duration = 0.3f;
            float t = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = new Vector3(startScale.x * 1.2f, startScale.y * 0.3f, startScale.z);
            float targetAngle = Random.Range(-10f, 10f);
            Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

            while (t < duration)
            {
                t += Time.deltaTime;
                float f = 1f - Mathf.Pow(1f - t / duration, 3f); // ease-in cúbico aprox.
                transform.localScale = Vector3.Lerp(startScale, targetScale, f);
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, targetAngle, f));
                if (spriteRenderer != null)
                {
                    var c = startColor; c.a = Mathf.Lerp(startColor.a, 0f, f); spriteRenderer.color = c;
                }
                yield return null;
            }

            gameObject.SetActive(false);
            if (platformCollider != null) platformCollider.enabled = false;
        }

        /// <summary>
        /// crea partículas de rotura (procedurales, sin assets)
        /// </summary>
        private void CreateBreakParticles()
        {
            Color color = spriteRenderer != null ? spriteRenderer.color : Color.white;

            for (int i = 0; i < 15; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float speed = Mathf.Lerp(0.5f, 2f, Random.value);
                var particle = BaseBossAttackState.CreateColorCircleObject("PlatformParticle", 0.08f, color, 1f);
                particle.transform.position = transform.position;
                StartCoroutine(ParticleRoutine(particle, angle, speed));
            }
        }

        private IEnumerator ParticleRoutine(GameObject particle, float angle, float speed)
        {
            var sr = particle.GetComponent<SpriteRenderer>();
            Vector3 start = particle.transform.position;
            Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * speed;
            float duration = 1f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                velocity += (Vector3)(Physics2D.gravity * 0.3f * Time.deltaTime); // "gravityY: 300" aprox.
                particle.transform.position += velocity * Time.deltaTime;

                float f = t / duration;
                particle.transform.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.zero, f);
                if (sr != null) { var c = sr.color; c.a = Mathf.Lerp(1f, 0f, f); sr.color = c; }
                yield return null;
            }
            Destroy(particle);
        }

        /// <summary>
        /// reactiva la plataforma con animación de reconstrucción
        /// </summary>
        private void Reactivate()
        {
            gameObject.SetActive(true);
            if (spriteRenderer != null) { var c = spriteRenderer.color; c.a = 0f; spriteRenderer.color = c; }
            transform.localScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 0.3f, originalScale.z);

            StartCoroutine(ReactivateRoutine());
        }

        private IEnumerator ReactivateRoutine()
        {
            float duration = 0.5f;
            float t = 0f;
            Vector3 startScale = transform.localScale;

            while (t < duration)
            {
                t += Time.deltaTime;
                float f = EaseOutBack(t / duration);
                transform.localScale = Vector3.Lerp(startScale, originalScale, f);
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 0f, f));
                if (spriteRenderer != null) { var c = spriteRenderer.color; c.a = Mathf.Clamp01(f); spriteRenderer.color = c; }
                yield return null;
            }

            transform.localScale = originalScale;
            isDeactivated = false;
            if (platformCollider != null) platformCollider.enabled = true;

            // parpadeo final de confirmación
            if (spriteRenderer != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    yield return FadeAlpha(0.8f, 0.1f);
                    yield return FadeAlpha(1f, 0.1f);
                }
            }
        }

        private IEnumerator FadeAlpha(float target, float duration)
        {
            float t = 0f;
            float start = spriteRenderer.color.a;
            while (t < duration)
            {
                t += Time.deltaTime;
                var c = spriteRenderer.color;
                c.a = Mathf.Lerp(start, target, t / duration);
                spriteRenderer.color = c;
                yield return null;
            }
        }

        private static float EaseOutBack(float t)
        {
            const float s = 1.70158f;
            float t2 = t - 1f;
            return t2 * t2 * ((s + 1f) * t2 + s) + 1f;
        }

        /// <summary>
        /// equivalente al action() original: usa el mismo efecto que con el puño
        /// </summary>
        public void Action()
        {
            if (!gameObject.activeSelf || isDeactivated) return;
            DeactivateByPunch();
        }
    }
}