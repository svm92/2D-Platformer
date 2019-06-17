using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

    public AudioClip grayMusic;
    public AudioClip redMusic;
    public AudioClip greenMusic;
    public AudioClip blueMusic;
    public AudioClip cyanMusic;
    public AudioClip magentaMusic;
    public AudioClip yellowMusic;
    public AudioClip rainbowMusic;
    public AudioClip miniBossMusic;
    public AudioClip mainBossMusic;
    public AudioClip finalBossMusic;

    AudioSource audioSource;
    static MusicManager musicManager;

    private void Awake()
    {
        if (musicManager == null)
        {
            DontDestroyOnLoad(gameObject);
            musicManager = this;
        }
        else if (musicManager != this)
        {
            musicManager.changeMusic();
            Destroy(gameObject);
            return;
        }
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        changeMusic();
    }

    void changeMusic()
    {
        AudioClip newClip = decideClip();
        if (audioSource.clip != newClip)
        {
            audioSource.clip = newClip;
            audioSource.Play();
        }
    }

    AudioClip decideClip()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("SceneBoss"))
            return miniBossMusic;
        else if (sceneName.StartsWith("SceneMain"))
            return mainBossMusic;
        switch (sceneName)
        {
            case "Scene00":
                return grayMusic;
            case "SceneRed":
                return redMusic;
            case "SceneGreen":
                return greenMusic;
            case "SceneBlue":
                return blueMusic;
            case "SceneCyan":
                return cyanMusic;
            case "SceneMagenta":
                return magentaMusic;
            case "SceneYellow":
                return yellowMusic;
            case "SceneRainbow":
                return rainbowMusic;
            case "SceneFinalBoss":
                return finalBossMusic;
            default:
                return null;
        }
    }

}
