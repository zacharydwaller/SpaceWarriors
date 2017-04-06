using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Singleton (kind-of) Settings object
public class Settings : MonoBehaviour
{
    public int scoreToWin = 3;
    public bool useBlackHole = true;

    private static Settings singletonRef;

    private void Awake()
    {
        if(singletonRef == null)
        {
            singletonRef = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    // Following functions used by UI elements

    public void SetScoreToWin(string newValue)
    {
        if(int.TryParse(newValue, out scoreToWin) == false)
        {
            // Invalid text entered
            scoreToWin = 3;
        }

        if(scoreToWin <= 0)
        {
            scoreToWin = 1;
        }
    }

    public void SetUseBlackHole(bool newValue)
    {
        useBlackHole = newValue;
    }
}
