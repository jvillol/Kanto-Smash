using UnityEngine;

namespace KantoSmash
{
    /// <summary>
    /// Construye el escenario de Kanto Smash:
    /// - 1 Plataforma grande principal en la base
    /// - 3 Plataformas pequeñas flotantes a las que se puede saltar y atravesar hacia abajo pulsando S dos veces
    /// </summary>
    public static class SmashStageBuilder
    {
        public static GameObject BuildBattleStage()
        {
            GameObject stageRoot = new GameObject("KantoSmash_Stage");

            // 1. Plataforma Grande Principal
            CreatePlatform(
                parent: stageRoot.transform,
                name: "MainLargePlatform",
                position: new Vector2(0f, -2.5f),
                size: new Vector2(12.5f, 0.75f),
                color: new Color(0.23f, 0.30f, 0.45f),
                isPassThrough: false
            );

            // 2. Tres Plataformas Pequeñas Flotantes (Izquierda, Derecha y Central Superior)
            CreatePlatform(
                parent: stageRoot.transform,
                name: "SmallPlatform_Left",
                position: new Vector2(-3.4f, -0.3f),
                size: new Vector2(3.2f, 0.28f),
                color: new Color(0.49f, 0.66f, 1.0f),
                isPassThrough: true
            );

            CreatePlatform(
                parent: stageRoot.transform,
                name: "SmallPlatform_Right",
                position: new Vector2(3.4f, -0.3f),
                size: new Vector2(3.2f, 0.28f),
                color: new Color(0.49f, 0.66f, 1.0f),
                isPassThrough: true
            );

            CreatePlatform(
                parent: stageRoot.transform,
                name: "SmallPlatform_TopCenter",
                position: new Vector2(0f, 1.85f),
                size: new Vector2(3.5f, 0.28f),
                color: new Color(0.49f, 0.66f, 1.0f),
                isPassThrough: true
            );

            return stageRoot;
        }

        private static void CreatePlatform(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool isPassThrough)
        {
            GameObject plat = new GameObject(name);
            plat.transform.SetParent(parent);
            plat.transform.position = position;

            SpriteRenderer sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSolidRectSprite(color);
            plat.transform.localScale = new Vector3(size.x, size.y, 1f);

            BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            if (isPassThrough)
            {
                plat.tag = "PassThroughPlatform";
                col.usedByEffector = true;
                PlatformEffector2D effector = plat.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.surfaceArc = 170f;
            }
        }

        private static Sprite CreateSolidRectSprite(Color color)
        {
            Texture2D tex = new Texture2D(16, 16);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    bool isBorder = (y == 15);
                    tex.SetPixel(x, y, isBorder ? Color.white : color);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}

