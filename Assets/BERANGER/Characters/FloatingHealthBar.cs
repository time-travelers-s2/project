using UnityEngine;
using UnityEngine.UI;

/*
This code was made using these tutorials:
BMo - (Easy Enemy Health Bars in Unity) https://www.youtube.com/watch?v=_lREXfAMUcE

and some magic by Beranger
*/
public class FloatingHealthBar : MonoBehaviour
{
    public GameObject healthBarHolder;
    public Slider slider;
    public new Camera camera;
    //public Gradient gradient;
    //public Image fill;
    private bool firstHit;

    public void setup(int maxHealth, int health)
    {
        setMaxHealth(maxHealth);
        setHealth(health);
        healthBarHolder.SetActive(false);
        firstHit = true;
    }

    public void setMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
    }

    public void setHealth(int health)
    {
        if(firstHit)
        {
            healthBarHolder.SetActive(true);
            firstHit = false;
        }
        slider.value = health;
        //fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void Update()
    {
        transform.rotation = camera.transform.rotation;
    }
}
