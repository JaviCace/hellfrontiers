using UnityEngine;

namespace Starters { 
public class RandomMover : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 2f;
    public float boundsX = 8f;
    public float boundsY = 4f;

    Vector2 _direction;

    void Start()
    {
        // Elige una dirección aleatoria al arrancar
        _direction = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);

        // Si se sale de los límites, rebota cambiando de dirección
        Vector3 pos = transform.position;
        if (pos.x > boundsX || pos.x < -boundsX) _direction.x *= -1;
        if (pos.y > boundsY || pos.y < -boundsY) _direction.y *= -1;
    }
}
}