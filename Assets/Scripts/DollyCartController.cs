using UnityEngine;

public class DollyCartController : MonoBehaviour {
    public Transform player; // Assign your player transform in the Inspector
    public Cinemachine.CinemachineDollyCart dollyCart; // Assign the dolly cart component in the Inspector
    public float speed = 5f;

    [HideInInspector]
    public bool playerInThreshold = false;
    private bool movingForward = true;

    void Update() {
        if (playerInThreshold) {
            if (movingForward) {
                dollyCart.m_Position += speed * Time.deltaTime;
            }
            else {
                dollyCart.m_Position -= speed * Time.deltaTime;
            }
        }
    }

    public void ReverseDirection() {
        movingForward = !movingForward;
    }
}