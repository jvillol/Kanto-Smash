using UnityEngine;

namespace KantoSmash
{
    /// <summary>
    /// Inicializador automático del juego en Unity.
    /// Al pulsar Play en cualquier escena de Unity, configura la cámara ortográfica 2D,
    /// el SmashGameManager y el SmashUIManager sin necesidad de arrastrar prefabs manualmente.
    /// </summary>
    public static class SmashBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitializeGame()
        {
            if (SmashGameManager.Instance != null) return;

            // Configurar Cámara Principal 2D
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                cam = camObj.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 5.6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.09f, 0.17f);

            // Crear el Sistema Central de Kanto Smash
            GameObject systemObj = new GameObject("KantoSmash_System");
            Object.DontDestroyOnLoad(systemObj);
            systemObj.AddComponent<SmashGameManager>();
            systemObj.AddComponent<SmashUIManager>();
        }
    }
}

