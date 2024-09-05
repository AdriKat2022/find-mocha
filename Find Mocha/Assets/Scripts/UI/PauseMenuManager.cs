using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenu;

    private bool isGamePaused;
    private bool inAnimation;


    private void Awake()
    {
        isGamePaused = false;
        inAnimation = false;
    }


    private void Update()
    {
        if (CheckInput() && !inAnimation)
        {
            if(isGamePaused)
                UnpauseGame();
            else
                PauseGame();
        }
    }

    private bool CheckInput() => Input.GetKeyDown(KeyCode.Escape);

    public void PauseGame()
    {
        if (!GameManager.Instance.CanPauseGame)
            return;

        Time.timeScale = 0f;
        isGamePaused = true;
        SoundManager.Instance.PlaySound(SoundManager.Instance.pause_sound);
        pauseMenu.SetActive(true);
    }

    public void UnpauseGame()
    {
        isGamePaused = false;
        pauseMenu.SetActive(false);
        SoundManager.Instance.PlaySound(SoundManager.Instance.unpause_sound);
        Time.timeScale = 1f;
    }
}
