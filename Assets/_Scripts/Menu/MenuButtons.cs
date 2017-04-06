using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MenuButtons : MonoBehaviour
{
    public GameObject titleUI;
    public GameObject controlsUI;
    public GameObject playSettingsUI;

    public AudioClip buttonSound;

    private AudioSource _audioSource;

    public void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        SwitchUI(titleUI);
    }

    public void SwitchUI(GameObject switchTo)
    {
        titleUI.SetActive(false);
        controlsUI.SetActive(false);
        playSettingsUI.SetActive(false);

        switchTo.SetActive(true);
    }

    public void PlayGame(string sceneName)
    {
        _audioSource.PlayOneShot(buttonSound);
        SceneManager.LoadScene(sceneName);
    }

    public void GoToPlaySettings()
    {
        _audioSource.PlayOneShot(buttonSound);
        SwitchUI(playSettingsUI);
    }

    public void GoToControls()
    {
        _audioSource.PlayOneShot(buttonSound);
        SwitchUI(controlsUI);
    }

    public void GoToMainMenu()
    {
        _audioSource.PlayOneShot(buttonSound);
        SwitchUI(titleUI);
    }

    public void QuitGame()
    {
        _audioSource.PlayOneShot(buttonSound);
        Application.Quit();
    }
}
