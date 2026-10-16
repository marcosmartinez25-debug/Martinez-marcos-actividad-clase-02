using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Actividad de Clase 02 - Practica de Materiales
/// Genera las capturas de pantalla de las dos variantes del edificio.
/// La camara se coloca sola calculando el tamano real del edificio.
/// Menu: "Actividad 02"
/// </summary>
public static class CapturasActividad02
{
    const int ANCHO = 1920;
    const int ALTO = 1080;
    const float FOV = 45f;

    [MenuItem("Actividad 02/Capturar las dos variantes")]
    public static void CapturarVariantes()
    {
        bool a = Capturar("Edificio_Cartoon", "Variante_Cartoon.png", -25f, 16f);
        bool b = Capturar("Edificio_Realista", "Variante_Realista.png", -25f, 16f);
        Terminar(a && b);
    }

    [MenuItem("Actividad 02/Capturar vista general (las dos juntas)")]
    public static void CapturarGeneral()
    {
        GameObject c = GameObject.Find("Edificio_Cartoon");
        GameObject r = GameObject.Find("Edificio_Realista");
        if (c == null || r == null) { Debug.LogError("No encuentro los edificios en la escena."); return; }
        Bounds b;
        if (!CalcularBounds(c, out b)) return;
        Bounds b2;
        if (CalcularBounds(r, out b2)) b.Encapsulate(b2);
        Terminar(Render(b, "Comparativa_Cartoon_vs_Realista.png", 0f, 14f));
    }

    static void Terminar(bool ok)
    {
        if (!ok) return;
        AssetDatabase.Refresh();
        string carpeta = Path.GetFullPath("Capturas");
        Debug.Log("Capturas guardadas en: " + carpeta);
        EditorUtility.RevealInFinder(carpeta);
    }

    static bool Capturar(string nombre, string fichero, float giro, float altura)
    {
        GameObject go = GameObject.Find(nombre);
        if (go == null) { Debug.LogError("No encuentro el objeto " + nombre); return false; }
        Bounds b;
        if (!CalcularBounds(go, out b)) return false;
        return Render(b, fichero, giro, altura);
    }

    static bool CalcularBounds(GameObject go, out Bounds b)
    {
        b = new Bounds(go.transform.position, Vector3.zero);
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) { Debug.LogError("El objeto " + go.name + " no tiene nada que dibujar."); return false; }
        b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return true;
    }

    static bool Render(Bounds b, string fichero, float giro, float inclinacion)
    {
        // Camara temporal colocada en funcion del tamano del edificio
        GameObject tmp = new GameObject("__CamaraCapturas");
        tmp.hideFlags = HideFlags.HideAndDontSave;
        Camera cam = tmp.AddComponent<Camera>();
        cam.fieldOfView = FOV;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 2000f;
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = new Color(0.19f, 0.30f, 0.47f);

        float radio = b.extents.magnitude;
        float distancia = radio / Mathf.Sin(Mathf.Deg2Rad * FOV * 0.5f) * 1.08f;
        Quaternion rot = Quaternion.Euler(inclinacion, giro, 0f);
        cam.transform.rotation = rot;
        cam.transform.position = b.center - rot * Vector3.forward * distancia;

        RenderTexture rt = new RenderTexture(ANCHO, ALTO, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 8;
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture anterior = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D img = new Texture2D(ANCHO, ALTO, TextureFormat.RGB24, false);
        img.ReadPixels(new Rect(0, 0, ANCHO, ALTO), 0, 0);
        img.Apply();
        RenderTexture.active = anterior;

        cam.targetTexture = null;
        Directory.CreateDirectory("Capturas");
        string ruta = Path.Combine("Capturas", fichero);
        File.WriteAllBytes(ruta, img.EncodeToPNG());

        Object.DestroyImmediate(img);
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tmp);
        Debug.Log("Guardada " + ruta);
        return true;
    }
}
