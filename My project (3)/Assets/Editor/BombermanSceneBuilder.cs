using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
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
        Debug.Log("Sahne baştan kuruluyor ve Prefab'ler oluşturuluyor...");

        // Gerekli klasörleri oluştur
        if (!System.IO.Directory.Exists("Assets/Prefabs")) System.IO.Directory.CreateDirectory("Assets/Prefabs");
        if (!System.IO.Directory.Exists("Assets/Tiles")) System.IO.Directory.CreateDirectory("Assets/Tiles");

        // 1. Etiketleri (Tags) Otomatik Ekle
        AddTag("Player");
        AddTag("Enemy");
        AddTag("Wall");
        AddTag("Breakable");
        AddTag("Bomb");
        AddTag("Explosion");

        // Temizlik
        DestroyIfExists("Player");
        DestroyIfExists("Enemy");
        DestroyIfExists("Canvas");
        DestroyIfExists("EventSystem");
        DestroyIfExists("GameManager");
        DestroyIfExists("Environment");
        DestroyIfExists("Grid");
        DestroyIfExists("Demo_Items");

        // 2. Görselleri Hazırla
        SetupSprite("Assets/Sprites/Player.png");
        SetupSprite("Assets/Sprites/Enemy.png");
        SetupSprite("Assets/Sprites/Bomb.png");
        SetupSprite("Assets/Sprites/Wall.png");
        SetupSprite("Assets/Sprites/BreakableBlock.png");
        SetupSprite("Assets/Sprites/UI/Button.png");
        SetupSprite("Assets/Sprites/UI/Panel.png");
        SetupSprite("Assets/Sprites/UI/Heart.png");

        // 3. Ortak Sistemler
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 4f; // Daha küçük harita için kamerayı yakınlaştır
            mainCam.transform.position = new Vector3(2.5f, 2.5f, -10f);
            mainCam.backgroundColor = new Color(0.15f, 0.4f, 0.15f);
        }

        // 4. Prefab'leri Üret (Eğer yoklarsa veya güncelleyeceksek)
        GameObject explosionPrefab = CreateExplosionPrefab();
        GameObject bombPrefab = CreateBombPrefab(explosionPrefab);
        GameObject playerPrefab = CreatePlayerPrefab(bombPrefab);
        GameObject enemyPrefab = CreateEnemyPrefab();
        GameObject boxPrefab = CreateBoxPrefab();

        // 5. Tilemap ile Kırılamaz Duvarları Çiz (Grid -> Tilemap)
        GameObject gridObj = new GameObject("Grid");
        Grid grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0); // Hücreler 1x1
        gridObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f); // Tüm Grid'i 0.5 ölçeğine küçült (Duvarlar 0.5 olacak)

        GameObject tilemapObj = new GameObject("WallTilemap");
        tilemapObj.transform.SetParent(gridObj.transform);
        tilemapObj.tag = "Wall";
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer tilemapRenderer = tilemapObj.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingOrder = 1;
        tilemapObj.AddComponent<TilemapCollider2D>();

        // Duvar Tile Asseti oluştur
        Tile wallTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/WallTile.asset");
        if (wallTile == null)
        {
            wallTile = ScriptableObject.CreateInstance<Tile>();
            wallTile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Wall.png");
            AssetDatabase.CreateAsset(wallTile, "Assets/Tiles/WallTile.asset");
        }

        // Dış Çerçeveyi Çiz (11x11 Grid'de, dış sınırlar)
        for (int x = 0; x < 11; x++)
        {
            for (int y = 0; y < 11; y++)
            {
                bool isBorder = (x == 0 || x == 10 || y == 0 || y == 10);
                if (isBorder)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
            }
        }

        // 6. Kullanıcı İçin Örnek Objeleri Sahneye Bırak
        GameObject demoParent = new GameObject("Demo_Items (Buradan cogaltin)");
        
        GameObject pObj = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        pObj.transform.position = new Vector3(0.5f, 0.5f, 0f); // Gerçek dünyada Grid'in (1,1) koordinatı 0.5 ölçekliyse 0.5, 0.5'tir.
        pObj.transform.SetParent(demoParent.transform);

        GameObject eObj = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
        eObj.transform.position = new Vector3(4.5f, 4.5f, 0f); // Grid (9,9)
        eObj.transform.SetParent(demoParent.transform);

        GameObject bObj = PrefabUtility.InstantiatePrefab(boxPrefab) as GameObject;
        bObj.transform.position = new Vector3(1.0f, 1.0f, 0f); // Grid (2,2)
        bObj.transform.SetParent(demoParent.transform);

        // 7. UI Kurulumu
        BuildUI();

        Debug.Log("Sahne, Prefab'ler ve Tilemap başarıyla oluşturuldu!");
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue.Equals(tag)) { found = true; break; }
        }
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }

    private static GameObject CreateExplosionPrefab()
    {
        string path = "Assets/Prefabs/Explosion.prefab";
        GameObject expObj = new GameObject("Explosion");
        expObj.tag = "Explosion";
        BoxCollider2D expCol = expObj.AddComponent<BoxCollider2D>();
        expCol.isTrigger = true;
        expObj.AddComponent<Explosion>();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(expObj, path);
        Object.DestroyImmediate(expObj);
        return prefab;
    }

    private static GameObject CreateBombPrefab(GameObject explosionPrefab)
    {
        string path = "Assets/Prefabs/Bomb.prefab";
        GameObject bombObj = new GameObject("Bomb");
        bombObj.tag = "Bomb";
        bombObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f); // Bomba da kutu/duvar boyutunda
        SpriteRenderer bSr = bombObj.AddComponent<SpriteRenderer>();
        bSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bomb.png");
        bSr.sortingOrder = 2;
        CircleCollider2D bCol = bombObj.AddComponent<CircleCollider2D>();
        Bomb bombComp = bombObj.AddComponent<Bomb>();
        bombComp.explosionPrefab = explosionPrefab;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bombObj, path);
        Object.DestroyImmediate(bombObj);
        return prefab;
    }

    private static GameObject CreatePlayerPrefab(GameObject bombPrefab)
    {
        string path = "Assets/Prefabs/Player.prefab";
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.localScale = new Vector3(0.3f, 0.3f, 1f); // İstenen ölçek
        
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Player.png");
        sr.sortingOrder = 10;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f); // Sprite'a tam oturması için

        player.AddComponent<PlayerController>();
        BombSpawner spawner = player.AddComponent<BombSpawner>();
        spawner.bombPrefab = bombPrefab;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, path);
        Object.DestroyImmediate(player);
        return prefab;
    }

    private static GameObject CreateEnemyPrefab()
    {
        string path = "Assets/Prefabs/Enemy.prefab";
        GameObject enemy = new GameObject("Enemy");
        enemy.tag = "Enemy";
        enemy.transform.localScale = new Vector3(0.4f, 0.4f, 1f); // İstenen ölçek

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemy.png");
        sr.sortingOrder = 9;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.9f, 0.9f);

        enemy.AddComponent<EnemyAI>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
        Object.DestroyImmediate(enemy);
        return prefab;
    }

    private static GameObject CreateBoxPrefab()
    {
        string path = "Assets/Prefabs/Box.prefab";
        GameObject box = new GameObject("Box");
        box.tag = "Breakable";
        box.transform.localScale = new Vector3(0.4f, 0.4f, 1f); // İstenen ölçek

        SpriteRenderer sr = box.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BreakableBlock.png");
        sr.sortingOrder = 2;

        BoxCollider2D col = box.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);
        box.AddComponent<BreakableBlock>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(box, path);
        Object.DestroyImmediate(box);
        return prefab;
    }

    private static void BuildUI()
    {
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

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
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

    private static void DestroyIfExists(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
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
