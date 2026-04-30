using System.Collections;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public BatteryTrigger batteryTrigger;
    public GameObject teleportTrigger;

    public GameObject robot;
    public GameObject robotEnter;
    public AudioSource enterLine;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (batteryTrigger.hasBattery && !teleportTrigger.activeInHierarchy)
        {
            teleportTrigger.SetActive(true);


            robot.SetActive(false);
            robotEnter.SetActive(true);


            StartCoroutine(EnterTeleporterLine());
        }
    }
    private IEnumerator EnterTeleporterLine()
    {
        yield return new WaitForSeconds(20f);

        enterLine.Play();


    }
}
