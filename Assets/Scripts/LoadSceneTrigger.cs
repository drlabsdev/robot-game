using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene(1);
    }

}
