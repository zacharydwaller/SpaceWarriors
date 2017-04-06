using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatBars : MonoBehaviour
{
    public Slider healthBar;
    public Text healthText;
    public Slider missileCDBar;
    public Text teleportsText;

    private ShipBase _shipBase;

    public void Start()
    {
        _shipBase = GetComponent<ShipBase>();

        healthBar.minValue = 0f;
        healthBar.maxValue = _shipBase.maxHP;

        healthText.text = _shipBase.maxHP.ToString();

        missileCDBar.minValue = 0f;
        missileCDBar.maxValue = _shipBase.secondaryFireDelay;

        teleportsText.text = _shipBase.teleportsRemaining.ToString();
    }

    public void Update()
    {
        healthBar.value = _shipBase.currentHP;

        healthText.text = ((int) _shipBase.currentHP).ToString();

        float timeRemaining;
        timeRemaining = Mathf.Clamp(_shipBase.secondaryFireNext - Time.time, missileCDBar.minValue, missileCDBar.maxValue);
        missileCDBar.value = missileCDBar.maxValue - timeRemaining;

        teleportsText.text = _shipBase.teleportsRemaining.ToString();
        if(_shipBase.teleportsRemaining == 0)
        {
            teleportsText.color = Color.red;
        }
        else
        {
            teleportsText.color = Color.white;
        }
    }
}
