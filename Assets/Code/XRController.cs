using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class XRController : MonoBehaviour
{
    public TextMeshPro outText;
    public XRNode handRole = XRNode.LeftHand;

    void Update()
    {
        InputDevice controller = InputDevices.GetDeviceAtXRNode(handRole);
        outText.text = "none";


        controller.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger);
        if (trigger)
        {
            outText.text = "trigger";
        }

        controller.TryGetFeatureValue(CommonUsages.gripButton, out bool grip);
        if (grip)
        {
            outText.text = "grip";
        }

        controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryBtn);
        if (primaryBtn)
        {
            outText.text = "primary Button";
        }

        controller.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryBtn);
        if (secondaryBtn)
        {
            outText.text = "secondary Button";
        }

        controller.TryGetFeatureValue(CommonUsages.menuButton, out bool menuBtn);
        if (menuBtn)
        {
            outText.text = "menu Button";
        }
    }
}