using UnityEngine;

public class CheckpointIndicator : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            // Disappear when touched by the player
            gameObject.SetActive(false);
            Debug.Log($"Checkpoint indicator at {transform.position} collected.");
        }
    }
}
