using UnityEngine;

public class BatteryTrigger : MonoBehaviour
{
    public bool hasBattery;
    void OnTriggerEnter(Collider other)
    {
        
        hasBattery = true;
        
    }
    void OnTriggerExit(Collider other)
    {
        
        hasBattery = false;
        
    }
}
