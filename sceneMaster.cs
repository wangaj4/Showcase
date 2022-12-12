using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class sceneMaster : MonoBehaviour
{
    public static int level;


    public bool cutscene;

    [SerializeField] private Image blackTint;
    [SerializeField] private Color replacement;
    [SerializeField] private bool replace;

    [SerializeField] private ambienceFade fade;
    [SerializeField] private ambienceFade fade2;


    [SerializeField] private float fadeTime;

    [SerializeField] private pauseMenu pausemenu;
    public bool end;
    private void Awake()
    {
        Application.targetFrameRate = -1;
        Time.timeScale = 1;
        
        
        StartCoroutine(fadeFromBlack());
        if (end) Cursor.visible = true;
        
    }

    IEnumerator fadeFromBlack()
    {
        blackTint.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1);
        if (!cutscene) blackTint.gameObject.SetActive(false);
    }

    IEnumerator fadeToBlack()
    {
        StartCoroutine(fade.fadeOut());
        if(fade2 != null) StartCoroutine(fade2.fadeOut());

        if (replace) blackTint.color = replacement;

        blackTint.gameObject.SetActive(true);
        blackTint.GetComponent<Animator>().SetTrigger("ToBlack");
        
        yield break;
    }

    public IEnumerator transitionNext()
    {
        saveSystem.shouldManualLoad = false;
        if (pausemenu != null) pausemenu.saveSettings();
        StartCoroutine(fadeToBlack());
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void continueLastScene()
    {
        StartCoroutine(Continue());
        
    }

    public IEnumerator Continue()
    {
        saveSystem.AutoLoadScene();
        saveSystem.shouldManualLoad = false;
        if (saveSystem.hasAutosave)
        {


            if (saveSystem.lastCompletedScene <= 3) StartCoroutine(loadFirstNonTutorial());
            else
            {
                StartCoroutine(fadeToBlack());
                yield return new WaitForSecondsRealtime(1);
                SceneManager.LoadScene(saveSystem.lastCompletedScene + 1);
            }
        }
        
    }

    public void manualLoadScene(string name)
    {
        StartCoroutine(manualLoad(name));
    }

    public IEnumerator manualLoad(string name)
    {
        saveSystem.shouldManualLoad = true;
        saveSystem.saveID = name;
        saveSystem.ManualLoadScene();

        StartCoroutine(fadeToBlack());
        yield return new WaitForSecondsRealtime(1);
        SceneManager.LoadScene(saveSystem.toLoad);
    }

    

    public IEnumerator retryCurrentLevel()
    {
        StartCoroutine(fadeToBlack());
        yield return new WaitForSeconds(1);
        level = SceneManager.GetActiveScene().buildIndex;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void retryLastLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(level);
        
    }

    public void loadLevel(int x)
    {
        StartCoroutine(load(x));
    }

    public IEnumerator load(int x)//strictly does nothing but change the scene
    {
        if (pausemenu != null) pausemenu.saveSettings();
        StartCoroutine(fadeToBlack());
        yield return new WaitForSecondsRealtime(1);
        SceneManager.LoadScene(x);
    }

    public IEnumerator loadFirstNonTutorial()
    {
        if (pausemenu != null) pausemenu.saveSettings();
        StartCoroutine(fadeToBlack());
        yield return new WaitForSecondsRealtime(1);
        int first = getFirstLevelNum();
        SceneManager.LoadScene(first);
    }

    public int getFirstLevelNum()
    {
        return 4;
    }

    public void main()
    {

        StartCoroutine(load(0));
    }


    public void quitGame()
    {
        pausemenu.saveSettings();
        Application.Quit();
    }

    public int getSceneNumber()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

}
