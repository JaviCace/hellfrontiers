using System.Collections;
using UnityEngine;

namespace Enemies
{

/// <summary>
/// Efectos visuales procedurales (rectángulos y círculos de color) para los avisos de los jefes,
/// sin necesitar ningún sprite.
/// </summary>
public static class BossFx
{
    private static Material NewMaterial(Color color)
    {
        var m = new Material(Shader.Find("Sprites/Default"));
        m.color = color;
        return m;
    }

    public static GameObject CreateQuad(Vector3 position, Vector2 size, Color color, float angleZ = 0f, int sortingOrder = 5)
    {
        var go = new GameObject("BossFxQuad");
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angleZ);

        float hw = size.x / 2f, hh = size.y / 2f;
        var mesh = new Mesh();
        mesh.vertices = new[]
        {
            new Vector3(-hw, -hh, 0f), new Vector3(-hw, hh, 0f),
            new Vector3(hw, hh, 0f), new Vector3(hw, -hh, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();

        go.AddComponent<MeshFilter>().mesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = NewMaterial(color);
        mr.sortingOrder = sortingOrder;
        return go;
    }

    public static GameObject CreateCircle(Vector3 position, float radius, Color color, int sortingOrder = 5)
    {
        var go = new GameObject("BossFxCircle");
        go.transform.position = position;

        const int segments = 32;
        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            vertices[i + 1] = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }
        var mesh = new Mesh { vertices = vertices, triangles = triangles };
        mesh.RecalculateBounds();

        go.AddComponent<MeshFilter>().mesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = NewMaterial(color);
        mr.sortingOrder = sortingOrder;
        return go;
    }

    public static void SetAlpha(GameObject go, float alpha)
    {
        if (go == null) return;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr == null) return;
        var c = mr.material.color;
        c.a = alpha;
        mr.material.color = c;
    }

    /// <summary>
    /// agranda el objeto mientras se desvanece y lo destruye al terminar
    /// </summary>
    public static IEnumerator ExpandAndFade(GameObject go, float toScale, float duration, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (go == null) yield break;

        var mr = go.GetComponent<MeshRenderer>();
        Color start = mr.material.color;
        float t = 0f;
        while (t < duration)
        {
            if (go == null) yield break;
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            go.transform.localScale = Vector3.one * Mathf.Lerp(1f, toScale, p);
            var c = start;
            c.a = Mathf.Lerp(start.a, 0f, p);
            mr.material.color = c;
            yield return null;
        }
        if (go != null) Object.Destroy(go);
    }

    /// <summary>
    /// centro y medias dimensiones de lo que ve la cámara (equivalente a cameras.main.worldView)
    /// </summary>
    public static void GetCameraView(Vector3 fallbackCenter, out Vector2 center, out float halfWidth, out float halfHeight)
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            center = cam.transform.position;
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * cam.aspect;
        }
        else
        {
            center = fallbackCenter;
            halfHeight = 6f;
            halfWidth = 10f;
        }
    }
}
}
