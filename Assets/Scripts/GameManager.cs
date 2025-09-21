using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // For reloading the scene

// BEGIN 2d_gamemanager
// Manages the game state.
public class GameManager : Singleton<GameManager> {

    // The location where the gnome should appear.
    public GameObject startingPoint;

    // The rope object, which lowers and raises the gnome.
    public Rope rope;

    // The fade-out script (triggered when the game resets)
    public Fade fade;

    // The follow script, which will follow the gnome
    public CameraFollow cameraFollow;

    // The 'current' gnome (as opposed to all those dead ones)
    Gnome currentGnome;

    // The prefab to instantiate when we need a new gnome
    public GameObject gnomePrefab;

    // 🔴 NEW: The pause menu (Resume/Restart/Quit)
    public RectTransform pauseMenu;

    // The UI component that contains the 'up', 'down' and 'menu' buttons
    public RectTransform gameplayMenu;

    // The UI component that contains the 'you win!' screen
    public RectTransform gameOverMenu;

    // The UI component that contains the "Game Over / Restart" screen
    public RectTransform deathMenu;

    // If true, ignore all damage (but still show damage effects)
    public bool gnomeInvincible { get; set; }

    // How long to wait after dying before creating a new gnome
    public float delayAfterDeath = 1.0f;

    // The sound to play when the gnome dies
    public AudioClip gnomeDiedSound;

    // The sound to play when the game is won
    public AudioClip gameOverSound;

    void Start() {
        // When the game starts, call Reset to set up the gnome.
        Reset();
    }

    // Reset the entire game.
    public void Reset() {
        // Turn off the menus, turn on the gameplay UI
        if (gameOverMenu)
            gameOverMenu.gameObject.SetActive(false);

        if (pauseMenu)
            pauseMenu.gameObject.SetActive(false);

        if (gameplayMenu)
            gameplayMenu.gameObject.SetActive(true);

        if (deathMenu) 
            deathMenu.gameObject.SetActive(false); // hide Death Menu at reset

        // Find all Resettable components and tell them to reset
        var resetObjects = FindObjectsOfType<Resettable>();

        foreach (Resettable r in resetObjects) {
            r.Reset();
        }

        // Make a new gnome
        CreateNewGnome();

        // Un-pause the game
        Time.timeScale = 1.0f;
    }

    // Create a new gnome.
    void CreateNewGnome() {
        RemoveGnome();

        GameObject newGnome = (GameObject)Instantiate(
            gnomePrefab, 
            startingPoint.transform.position, 
            Quaternion.identity
        );                                                     
        currentGnome = newGnome.GetComponent<Gnome>();

        rope.gameObject.SetActive(true);
        rope.connectedObject = currentGnome.ropeBody;
        rope.ResetLength();

        cameraFollow.target = currentGnome.cameraFollowTarget;
    }

    // Remove the gnome from the scene
    void RemoveGnome() {
        if (gnomeInvincible)
            return;

        rope.gameObject.SetActive(false);
        cameraFollow.target = null;

        if (currentGnome != null) {
            currentGnome.holdingTreasure = false;
            currentGnome.gameObject.tag = "Untagged";
            
            foreach (Transform child in currentGnome.transform) {
                child.gameObject.tag = "Untagged";
            }

            currentGnome = null;
        }
    }

    // Kills the gnome.
    void KillGnome(Gnome.DamageType damageType) {
        var audio = GetComponent<AudioSource>();
        if (audio) {
            audio.PlayOneShot(this.gnomeDiedSound);
        }

        currentGnome.ShowDamageEffect(damageType);

        if (!gnomeInvincible) {
            currentGnome.DestroyGnome(damageType);
            RemoveGnome();

            if (deathMenu) {
                Time.timeScale = 0f; // pause
                deathMenu.gameObject.SetActive(true);
                if (gameplayMenu) gameplayMenu.gameObject.SetActive(false);
            }
            else {
                StartCoroutine(ResetAfterDelay());
            }
        }
    }

    IEnumerator ResetAfterDelay() {
        yield return new WaitForSeconds(delayAfterDeath);
        Reset();
    }

    public void TrapTouched() {
        KillGnome(Gnome.DamageType.Slicing);
    }

    public void FireTrapTouched() {
        KillGnome(Gnome.DamageType.Burning);
    }

    public void TreasureCollected() {
        currentGnome.holdingTreasure = true;
    }

    public void ExitReached() {
        if (currentGnome != null && currentGnome.holdingTreasure) {
            var audio = GetComponent<AudioSource>();
            if (audio) {
                audio.PlayOneShot(this.gameOverSound);
            }

            Time.timeScale = 0.0f;

            if (gameOverMenu)
                gameOverMenu.gameObject.SetActive(true);

            if (gameplayMenu)
                gameplayMenu.gameObject.SetActive(false);
        }
    }

    // Pause & Resume
    public void SetPaused(bool paused) {
        if (paused) {
            Time.timeScale = 0.0f;
            pauseMenu.gameObject.SetActive(true);
            gameplayMenu.gameObject.SetActive(false);
        } else {
            Time.timeScale = 1.0f;
            pauseMenu.gameObject.SetActive(false);
            gameplayMenu.gameObject.SetActive(true);
        }
    }

    // Restart the game
    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



public void SetInvincible(bool value) {
    gnomeInvincible = value;
    Debug.Log("Invincible mode set to: " + value);
}


}
// END 2d_gamemanager
