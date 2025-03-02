using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/*
This code was made using these tutorials:
Brackeys - (How to make a HEALTH BAR in Unity!) https://www.youtube.com/watch?v=BLfNP4Sc_iA

and some magic by Beranger
*/


public class HealthBarController : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void setMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
    }

    public void setHealth(int health)
    {
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);

    }

}
