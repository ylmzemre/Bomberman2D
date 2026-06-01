using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Bomberman2D.Player;
using Bomberman2D.Enemy;
using Bomberman2D.Core;
using Bomberman2D.Environment;
using Bomberman2D.Mechanics;
using Bomberman2D.UI;

public class BombermanSceneBuilder
{
    [MenuItem("Bomberman/1. Otomatik Sahneyi Kur")]
    public static void BuildScene()
    {
        Debug.Log("Sahne temizleniyor ve baştan kuruluyor...");

        // Önceki bozuk kalıntıları temizle
        DestroyIfExists("Player");
        DestroyIfExists("Enemy");
        DestroyIfExists("Canvas");
        DestroyIfExists("EventSystem");
        DestroyIfExists("GameManager");
        DestroyIfExists("Environment");

        // 1. Görselleri Pixel Art Sprite'a Çevir
        SetupSprite("Assets/Sprites/Player.png");
        SetupSprite("Assets/Sprites/Enemy.png");
        SetupSprite("Assets/Sprites/Bomb.png");
        SetupSprite("Assets/Sprites/Wall.png");
        SetupSprite("Assets/Sprites/BreakableBlock.png");
        SetupSprite("Assets/Sprites/UI/Button.png");
        SetupSprite("Assets/Sprites/UI/Panel.png");
        SetupSprite("Assets/Sprites/UI/Heart.png");

        // 2. GameManager Oluştur
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // 3. Kamera Ayarları
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 6f;
            mainCam.transform.position = new Vector3(5f, 5f, -10f);
            mainCam.backgroundColor = new Color(0.2f, 0.5f, 0.2f); // Koyu yeşil zemin
        }

        // 4. Oyuncu Oluştur
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(1, 1, 0);
        player.tag = "Player";
        
        SpriteRenderer pSr = player.AddComponent<SpriteRenderer>();
        pSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player.png");
        pSr.sortingOrder = 10;

        Rigidbody2D pRb = player.AddComponent<Rigidbody2D>();
        pRb.gravityScale = 0;
        pRb.freezeRotation = true;

        BoxCollider2D pCol = player.AddComponent<BoxCollider2D>();
        pCol.size = new Vector2(0.8f, 0.8f);

        player.AddComponent<PlayerController>();
        BombSpawner spawner = player.AddComponent<BombSpawner>();
        
        // Otomatik Bomb ve Explosion Prefab Üretimi
        GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Explosion.prefab");
        if (explosionPrefab == null)
        {
            GameObject expObj = new GameObject("ExplosionPrefab");
            BoxCollider2D expCol = expObj.AddComponent<BoxCollider2D>();
            expCol.isTrigger = true;
            expObj.AddComponent<Explosion>();
            if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");
            explosionPrefab = PrefabUtility.SaveAsPrefabAsset(expObj, "Assets/Prefabs/Explosion.prefab");
            Object.DestroyImmediate(expObj);
        }

        GameObject bombPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bomb.prefab");
        if (bombPrefab == null)
        {
            GameObject bombObj = new GameObject("BombPrefab");
            SpriteRenderer bSr = bombObj.AddComponent<SpriteRenderer>();
            bSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bomb.png");
            CircleCollider2D bCol = bombObj.AddComponent<CircleCollider2D>();
            Bomb bombComp = bombObj.AddComponent<Bomb>();
            bombComp.explosionPrefab = explosionPrefab;
            bombPrefab = PrefabUtility.SaveAsPrefabAsset(bombObj, "Assets/Prefabs/Bomb.prefab");
            Object.DestroyImmediate(bombObj);
        }

        spawner.bombPrefab = bombPrefab;

        // 5. Düşman Oluştur
        GameObject enemy = new GameObject("Enemy");
        enemy.transform.position = new Vector3(9, 9, 0);
        enemy.tag = "Enemy";

        SpriteRenderer eSr = enemy.AddComponent<SpriteRenderer>();
        eSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy.png");
        eSr.sortingOrder = 9;

        Rigidbody2D eRb = enemy.AddComponent<Rigidbody2D>();
        eRb.gravityScale = 0;
        eRb.freezeRotation = true;

        BoxCollider2D eCol = enemy.AddComponent<BoxCollider2D>();
        eCol.size = new Vector2(0.8f, 0.8f);

        enemy.AddComponent<EnemyAI>();

        // 6. Basit Harita (11x11 Grid)
        GameObject env = new GameObject("Environment");
        Sprite wallSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Wall.png");
        Sprite boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BreakableBlock.png");

        for (int x = 0; x < 11; x++)
        {
            for (int y = 0; y < 11; y++)
            {
                bool isBorder = (x == 0 || x == 10 || y == 0 || y == 10);
                bool isInnerWall = (x % 2 == 0 && y % 2 == 0);
                bool isPlayerSafeZone = (x <= 2 && y <= 2);

                if (isBorder || isInnerWall)
                {
                    CreateWall(x, y, env.transform, wallSprite, "Wall");
                }
                else if (!isPlayerSafeZone && Random.value > 0.3f)
                {
                    CreateBreakable(x, y, env.transform, boxSprite);
                }
            }
        }

        // 7. UI Canvas Oluştur
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        GameObject panelObj = new GameObject("TopPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel.png");
        panelImg.color = new Color(0, 0, 0, 0.5f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0.5f, 1);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0, 60);

        uiManager.scoreText = CreateText("ScoreText", panelObj.transform, new Vector2(20, -30), "Score: 0");
        uiManager.timeText = CreateText("TimeText", panelObj.transform, new Vector2(250, -30), "Time: 05:00");
        uiManager.bombCountText = CreateText("BombText", panelObj.transform, new Vector2(500, -30), "Bombs: 1/1");
        uiManager.enemyCountText = CreateText("EnemyText", panelObj.transform, new Vector2(750, -30), "Enemies: 0");

        GameObject livesObj = new GameObject("LivesContainer");
        livesObj.transform.SetParent(panelObj.transform, false);
        RectTransform livesRect = livesObj.AddComponent<RectTransform>();
        livesRect.anchorMin = new Vector2(1, 0.5f);
        livesRect.anchorMax = new Vector2(1, 0.5f);
        livesRect.pivot = new Vector2(1, 0.5f);
        livesRect.anchoredPosition = new Vector2(-20, 0);
        livesRect.sizeDelta = new Vector2(150, 40);

        GameObject heartPrefabObj = new GameObject("HeartPrefab");
        Image heartImg = heartPrefabObj.AddComponent<Image>();
        heartImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Heart.png");
        heartPrefabObj.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
        string heartPrefabPath = "Assets/Prefabs/HeartIcon.prefab";
        GameObject savedHeartPrefab = PrefabUtility.SaveAsPrefabAsset(heartPrefabObj, heartPrefabPath);
        Object.DestroyImmediate(heartPrefabObj);

        uiManager.livesContainer = livesObj.transform;
        uiManager.heartIconPrefab = savedHeartPrefab;

        // 8. Event System (InputSystemUIInputModule ile)
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        Debug.Log("Sahne tamamen sıfırlandı ve hatasız şekilde kuruldu!");
    }

    private static void DestroyIfExists(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 position, string defaultText)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 40);
        return tmp;
    }

    private static void CreateWall(int x, int y, Transform parent, Sprite sprite, string tagStr)
    {
        GameObject wall = new GameObject($"Wall_{x}_{y}");
        wall.transform.position = new Vector3(x, y, 0);
        wall.transform.parent = parent;
        wall.tag = tagStr;
        
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);
    }

    private static void CreateBreakable(int x, int y, Transform parent, Sprite sprite)
    {
        GameObject box = new GameObject($"Box_{x}_{y}");
        box.transform.position = new Vector3(x, y, 0);
        box.transform.parent = parent;
        box.tag = "Breakable";
        
        SpriteRenderer sr = box.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        BoxCollider2D col = box.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);

        box.AddComponent<BreakableBlock>();
    }

    private static void SetupSprite(string path)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite || importer.filterMode != FilterMode.Point)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 256; 
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}
