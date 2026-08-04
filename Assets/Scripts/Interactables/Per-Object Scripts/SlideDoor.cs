using UnityEngine;

public class SlideDoor : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    public Vector3 leftDoorOffset = new Vector3(-2f, 0f, 0f);
    public Vector3 rightDoorOffset = new Vector3(2f, 0f, 0f);
    public float speed = 3f;
    public string triggerTag = "Player"; //check tag
    //track pos
    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;
    private bool isOpen = false;

    void Start()
    {
        //rememebr starting pos
        leftClosedPos = leftDoor.localPosition;
        leftOpenPos = leftClosedPos + leftDoorOffset;
        rightClosedPos = rightDoor.localPosition;
        rightOpenPos = rightClosedPos + rightDoorOffset;

    }

    void Update()
    {
        //move left with smooth 
        if (leftDoor != null)
        {
            Vector3 leftTarget = isOpen ? leftOpenPos : leftClosedPos;
            leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftTarget, speed * Time.deltaTime);
        }

        //move right
        if (rightDoor != null)
        {
            Vector3 rightTarget = isOpen ? rightOpenPos : rightClosedPos;
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightTarget, speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Open the doors if the entering object has the correct tag
        if (other.CompareTag(triggerTag))
        {
            isOpen = true;
            InteractionSingleton.Instance.PlaySound("Slide Open");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Close the doors when the object leaves the trigger zone
        if (other.CompareTag(triggerTag))
        {
            isOpen = false;
            InteractionSingleton.Instance.PlaySound("Slide Close");

        }
    }
}
