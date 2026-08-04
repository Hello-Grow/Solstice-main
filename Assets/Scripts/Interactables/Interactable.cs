using UnityEngine;
using Yarn.Unity;

public class Interactable : MonoBehaviour
{
    private DialogueRunner dialogueRunner;
    [SerializeField] private string nodeToPlayAfterInteraction;
    [SerializeField] private float interactionDistance = 1.5f;
    public Vector3 interactionOffset;
    public float InteractionDistance => interactionDistance;

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public async void OnInteract()
    {
        if (!dialogueRunner.IsDialogueRunning)
        {
            await dialogueRunner.StartDialogue(nodeToPlayAfterInteraction); 
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position+interactionOffset, interactionDistance);
    }
}
