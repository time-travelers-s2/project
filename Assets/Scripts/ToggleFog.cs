using UnityEngine;
using System.Collections;

public class ToggleFog : MonoBehaviour
{
    public GameObject fog;
    private MeshRenderer mesh;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("found fog");
        fog = GameObject.Find("fog");
        mesh = fog.GetComponent<MeshRenderer>();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        mesh.enabled = true;
        Debug.Log("TRIGGER ENTER");
    }

    private void OnTriggerExit(Collider other) {
        mesh.enabled = false;
        Debug.Log("TRIGGER EXIT");
    }
}
