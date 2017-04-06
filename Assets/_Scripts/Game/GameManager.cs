using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    const string humanTag = "Human";
    const string alienTag = "Alien";

    public int scoreToWin = 3;

    public Text humanScoreText;
    public Text alienScoreText;

    private GameObject humanObj;
    private ShipHuman humanScript;
    private Vector3 humanStartPosition;
    private Quaternion humanStartRotation;
    public int humanScore;

    private GameObject alienObj;
    private ShipAlien alienScript;
    private Vector3 alienStartPosition;
    private Quaternion alienStartRotation;
    public int alienScore;

    public Text countDownText;
    private int currentCountDown;
    public AudioClip countDownBeep;

    public Text winnerText;

    public GameObject matchOverUI;

    public bool isPaused;
    private KeyCode pauseButton = KeyCode.Escape;
    public GameObject pauseUI;

    public AudioClip buttonSound;
    private AudioSource _audioSource;

    private Settings _settings;

    public void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        InitializeSettings();

        // Initialize Human info
        humanObj = GameObject.FindGameObjectWithTag(humanTag);
        humanScript = humanObj.GetComponent<ShipHuman>();
        humanStartPosition = humanObj.transform.position;
        humanStartRotation = humanObj.transform.rotation;
        humanScore = 0;
        humanScoreText.text = humanScore.ToString();

        // Initialize Alien info
        alienObj = GameObject.FindGameObjectWithTag(alienTag);
        alienScript = alienObj.GetComponent<ShipAlien>();
        alienStartPosition = alienObj.transform.position;
        alienStartRotation = alienObj.transform.rotation;
        alienScore = 0;
        alienScoreText.text = alienScore.ToString();

        // Initialize UI
        countDownText.gameObject.SetActive(false);
        winnerText.gameObject.SetActive(false);
        matchOverUI.gameObject.SetActive(false);

        pauseUI.SetActive(false);

        StartGame();
    }

    public void Update()
    {
        if(Input.GetKeyDown(pauseButton))
        {
            isPaused = !isPaused;

            if(isPaused)
            {
                Pause();
            }
            else
            {
                UnPause();
            }
        }
    }

    public void PlayerDefeated(string defeatTag)
    {
        string victorTag;

        if(defeatTag == humanTag) victorTag = alienTag;
        else victorTag = humanTag;

        AddScore(victorTag, 1);
        // End of the whole match
        if(CheckIfWin(victorTag))
        {
            ResetPlayers();
            FreezePlayers();
            matchOverUI.gameObject.SetActive(true);
        }
        // End of the round
        else
        {
            winnerText.gameObject.SetActive(true);
            winnerText.text = victorTag + " wins the round!";
            ResetPlayers();
            StartGame();
        }
    }

    public void StartGame()
    {
        FreezePlayers();
        DoCountDown();
    }

    public void ResetPlayers()
    {
        humanObj.SendMessage("InitializeStats");
        humanObj.transform.position = humanStartPosition;
        humanObj.transform.rotation = humanStartRotation;

        alienObj.SendMessage("InitializeStats");
        alienObj.transform.position = alienStartPosition;
        alienObj.transform.rotation = alienStartRotation;

        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach(GameObject projectile in projectiles)
        {
            GameObject.Destroy(projectile);
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;

        _audioSource.PlayOneShot(buttonSound);

        isPaused = true;
        pauseUI.SetActive(true);
    }

    public void UnPause()
    {
        Time.timeScale = 1f;

        _audioSource.PlayOneShot(buttonSound);

        isPaused = false;
        pauseUI.SetActive(false);
    }

    public void QuitGame()
    {
        // Have to unpause or else main menu will be paused
        UnPause();
        GameObject.Destroy(GameObject.FindGameObjectWithTag("SettingsHolder"));
        SceneManager.LoadScene(0);
    }

    public void FreezePlayers()
    {
        humanScript.Freeze();
        alienScript.Freeze();
    }

    public void UnFreezePlayers()
    {
        humanScript.UnFreeze();
        alienScript.UnFreeze();
    }

    public void DoCountDown()
    {
        countDownText.gameObject.SetActive(true);
        currentCountDown = 3;

        StartCoroutine("CountDownHelper");
    }

    IEnumerator CountDownHelper()
    {
        // Do countdown
        while(currentCountDown > 0)
        {
            _audioSource.PlayOneShot(countDownBeep);
            countDownText.text = currentCountDown.ToString();
            currentCountDown--;
            yield return new WaitForSeconds(1.0f);
        }

        //Countdown finished
        UnFreezePlayers();

        countDownText.gameObject.SetActive(false);
        winnerText.gameObject.SetActive(false);

        yield return null;
    }

    public void AddScore(string tag, int scoreAdded)
    {
        if(tag == humanTag)
        {
            humanScore += scoreAdded;
            humanScoreText.text = humanScore.ToString();
        }
        else
        {
            alienScore += scoreAdded;
            alienScoreText.text = alienScore.ToString();
        }
    }

    public bool CheckIfWin(string tag)
    {
        bool didWin = false;
        if(humanScore >= scoreToWin || alienScore >= scoreToWin)
        {
            didWin = true;
        }

        if(didWin)
        {
            winnerText.gameObject.SetActive(true);
            winnerText.text = tag + " won the game!";
        }

        return didWin;
    }

    public void InitializeSettings()
    {
        GameObject settingsObject = GameObject.FindGameObjectWithTag("SettingsHolder");
        GameObject blackHole = GameObject.FindGameObjectWithTag("BlackHole");

        // If no settings object just use default settings
        if(!settingsObject) return;

        _settings = settingsObject.GetComponent<Settings>();

        scoreToWin = _settings.scoreToWin;

        // Have to check for null here OR ELSE
        if(blackHole != null)
        {
            if(_settings.useBlackHole)
            {
                blackHole.SetActive(true);
            }
            else
            {
                blackHole.SetActive(false);
            }
        }
    }

    public void RestartButtonPressed()
    {
        _audioSource.PlayOneShot(buttonSound);

        AddScore(humanTag, -humanScore);
        AddScore(alienTag, -alienScore);

        winnerText.gameObject.SetActive(false);
        matchOverUI.gameObject.SetActive(false);

        ResetPlayers();
        StartGame();
    }
}
