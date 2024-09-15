using System.Collections;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Start animation")]
    [SerializeField]
    private float waitFade;
    [SerializeField]
    private float fadeDuration;

    [Header("Menus")]
    [SerializeField]
    private CanvasGroup mainMenu;
    [SerializeField]
    private CanvasGroup levelSelectionMenu;

    public bool isMainMenuActive = false;

    private void Awake()
    {
        HideMenu();
    }

    public void StartMenu()
    {
        isMainMenuActive = true;
        print("Starting up menu");
        mainMenu.alpha = 0;
        mainMenu.gameObject.SetActive(true);
        levelSelectionMenu.gameObject.SetActive(false);
        Debug.Log(mainMenu.gameObject.name + " : " + mainMenu.gameObject.activeSelf.ToString(), mainMenu.gameObject);
        Debug.Log(levelSelectionMenu.gameObject.name + " : " + levelSelectionMenu.gameObject.activeSelf.ToString(), levelSelectionMenu.gameObject);
        StartCoroutine(StartingAnimation());
    }

    public void HideMenu()
    {
        CancelAnimation();

        isMainMenuActive = false;
        print("Hidding..");
        mainMenu.gameObject.SetActive(false);
        levelSelectionMenu.gameObject.SetActive(false);
        Debug.Log(mainMenu.gameObject.name + " : " + mainMenu.gameObject.activeSelf.ToString(), mainMenu.gameObject);
        Debug.Log(levelSelectionMenu.gameObject.name + " : " + levelSelectionMenu.gameObject.activeSelf.ToString(), levelSelectionMenu.gameObject);
    }

    private IEnumerator StartingAnimation()
    {
        yield return new WaitForSeconds(waitFade);

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            mainMenu.alpha = time / fadeDuration;
            yield return null;
        }
    }

    public void ToogleLevelSelectionMenu(bool toggle)
    {
        CancelAnimation();
        levelSelectionMenu.gameObject.SetActive(toggle);
        mainMenu.gameObject.SetActive(!toggle);
    }

    private void CancelAnimation()
    {
        StopAllCoroutines();
        mainMenu.alpha = 1;
    }
}
