using UnityEngine;
using UnityEngine.UI;

public class UIPlacer : MonoBehaviour
{
    public GameObject healthBar;
    
    public float paddingX = 30;
    public float paddingY = 30;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar = GameObject.Find("Canvas/HealthBar");

        float width = healthBar.GetComponent<RectTransform>().sizeDelta.x;
        float height = healthBar.GetComponent<RectTransform>().sizeDelta.y;

        Debug.Log((width, height));

        healthBar.GetComponent<RectTransform>().position = new Vector3(width / 2 + paddingX, height/2 + paddingY, 0);
        Debug.Log((width/2+paddingX, height/2+paddingY));
        Debug.Log(healthBar.GetComponent<RectTransform>().position);
    }

}
