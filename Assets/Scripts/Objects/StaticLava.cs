using UnityEngine;

namespace Level
{

/// <summary>
/// Obstáculo de lava estática. Usa un material con textura "tileable" y desplaza
/// su UV para simular el flujo (equivalente al TileSprite de Phaser).
/// NOTA: en el archivo original no había lógica de daño al jugador implementada
/// (solo el scroll visual y el cuerpo físico) — si tu juego mata al jugador al tocar
/// la lava, esa lógica vive en otro sitio; dímelo si quieres que la añada aquí.
/// </summary>
public class StaticLava : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer; // con un material que tilee la textura
    [SerializeField] private BoxCollider2D lavaCollider;

    public float flowSpeedX = 0.05f; // eran 0.5 en px/frame, ajusta a tu escala
    public float flowSpeedY = 0.025f;

    private void Update()
    {
        // desplazar la textura para simular el flujo de la lava
        if (spriteRenderer != null && spriteRenderer.material.HasProperty("_MainTex"))
        {
            Vector2 offset = spriteRenderer.material.mainTextureOffset;
            offset.y -= flowSpeedY * Time.deltaTime;
            spriteRenderer.material.mainTextureOffset = offset;
        }
    }
}
}
