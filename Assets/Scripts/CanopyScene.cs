using UnityEngine;
using System.Collections;

public class CanopyScene : MonoBehaviour
{

    // START
    public AudioSource background;

    public Rigidbody podDoor;
    public Animator startRobot;
    public AudioSource robotGreet;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(GameState());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator GameState()
    {
        // START DELAY
        yield return new WaitForSeconds(5);

        // OPEN DOOR
        podDoor.isKinematic = false;
        // START ROBOT
        startRobot.enabled = true;

        yield return new WaitForSeconds(6.5f);

        // ROBOT Greet 
        robotGreet.enabled = true;


        yield return new WaitForSeconds(9);

        // Start Backround Music
        background.enabled = true;

     
        yield return new WaitForSeconds(5);


        // HIDE ROBOT
        startRobot.transform.parent.gameObject.SetActive(false);

        // LOCK LID IN PLACE
        podDoor.isKinematic = true;

        // LET PLAYER MOVE
        

    }



    
}
