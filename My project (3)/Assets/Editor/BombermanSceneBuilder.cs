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
            mainCam.orthographicSize = 6f; // Daha büyük (21x11) harita için kamerayı uzaklaştır
            mainCam.transform.position = new Vector3(5f, 2.5f, -10f); // 21x11 Grid (Scale 0.5) merkezi (X: 5, Y: 2.5)
            mainCam.backgroundColor = new Color(0.15f, 0.4f, 0.15f);
        }

        // 4. Prefab'leri Üret
        GameObject explosionPrefab = CreateExplosionPrefab();
        GameObject bombPrefab = CreateBombPrefab(explosionPrefab);
        GameObject playerPrefab = CreatePlayerPrefab(bombPrefab);
        GameObject enemyPrefab = CreateEnemyPrefab();
        GameObject boxPrefab = CreateBoxPrefab();

        // 5. Tilemap ile Kırılamaz Duvarları Çiz (Grid -> Tilemap)
        GameObject gridObj = new GameObject("Grid");
        Grid grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0); 
        gridObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f); 

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
            wallTile.sprite = LoadSprite("Assets/Sprites/Wall.png");
            AssetDatabase.CreateAsset(wallTile, "Assets/Tiles/WallTile.asset");
        }

        // Dikdörtgen 21x11 Dış Çerçeve Çiz (Grid cell koordinatları: X 0-20, Y 0-10)
        for (int x = 0; x < 21; x++)
        {
            for (int y = 0; y < 11; y++)
            {
                bool isBorder = (x == 0 || x == 20 || y == 0 || y == 10);
                if (isBorder)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
            }
        }

        // 6. Kullanıcı İçin Örnek Objeleri Sahneye Bırak
        GameObject demoParent = new GameObject("Demo_Items (Buradan cogaltin)");
        
        GameObject pObj = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        pObj.transform.position = new Vector3(0.5f, 0.5f, 0f); // X=1, Y=1 (Grid x=1, y=1 -> *0.5)
        pObj.transform.SetParent(demoParent.transform);

        GameObject eObj = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
        eObj.transform.position = new Vector3(9.5f, 4.5f, 0f); // X=19, Y=9
        eObj.transform.SetParent(demoParent.transform);

        GameObject bObj = PrefabUtility.InstantiatePrefab(boxPrefab) as GameObject;
        bObj.transform.position = new Vector3(1.0f, 1.0f, 0f); // X=2, Y=2
        bObj.transform.SetParent(demoParent.transform);

        // 7. UI Kurulumu
        BuildUI();

        Debug.Log("Sahne (Dikdörtgen 21x11), Prefab'ler ve Tilemap başarıyla oluşturuldu!");
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
        // Kutu çarpışmasını algılayabilmesi için Explosion'a Kinematic Rigidbody ekliyoruz
        Rigidbody2D expRb = expObj.AddComponent<Rigidbody2D>();
        expRb.bodyType = RigidbodyType2D.Kinematic;
        
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
        bombObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f); 
        SpriteRenderer bSr = bombObj.AddComponent<SpriteRenderer>();
        bSr.sprite = LoadSprite("Assets/Sprites/Bomb.png");
        bSr.sortingOrder = 2;
        PolygonCollider2D bCol = bombObj.AddComponent<PolygonCollider2D>();
        
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
        player.transform.localScale = new Vector3(0.5f, 0.5f, 1f); 
        
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/Player.png");
        sr.sortingOrder = 10;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        player.AddComponent<PolygonCollider2D>();

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
        enemy.transform.localScale = new Vector3(0.5f, 0.5f, 1f); 

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/Enemy.png");
        sr.sortingOrder = 9;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        enemy.AddComponent<PolygonCollider2D>();

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
        box.transform.localScale = new Vector3(0.5f, 0.5f, 1f); 

        SpriteRenderer sr = box.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/BreakableBlock.png");
        sr.sortingOrder = 2;

        box.AddComponent<PolygonCollider2D>();
        
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
        panelImg.sprite = LoadSprite("Assets/Sprites/UI/Panel.png");
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
        heartImg.sprite = LoadSprite("Assets/Sprites/UI/Heart.png");
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
            // Orijinal dosya boyutunu oku
            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            
            // Resmi 1x1 birim dünya koordinatına sığdırmak için PPU'yu en büyük boyuta eşitle
            float targetPPU = Mathf.Max(tex.width, tex.height);
            // Minimum PPU değeri 100 olsun ki çok küçük UI görselleri bozulmasın
            if (targetPPU < 100) targetPPU = 100;

            if (importer.textureType != TextureImporterType.Sprite || importer.filterMode != FilterMode.Bilinear || Mathf.Abs(importer.spritePixelsPerUnit - targetPPU) > 0.1f)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = targetPPU; 
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object obj in allAssets)
            {
                if (obj is Sprite s) return s;
            }
        }
        return sprite;
    }
}
