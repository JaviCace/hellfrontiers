using System.Collections;
using UnityEngine;
using Enemies;

namespace Level
{

/// <summary>
/// Clase base para objetos de tipo puerta.
/// Implementa IOpenable para que BaseBoss y los triggers puedan abrirla/cerrarla
/// sin necesitar conocer el tipo concreto.
/// </summary>
public class Door : MonoBehaviour, IOpenable
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Collider2D doorCollider;

    public bool abrir = false;                 // estado de apertura

    protected virtual void Awake()
    {
        if (rb != null) rb.gravityScale = 0f;  // sin gravedad
    }

    /// <summary>
    /// alterna el estado de apertura
    /// </summary>
    public virtual void ChangeOpen()
    {
        abrir = !abrir;
    }

    /// <summary>
    /// abre la puerta (pensado para ser sobreescrito por clases hijas)
    /// </summary>
    public virtual void OpenDoor()
    {
        Debug.Log("Puerta abierta");
    }

    /// <summary>
    /// cierra la puerta (pensado para ser sobreescrito por clases hijas)
    /// </summary>
    public virtual void CloseDoor()
    {
        Debug.Log("Puerta cerrada");
    }

    /// <summary>
    /// muestra un mensaje temporal sobre la puerta (TextMesh, sin necesitar assets)
    /// </summary>
    public void ShowMessage(string text)
    {
        var go = new GameObject("DoorMessage");
        go.transform.position = transform.position + new Vector3(0f, 1f, 0f);
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 48;
        tm.characterSize = 0.2f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.red;

        StartCoroutine(FadeAndDestroy(go, tm, 2f, 1f));
    }

    private IEnumerator FadeAndDestroy(GameObject go, TextMesh tm, float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        float t = 0f;
        Color start = tm.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            var c = start;
            c.a = Mathf.Lerp(start.a, 0f, t / fadeDuration);
            tm.color = c;
            yield return null;
        }
        Destroy(go);
    }
}
}
