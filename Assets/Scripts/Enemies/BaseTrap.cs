using UnityEngine;

namespace Enemies
{

/// <summary>
/// Trampa básica que toca y mata.
/// </summary>
public class BaseTrap : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();                                       // inicializar BaseEnemy

        // stats
        speed = 0f;                                          // trampa no se mueve
        inmune = true;                                        // no recibe daño
        collisionDamage = 100f;                               // daño extremo

        // render
        transform.localScale = new Vector3(15f, 1f, 1f);     // trampa muy larga
        if (SpriteRenderer != null) SpriteRenderer.sortingOrder = -1; // dibujar detrás

        // no añade estados: igual que en el original, la trampa no se mueve ni ataca
    }
}
}
