using UnityEngine;

public class RobotDead : MonoBehaviour
{
    private SpringFollow[] springFollow;
    private Rigidbody[] rigidbodies;

    [SerializeField] public bool dead = false;

    void Start()
    {
        // Cache the components once at the start
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        springFollow = GetComponentsInChildren<SpringFollow>();
    }

    void Update()
    {
        // Note: In a real game, you'd usually call a function once when the robot dies
        // rather than checking every frame in Update, but here is the logic you requested:
        
        if (dead)
        {
            // Disable kinematic to let gravity/physics take over
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = false;
            }

            // Disable the spring joints
            foreach (SpringFollow sj in springFollow)
            {
                sj.enabled = false;
            }
        }
        else
        {
            // Re-enable kinematic to "freeze" the parts or follow animations
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }

            // Re-enable the spring joints
            foreach (SpringFollow sj in springFollow)
            {
                sj.enabled = true;
            }
        }
    }
}