using UnityEngine;
using System.Collections;

public class ShackScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject robotObject;
    public GameObject clip;
    public RobotDead robotDead;
    public AudioSource finalLine;
    private bool activeScene;
    void Start()
    {
        //StartCoroutine(EnterTeleporterLine());
    }
    void OnTriggerEnter(Collider collider)
    {
        if (!activeScene)
        {
            robotObject.SetActive(true);

            StartCoroutine(EnterTeleporterLine()); // Starts the robot walking from teleporter

            clip.SetActive(true); // enables clip that blocks player from robot during death


            activeScene = true;
        } 
    }

    private IEnumerator EnterTeleporterLine()
    {
        yield return new WaitForSeconds(15f);


        finalLine.enabled = true;


        yield return new WaitForSeconds(17f);

        robotDead.dead = true;

    }



}
