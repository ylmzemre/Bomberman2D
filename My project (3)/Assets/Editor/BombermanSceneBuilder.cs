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
        
        // AudioManager Kurulumu (Kendi objesi olmalı ki DontDestroyOnLoad GameManager'ı da ölümsüz yapmasın)
        GameObject amObj = new GameObject("AudioManager");
        AudioManager am = amObj.AddComponent<AudioManager>();
        am.sfxSource = amObj.AddComponent<AudioSource>();
        am.musicSource = amObj.AddComponent<AudioSource>();
        
        AssetDatabase.Refresh(); // Python'un ürettiği ses dosyalarının import olmasını sağla
        am.explosionClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/explosion.wav");
        am.dropBombClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/drop_bomb.wav");
        am.clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/click.wav");
        am.deathClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/death.wav");

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.orthographicSize = 6.5f; // Scale 1 olduğu için harita yüksekliği 11, yarısı 5.5 (Biraz boşlukla 6.5)
            mainCam.transform.position = new Vector3(10f, 5.5f, -10f); // 21x11 Grid merkezi
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
        gridObj.transform.localScale = Vector3.one; 

        GameObject tilemapObj = new GameObject("WallTilemap");
        tilemapObj.transform.SetParent(gridObj.transform);
        tilemapObj.tag = "Wall";
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer tilemapRenderer = tilemapObj.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingOrder = 1;
        tilemapObj.AddComponent<TilemapCollider2D>();

        Tile wallTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/WallTile.asset");
        if (wallTile == null)
        {
            wallTile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(wallTile, "Assets/Tiles/WallTile.asset");
        }
        wallTile.sprite = LoadSprite("Assets/Sprites/Wall.png");
        EditorUtility.SetDirty(wallTile);

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

        GameObject demoParent = new GameObject("Demo_Items (Buradan cogaltin)");
        
        GameObject pObj = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        pObj.transform.position = new Vector3(1f, 1f, 0f); 
        pObj.transform.SetParent(demoParent.transform);

        GameObject eObj = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
        eObj.transform.position = new Vector3(19f, 9f, 0f); 
        eObj.transform.SetParent(demoParent.transform);

        GameObject bObj = PrefabUtility.InstantiatePrefab(boxPrefab) as GameObject;
        bObj.transform.position = new Vector3(2f, 2f, 0f); 
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
        bombObj.transform.localScale = Vector3.one; 
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
        player.transform.localScale = Vector3.one; 
        
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        
        Sprite[] playerSprites = LoadAllSprites("Assets/Sprites/PlayerSheet.png");
        if (playerSprites.Length > 0)
        {
            sr.sprite = playerSprites[0];
            
            Mechanics.SpriteAnimator anim = player.AddComponent<Mechanics.SpriteAnimator>();
            int cols = playerSprites.Length / 4;
            anim.downSprites = GetSpriteRange(playerSprites, 0, cols);
            anim.upSprites = GetSpriteRange(playerSprites, cols, cols);
            anim.leftSprites = GetSpriteRange(playerSprites, cols * 2, cols);
            anim.rightSprites = GetSpriteRange(playerSprites, cols * 3, cols);
        }
        else
        {
            sr.sprite = LoadSprite("Assets/Sprites/Player.png");
        }
        
        sr.sortingOrder = 3;

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
        enemy.transform.localScale = Vector3.one; 

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();

        Sprite[] enemySprites = LoadAllSprites("Assets/Sprites/EnemySheet.png");
        if (enemySprites.Length >= 12)
        {
            sr.sprite = enemySprites[0];
            
            Mechanics.SpriteAnimator anim = enemy.AddComponent<Mechanics.SpriteAnimator>();
            anim.downSprites = new Sprite[] { enemySprites[0], enemySprites[1], enemySprites[2], enemySprites[8] };
            anim.rightSprites = new Sprite[] { enemySprites[3], enemySprites[4], enemySprites[9], enemySprites[10], enemySprites[11] };
            anim.upSprites = new Sprite[] { enemySprites[6], enemySprites[7] };
            anim.useFlipForLeft = true;
        }
        else
        {
            sr.sprite = LoadSprite("Assets/Sprites/Enemy.png");
        }

        sr.sortingOrder = 3;

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
        box.transform.localScale = Vector3.one; 

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

        // Ana Menü Paneli
        GameObject mainMenuObj = new GameObject("MainMenuPanel");
        mainMenuObj.transform.SetParent(canvasObj.transform, false);
        Image mainImg = mainMenuObj.AddComponent<Image>();
        mainImg.color = new Color(0.1f, 0.1f, 0.2f, 1f); // Koyu arka plan
        RectTransform mainRect = mainMenuObj.GetComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.offsetMin = Vector2.zero;
        mainRect.offsetMax = Vector2.zero;

        // Oyunun başında TopPanel kapalı, MainMenu açık olacak
        panelObj.SetActive(false); 

        // MainMenu scriptini ekle
        MainMenu menuScript = mainMenuObj.AddComponent<MainMenu>();
        menuScript.topPanel = panelObj;

        // Play Butonu
        GameObject playBtnObj = new GameObject("PlayButton");
        playBtnObj.transform.SetParent(mainMenuObj.transform, false);
        Image btnImg = playBtnObj.AddComponent<Image>();
        btnImg.sprite = LoadSprite("Assets/Sprites/UI/Button.png");
        Button playBtn = playBtnObj.AddComponent<Button>();
        RectTransform btnRect = playBtnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(300, 100);
        btnRect.anchoredPosition = new Vector2(0, 0);
        
        // Butonu scripte bağla
        menuScript.playButton = playBtn;
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
