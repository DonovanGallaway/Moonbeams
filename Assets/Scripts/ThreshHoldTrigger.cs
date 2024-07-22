using UnityEngine;

public class ThresholdTrigger : MonoBehaviour {
    public DollyCartController dollyCartController;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) // Ensure your player has the "Player" tag
        {
            dollyCartController.playerInThreshold = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            dollyCartController.playerInThreshold = false;
            dollyCartController.ReverseDirection();
        }
    }
}