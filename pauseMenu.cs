using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pauseMenu : MonoBehaviour
{

    public bool startPaused;
    private bool canUnpause;

    private static bool healthBars;
    private static bool numbers;
    [SerializeField] private bool mainMenu;

    [SerializeField] private GameObject enableBorder;
    [SerializeField] private GameObject disableBorder;


    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private List<GameObject> turnoff;
    [SerializeField] private playerMaster master;
    private bool paused;

    [SerializeField] private GameObject crosshair;

    private int volume = 7;//should be saved
    [SerializeField] private Image volumeSlider;
    [SerializeField] private List<Sprite> volumes;


    public bool getPaused()
    {
        return paused;
    }

    private void Awake()
    {
        if(master!=null) pauseScreen.SetActive(false);
        paused = false;
        Time.timeScale = 1;
        foreach (GameObject x in turnoff)
        {
            x.SetActive(false);
        }

        if (mainMenu)
        {
            settingsInfo saved = saveSystem.LoadSettings();
            AudioListener.volume = saved.volume;
            healthBars = saved.bars;
            numbers = saved.respawn;
        }

        healthBarStatus(healthBars);
        enableBorder.SetActive(healthBars);
        disableBorder.SetActive(!healthBars);

        

        int v = (int)(AudioListener.volume * 10);
        setVolume(v);
    }

    private void Start()
    {
        if (startPaused)
        {
            pauseToggle();
            canUnpause = false;
        }
        else canUnpause = true;
    }

    public void saveSettings()
    {
        settingsInfo toSave = new settingsInfo(AudioListener.volume, healthBars, numbers);
        saveSystem.SaveSettings(toSave);
    }

    
    private void Update()
    {
        if (Input.GetButtonDown("Cancel") && master != null && canUnpause)
        {
            pauseToggle();
            
        }
    }

    public void healthBarsToggle(bool on)
    {
        healthBars = on;
        healthBarStatus(on);
    }

    

    private void healthBarStatus(bool on)
    {
        foreach (GameObject barParent in GameObject.FindGameObjectsWithTag("healthParent"))
        {
            barParent.GetComponent<healthBar>().toggleParent(on);
        }
    }

    public bool healthBarsOn()
    {
        return healthBars;
    }

    public void pauseToggle()
    {
        canUnpause = true;
        if (paused)
        {
            foreach(GameObject x in turnoff)
            {
                x.SetActive(false);
            }
            pauseScreen.SetActive(false);
            paused = false;
            Time.timeScale = 1;
            master.PauseGame(false);
            Cursor.visible = false;
            crosshair.SetActive(true);
            continueAudio();
        }
        else
        {
            pauseScreen.SetActive(true);
            paused = true;
            Time.timeScale = 0;
            master.PauseGame(true);
            Cursor.visible = true;
            crosshair.SetActive(false);
            pauseAudio();
        }
    }

    private void pauseAudio()
    {
        AudioSource[] allAudio = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach (AudioSource sound in allAudio){

            if (sound.gameObject.tag != "Ambience") sound.Pause();
            
            
        }
    }

    private void continueAudio()
    {
        AudioSource[] allAudio = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach (AudioSource sound in allAudio)
        {
            sound.UnPause();
        }
    }



    public void volumeUp()
    {
        volume += 1;
        if (volume > 10) volume = 10;
        setVolume(volume);
    }

    public void volumeDown()
    {
        volume -= 1;
        if (volume < 1) volume = 1;
        setVolume(volume);
    }

    public void setVolume(int level)
    {
        volume = level;
        volumeSlider.sprite = volumes[level - 1];
        AudioListener.volume = volume / 10f;
    }
}
