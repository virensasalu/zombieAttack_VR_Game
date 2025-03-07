using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Player player;
    public GameObject enemyPrefab;
    public TextMesh infoText;
    public TextMesh ammoText; // New reference for ammo text
    public GameObject splashScreen; // Reference to your Canvas > Image splash screen
    
    [Header("Audio")]
    public AudioClip backgroundMusic; // Reference to your CoFLoop1 audio file
    private AudioSource musicAudioSource;

    [Header("Lighting Settings")]
    public float ambientIntensity = 3.0f;
    public float reflectionIntensity = 1.0f;
    public Color ambientLight = Color.white;

    [Header("Enemy Spawn Settings")]
    public float enemySpawnDistance = 20f;
    public float initialEnemyInterval = 2.0f;
    public float enhancedEnemyInterval = 1.8f; // 60s: Reduced spawn rate
    public float extremeEnemyInterval = 2.0f;  // 75s: Significantly reduced spawn rate
    
    [Header("Text Background Settings")]
    public Color backgroundColor = new Color(0, 0, 0, 0.8f); // Black with 80% opacity
    
    [Header("Difficulty Progression")]
    public float enhancedModeTime = 60f; // Time when enhanced mode activates
    public float extremeModeTime = 75f;  // Time when extreme mode activates
    
    // Enemy enhancement settings
    public float enhancedSpeedMultiplier = 1.1f; // 10% faster at 60 seconds
    public float extremeSpeedMultiplier = 1.25f; // 25% faster at 75 seconds
    public int enhancedDamageBonus = 1; // +1 damage at 60 seconds
    public int extremeDamageBonus = 2;  // +2 damage at 75 seconds

    private float gameTimer = 0f;
    private float enemyTimer = 0f;
    private bool isGameOver = false;
    private bool gameStarted = false; // Track if the game has started
    
    // Difficulty tracking
    private bool enhancedModeActive = false;
    private bool extremeModeActive = false;
    
    // Enemy spawn burst settings
    private int normalBurstAmount = 1;
    private int enhancedBurstAmount = 1; // Reduced to 1 at a time but more frequent
    private int extremeBurstAmount = 1; // Reduced to 1 at a time

    // Materials for different difficulty levels
    private Material normalMaterial;
    private Material enhancedMaterial;
    private Material extremeMaterial;
    
    // Text background objects
    private GameObject textBackgroundObject;
    private GameObject ammoBackgroundObject; // New background for ammo text

    void Awake()
    {
        // Set up the audio source for background music
        SetupBackgroundMusic();
        
        // Apply lighting settings immediately
        ApplyLightingSettings();
        
        // Create materials for different difficulty levels
        CreateDifficultyMaterials();
        
        // Create text backgrounds for better readability
        CreateTextBackground();
        
        // Create ammo text background if ammoText exists
        if (ammoText != null)
        {
            CreateAmmoTextBackground();
        }
    }
    
    void CreateTextBackground()
    {
        if (infoText == null)
        {
            Debug.LogError("InfoText reference is missing!");
            return;
        }
        
        // Create a new GameObject for the background
        textBackgroundObject = new GameObject("TextBackground");
        
        // Create a mesh renderer and mesh filter
        MeshRenderer meshRenderer = textBackgroundObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = textBackgroundObject.AddComponent<MeshFilter>();
        
        // Create a simple quad mesh
        Mesh mesh = new Mesh();
        
        // Define vertices for a 200x150 rectangle
        // We'll center it, so half-width is 100, half-height is 75
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-20, -10, 0),  // Bottom left
            new Vector3(20, -10, 0),   // Bottom right
            new Vector3(-20, 10, 0),   // Top left
            new Vector3(20, 10, 0)     // Top right
        };
        
        // Define triangles
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        // Define UVs
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        // Assign mesh to mesh filter
        meshFilter.mesh = mesh;
        
        // Create a material for the background
        Material material = new Material(Shader.Find("UI/Default"));
        material.color = backgroundColor;
        
        // Assign material to mesh renderer
        meshRenderer.material = material;
        
        // Position the background behind the text
        textBackgroundObject.transform.SetParent(infoText.transform);
        textBackgroundObject.transform.localPosition = new Vector3(0, 0, 0.01f);
        textBackgroundObject.transform.localRotation = Quaternion.identity;
        
        // Ensure the background is rendered behind the text
        meshRenderer.sortingOrder = infoText.GetComponent<Renderer>().sortingOrder - 1;
        
        Debug.Log("Created text background with mesh renderer (40x20 units)");
    }
    
    void CreateAmmoTextBackground()
    {
        if (ammoText == null)
        {
            Debug.LogError("AmmoText reference is missing!");
            return;
        }
        
        Debug.Log("Creating ammo text background...");
        
        // Create a new GameObject for the background
        ammoBackgroundObject = new GameObject("AmmoTextBackground");
        
        // Create a mesh renderer and mesh filter
        MeshRenderer meshRenderer = ammoBackgroundObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = ammoBackgroundObject.AddComponent<MeshFilter>();
        
        // Create a simple quad mesh
        Mesh mesh = new Mesh();
        
        // Define vertices for a similar sized rectangle as the info text background
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-5, -11, 0),  // Bottom left
            new Vector3(70, -11, 0),   // Bottom right
            new Vector3(-5, 5, 0),   // Top left
            new Vector3(70, 5, 0)     // Top right
        };
        
        // Define triangles
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        // Define UVs
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        // Assign mesh to mesh filter
        meshFilter.mesh = mesh;
        
        // Create a material for the background - using the same shader as infoText background
        Material material = new Material(Shader.Find("UI/Default"));
        
        // Use the same backgroundColor as defined for infoText (black with 80% opacity)
        material.color = backgroundColor; // This should be Color(0, 0, 0, 0.8f)
        
        // Assign material to mesh renderer
        meshRenderer.material = material;
        
        // Position the background behind the text
        ammoBackgroundObject.transform.SetParent(ammoText.transform);
        ammoBackgroundObject.transform.localPosition = new Vector3(0, 0, 0.1f); // Slightly further back
        ammoBackgroundObject.transform.localRotation = Quaternion.identity;
        
        // Make sure the background is visible by adjusting the scale if needed
        ammoBackgroundObject.transform.localScale = Vector3.one;
        
        // Ensure the background is rendered behind the text but still visible
        if (ammoText.GetComponent<Renderer>() != null)
        {
            meshRenderer.sortingOrder = ammoText.GetComponent<Renderer>().sortingOrder - 1;
            Debug.Log("Set ammo background sorting order to: " + meshRenderer.sortingOrder);
        }
        else
        {
            Debug.LogWarning("AmmoText doesn't have a Renderer component!");
        }
        
        // Make sure the renderer is enabled
        meshRenderer.enabled = true;
        
        Debug.Log("Created ammo text background with mesh renderer (matching infoText background)");
    }

    void CreateDifficultyMaterials()
    {
        // Create materials for different difficulty levels
        normalMaterial = new Material(Shader.Find("Standard"));
        normalMaterial.EnableKeyword("_EMISSION");
        normalMaterial.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 0.2f, 1f) * 0.1f); // Green with 10% intensity
        
        enhancedMaterial = new Material(Shader.Find("Standard"));
        enhancedMaterial.EnableKeyword("_EMISSION");
        enhancedMaterial.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.2f, 1f) * 0.1f); // Orange with 10% intensity
        
        extremeMaterial = new Material(Shader.Find("Standard"));
        extremeMaterial.EnableKeyword("_EMISSION");
        extremeMaterial.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.2f, 1f) * 0.1f); // Red with 10% intensity
    }

    void ApplyLightingSettings()
    {
        // Apply lighting settings
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.reflectionIntensity = reflectionIntensity;
        RenderSettings.ambientLight = ambientLight;
        
        // Force lighting update
        DynamicGI.UpdateEnvironment();
    }

    void SetupBackgroundMusic()
    {
        // Create an AudioSource component for the background music
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        
        if (backgroundMusic != null)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.loop = true; // Make the music loop
            musicAudioSource.volume = 0.5f; // Set to 50% volume (adjust as needed)
            musicAudioSource.playOnAwake = true; // Start playing immediately
            musicAudioSource.priority = 0; // High priority
            
            // Start playing the background music
            musicAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music clip not assigned!");
        }
    }
    
    void Start(){
        // Apply lighting settings again to ensure they're set
        ApplyLightingSettings();
        
        // Show splash screen, hide game elements
        splashScreen.SetActive(true);
        
        // Ensure the splash screen fills the entire screen
        RectTransform splashRect = splashScreen.GetComponent<RectTransform>();
        if (splashRect != null)
        {
            splashRect.anchorMin = new Vector2(0, 0);
            splashRect.anchorMax = new Vector2(1, 1);
            splashRect.offsetMin = new Vector2(0, 0);
            splashRect.offsetMax = new Vector2(0, 0);
        }
        
        // Disable player controls initially
        if (player != null)
        {
            player.enabled = false;
        }
        
        // Hide cursor during splash screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Check for restart if game is over
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
            return;
        }
        
        // Check for Enter key if game hasn't started yet
        if (!gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartGame();
            }
            return; // Don't process the rest of Update until game starts
        }

        if (player.health > 0)
        {
            gameTimer += Time.deltaTime;
            
            // Update UI with current game state
            UpdateInfoText();
            
            // Check for difficulty progression
            CheckDifficultyProgression();

            // Handle enemy spawning
            enemyTimer -= Time.deltaTime;
            if (enemyTimer <= 0)
            {
                // Reset timer based on current difficulty
                if (extremeModeActive)
                    enemyTimer = extremeEnemyInterval;
                else if (enhancedModeActive)
                    enemyTimer = enhancedEnemyInterval;
                else
                    enemyTimer = initialEnemyInterval;
                
                // Spawn enemies based on current difficulty
                int spawnCount = normalBurstAmount;
                if (extremeModeActive)
                    spawnCount = extremeBurstAmount;
                else if (enhancedModeActive)
                    spawnCount = enhancedBurstAmount;
                
                // Spawn multiple enemies
                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnEnemy();
                }
            }
        }
        else
        {
            GameOver();
        }
    }
    
    void UpdateInfoText()
    {
        infoText.text = "Health: " + player.health;
        infoText.text += "\nTime: " + Mathf.Floor(gameTimer);
        
        // Show current difficulty mode
        if (extremeModeActive)
            infoText.text += "\nDifficulty: EXTREME!";
        else if (enhancedModeActive)
            infoText.text += "\nDifficulty: Enhanced";
    }
    
    void CheckDifficultyProgression()
    {
        // Check for enhanced mode activation (60 seconds)
        if (!enhancedModeActive && gameTimer >= enhancedModeTime)
        {
            ActivateEnhancedMode();
        }
        
        // Check for extreme mode activation (75 seconds)
        if (!extremeModeActive && gameTimer >= extremeModeTime)
        {
            ActivateExtremeMode();
        }
    }
    
    void ActivateEnhancedMode()
    {
        enhancedModeActive = true;
        Debug.Log("ENHANCED MODE ACTIVATED! Enemies will spawn more frequently and be stronger!");
        
        // Visual/audio feedback
        StartCoroutine(FlashScreen(new Color(1f, 0.6f, 0.2f, 0.3f))); // Orange flash
        
        // Spawn a burst of enhanced enemies immediately
        for (int i = 0; i < enhancedBurstAmount + 1; i++) // +1 for initial burst
        {
            SpawnEnemy(1); // 1 = enhanced difficulty
        }
    }
    
    void ActivateExtremeMode()
    {
        extremeModeActive = true;
        Debug.Log("EXTREME MODE ACTIVATED! Prepare for a real challenge!");
        
        // Visual/audio feedback
        StartCoroutine(FlashScreen(new Color(1f, 0.2f, 0.2f, 0.4f))); // Red flash
        
        // Spawn a burst of extreme enemies immediately
        for (int i = 0; i < extremeBurstAmount + 1; i++) // +1 for initial burst
        {
            SpawnEnemy(2); // 2 = extreme difficulty
        }
    }
    
    IEnumerator FlashScreen(Color flashColor)
    {
        // Create a temporary UI Image for the flash effect
        GameObject flashObj = new GameObject("ScreenFlash");
        flashObj.transform.SetParent(transform);
        Canvas canvas = flashObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Ensure it's on top
        
        Image flashImage = flashObj.AddComponent<Image>();
        flashImage.color = flashColor;
        
        RectTransform rect = flashObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        // Flash effect
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / duration);
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }
        
        Destroy(flashObj);
    }
    
    void StartGame()
    {
        // Hide splash screen
        splashScreen.SetActive(false);
        
        // Enable player controls
        if (player != null)
        {
            player.enabled = true;
        }
        
        // Start the game
        gameStarted = true;
        
        // Reset timers
        gameTimer = 0f;
        enemyTimer = 0f;
        
        // Spawn first enemy
        SpawnEnemy();
    }

    void SpawnEnemy(int difficultyLevel = 0)
    {
        GameObject enemyObject = Instantiate(enemyPrefab);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        // Adjust spawn position at player's height
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);
        enemy.transform.position = new Vector3(
            player.transform.position.x + Mathf.Cos(randomAngle) * enemySpawnDistance,
            player.transform.position.y, // Ensure it spawns at the player's height
            player.transform.position.z + Mathf.Sin(randomAngle) * enemySpawnDistance
        );

        // Assign player and movement
        enemy.player = player;
        enemy.direction = (player.transform.position - enemy.transform.position).normalized;
        enemy.transform.LookAt(new Vector3(player.transform.position.x, enemy.transform.position.y, player.transform.position.z));

        // Apply difficulty-based enhancements
        if (difficultyLevel == 0 && !enhancedModeActive && !extremeModeActive)
        {
            // Normal enemy - use default settings
            ApplyEnemyGlow(enemyObject, 0); // 0 = normal
        }
        else if (difficultyLevel == 1 || (difficultyLevel == 0 && enhancedModeActive && !extremeModeActive))
        {
            // Enhanced enemy (60s+)
            enemy.speed *= enhancedSpeedMultiplier;
            enemy.damage += enhancedDamageBonus;
            ApplyEnemyGlow(enemyObject, 1); // 1 = enhanced
        }
        else if (difficultyLevel == 2 || (difficultyLevel <= 1 && extremeModeActive))
        {
            // Extreme enemy (75s+)
            enemy.speed *= extremeSpeedMultiplier;
            enemy.damage += extremeDamageBonus;
            ApplyEnemyGlow(enemyObject, 2); // 2 = extreme
        }
    }
    
    void ApplyEnemyGlow(GameObject enemyObject, int difficultyLevel)
    {
        // Get all renderers in the enemy (including children)
        Renderer[] renderers = enemyObject.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found on enemy!");
            return;
        }
        
        // Select the appropriate material based on difficulty
        Material glowMaterial = null;
        Color lightColor = Color.white;
        float lightIntensity = 0.3f;
        
        switch (difficultyLevel)
        {
            case 0: // Normal
                glowMaterial = normalMaterial;
                lightColor = new Color(0.2f, 0.8f, 0.2f); // Green
                lightIntensity = 0.3f;
                break;
            case 1: // Enhanced
                glowMaterial = enhancedMaterial;
                lightColor = new Color(1f, 0.6f, 0.2f); // Orange
                lightIntensity = 0.4f;
                break;
            case 2: // Extreme
                glowMaterial = extremeMaterial;
                lightColor = new Color(1f, 0.2f, 0.2f); // Red
                lightIntensity = 0.5f;
                break;
        }
        
        // Apply the glow effect using a simpler approach
        foreach (Renderer renderer in renderers)
        {
            // Store original materials to preserve them
            Material[] originalMaterials = renderer.materials;
            Material[] newMaterials = new Material[originalMaterials.Length];
            
            // Copy properties from original materials to our glow materials
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                // Create a new instance of the material to avoid modifying shared materials
                newMaterials[i] = new Material(originalMaterials[i]);
                
                // Enable emission
                newMaterials[i].EnableKeyword("_EMISSION");
                
                // Set emission color based on difficulty
                Color emissionColor = Color.black;
                switch (difficultyLevel)
                {
                    case 0: // Normal
                        emissionColor = new Color(0.2f, 0.8f, 0.2f) * 0.1f; // Green with 10% intensity
                        break;
                    case 1: // Enhanced
                        emissionColor = new Color(1f, 0.6f, 0.2f) * 0.1f; // Orange with 10% intensity
                        break;
                    case 2: // Extreme
                        emissionColor = new Color(1f, 0.2f, 0.2f) * 0.1f; // Red with 10% intensity
                        break;
                }
                
                newMaterials[i].SetColor("_EmissionColor", emissionColor);
            }
            
            // Apply the new materials
            renderer.materials = newMaterials;
        }
        
        // Add a subtle point light for additional glow effect
        GameObject lightObj = new GameObject("GlowLight");
        lightObj.transform.SetParent(enemyObject.transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = lightColor;
        pointLight.intensity = lightIntensity;
        pointLight.range = 2.0f;
        pointLight.shadows = LightShadows.None;
    }

    void GameOver()
    {
        isGameOver = true;
        infoText.text = "Game Over!\nYou survived for " + Mathf.Floor(gameTimer) + " seconds!";
        
        // Add difficulty-specific messages
        if (extremeModeActive)
            infoText.text += "\nYou reached EXTREME difficulty!";
        else if (enhancedModeActive)
            infoText.text += "\nYou reached Enhanced difficulty!";
        else if (gameTimer >= enhancedModeTime * 0.8f)
            infoText.text += "\nSo close to Enhanced difficulty!";
            
        infoText.text += "\nPress 'R' to restart.";

        // Fade out music
        StartCoroutine(FadeOutMusic(2.0f));

        Time.timeScale = 0f; // Pause the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1f;
        
        // Store current lighting settings in PlayerPrefs to preserve them across scene loads
        PlayerPrefs.SetFloat("AmbientIntensity", ambientIntensity);
        PlayerPrefs.SetFloat("ReflectionIntensity", reflectionIntensity);
        PlayerPrefs.SetFloat("AmbientLightR", ambientLight.r);
        PlayerPrefs.SetFloat("AmbientLightG", ambientLight.g);
        PlayerPrefs.SetFloat("AmbientLightB", ambientLight.b);
        PlayerPrefs.Save();
        
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    void OnEnable()
    {
        // Check if we have stored lighting settings
        if (PlayerPrefs.HasKey("AmbientIntensity"))
        {
            // Retrieve lighting settings from PlayerPrefs
            ambientIntensity = PlayerPrefs.GetFloat("AmbientIntensity");
            reflectionIntensity = PlayerPrefs.GetFloat("ReflectionIntensity");
            float r = PlayerPrefs.GetFloat("AmbientLightR");
            float g = PlayerPrefs.GetFloat("AmbientLightG");
            float b = PlayerPrefs.GetFloat("AmbientLightB");
            ambientLight = new Color(r, g, b);
            
            // Apply lighting settings
            ApplyLightingSettings();
        }
    }

    IEnumerator FadeOutMusic(float fadeTime)
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            float startVolume = musicAudioSource.volume;
            
            for (float t = 0; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                musicAudioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
                yield return null;
            }
            
            musicAudioSource.Stop();
            musicAudioSource.volume = startVolume;
        }
    }
}