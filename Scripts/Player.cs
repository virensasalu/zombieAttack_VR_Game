using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int health = 100;
    public GameObject bulletPrefab;
    public float shootingCooldown = 1f;
    
    // Bullet spawn point - add this
    public Transform bulletSpawnPoint; // Reference to a child object at the cannon's head
    
    // Reload mechanism variables
    public int maxAmmo = 10;
    private int currentAmmo;
    public float reloadTime = 1f;
    private bool isReloading = false;
    
    // Perk system variables
    private float gameTimer = 0f;
    private bool ammoPerkActivated = false;
    private bool rapidFirePerkActivated = false;
    private int perkAmmoAmount = 20;
    private int rapidFireAmmoAmount = 25;
    private float ammoPerkActivationTime = 60f;
    private float rapidFirePerkActivationTime = 75f;
    private float rapidFireCooldown = 0.1f;
    
    // Optional UI elements for ammo display
    public TextMesh ammoText;
    
    private float shootingTimer;

    void Start()
    {
        // Initialize ammo
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
        
        // If no spawn point is assigned, create one
        if (bulletSpawnPoint == null)
        {
            // Use the current transform as fallback
            bulletSpawnPoint = transform;
            Debug.LogWarning("No bullet spawn point assigned. Using player position as default.");
        }
    }

    void Update()
    {
        // Update game timer
        gameTimer += Time.deltaTime;
        
        // Check for perk activations
        if (!ammoPerkActivated && gameTimer >= ammoPerkActivationTime)
        {
            ActivateAmmoPerk();
        }
        
        if (!rapidFirePerkActivated && gameTimer >= rapidFirePerkActivationTime)
        {
            ActivateRapidFirePerk();
        }
        
        // Decrease shooting timer
        if (shootingTimer > 0f)
        {
            shootingTimer -= Time.deltaTime;
        }
        
        // Handle shooting input based on current mode
        if (rapidFirePerkActivated)
        {
            // Rapid fire mode - continuous firing on mouse hold
            if (Input.GetMouseButton(0) && !isReloading)
            {
                TryShoot();
            }
            else if (Input.GetKeyDown("space") && !isReloading)
            {
                // Still allow space key for single shots
                TryShoot();
            }
        }
        else
        {
            // Normal mode - single shot on mouse click or space
            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown("space")) && !isReloading)
            {
                TryShoot();
            }
        }
    }
    
    void ActivateAmmoPerk()
    {
        ammoPerkActivated = true;
        maxAmmo = perkAmmoAmount;
        
        // Refill ammo to new maximum
        currentAmmo = maxAmmo;
        
        // Display perk activation message
        Debug.Log("PERK ACTIVATED: Ammo capacity increased to " + perkAmmoAmount + "!");
        
        // Show notification to player
        StartCoroutine(ShowPerkNotification("PERK: AMMO INCREASED TO " + perkAmmoAmount + "!"));
        
        // Update ammo display
        UpdateAmmoDisplay();
    }
    
    void ActivateRapidFirePerk()
    {
        rapidFirePerkActivated = true;
        maxAmmo = rapidFireAmmoAmount;
        
        // Refill ammo to new maximum
        currentAmmo = maxAmmo;
        
        // Display perk activation message
        Debug.Log("PERK ACTIVATED: Rapid Fire mode unlocked! Ammo increased to " + rapidFireAmmoAmount + "!");
        
        // Show notification to player
        StartCoroutine(ShowPerkNotification("PERK: RAPID FIRE MODE UNLOCKED! AMMO: " + rapidFireAmmoAmount));
        
        // Update ammo display
        UpdateAmmoDisplay();
    }
    
    IEnumerator ShowPerkNotification(string message)
    {
        // Store original ammo text
        string originalText = ammoText != null ? ammoText.text : "";
        Color originalColor = ammoText != null ? ammoText.color : Color.white;
        
        // Show perk notification
        if (ammoText != null)
        {
            ammoText.text = message;
            ammoText.color = Color.yellow; // Make it stand out
        }
        
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);
        
        // Restore original ammo display
        if (ammoText != null)
        {
            ammoText.color = originalColor;
            UpdateAmmoDisplay();
        }
    }
    
    void TryShoot()
    {
        // Check if we can shoot (cooldown is done and we have ammo)
        if (shootingTimer <= 0f && currentAmmo > 0)
        {
            // Reset shooting cooldown based on current mode
            shootingTimer = rapidFirePerkActivated ? rapidFireCooldown : shootingCooldown;
            
            // Create bullet at the spawn point position but KEEP ORIGINAL DIRECTION
            GameObject bulletObject = Instantiate(bulletPrefab);
            
            // Only change the position, not the rotation
            bulletObject.transform.position = bulletSpawnPoint.position;
            
            // Use the original direction logic
            Bullet bullet = bulletObject.GetComponent<Bullet>();
            bullet.direction = transform.forward; // Keep using the player's forward direction
            
            // Decrease ammo
            currentAmmo--;
            UpdateAmmoDisplay();
            
            // Check if we need to reload
            if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
        }
        else if (currentAmmo <= 0 && !isReloading)
        {
            // Auto-reload when trying to shoot with no ammo
            StartCoroutine(Reload());
        }
    }
    
    IEnumerator Reload()
    {
        isReloading = true;
        
        // Display reload message
        Debug.Log("Reloading...");
        if (ammoText != null)
        {
            string previousText = ammoText.text;
            ammoText.text = "Reloading...";
            
            // Wait for reload time
            yield return new WaitForSeconds(reloadTime);
            
            // Reload complete
            currentAmmo = maxAmmo;
            isReloading = false;
        }
        else
        {
            // If no text display, just wait
            yield return new WaitForSeconds(reloadTime);
            
            // Reload complete
            currentAmmo = maxAmmo;
            isReloading = false;
        }
        
        // Update UI
        UpdateAmmoDisplay();
        Debug.Log("Reload complete!");
    }
    
    void UpdateAmmoDisplay()
    {
        if (ammoText != null)
        {
            string modeText = rapidFirePerkActivated ? " [RAPID]" : "";
            ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo + modeText;
        }
    }
}