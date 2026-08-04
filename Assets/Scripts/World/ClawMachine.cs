using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class ClawMachine : MonoBehaviour
{
    private bool gameOver = true;
    private ClawMachineControls controls;
    [SerializeField] private GameObject claw;
    [SerializeField] private Sprite[] clawSprites;
    [SerializeField] private GameObject pinkBunny;
    private float clawInput;
    private float leftBound = -0.35f;
    private float rightBound = 0.35f;
    private float speed = 0.2f;
    private int interactionTimes = 0;
    private void Awake()
    {
        controls = new ClawMachineControls();
        controls.Disable();
        controls.ClawMachine.LeaveGame.performed += (InputAction.CallbackContext callback) => { gameOver = true; };
        controls.ClawMachine.ClawMoveDown.performed+= (InputAction.CallbackContext callback) => {StartCoroutine(MoveClawDown()); };
    }

    private IEnumerator MoveClawDown()
    {
        controls.Disable();
        for (int i = 0; i < clawSprites.Length; i++)
        {
            claw.GetComponent<SpriteRenderer>().sprite = clawSprites[i];
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = clawSprites.Length-1; i >=0; i--)
        {
            claw.GetComponent<SpriteRenderer>().sprite = clawSprites[i];
            if (interactionTimes > 0)
            {
                pinkBunny.transform.position = claw.transform.position + Vector3.down * i * 0.1f + Vector3.forward * 0.05f;
            }
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.5f);
        if (interactionTimes > 0)
        {
            pinkBunny.SetActive(false);
        }
        interactionTimes++;
        gameOver = true;

    }
    [YarnCommand("DoClawMachine")]
    public IEnumerator DoClawMachine()
    {
        controls.Enable();
        gameOver = false;
        yield return new WaitUntil(() => { return gameOver; });
        controls.Disable();

    }
    private void Update()
    {
        clawInput = controls.ClawMachine.Move.ReadValue<float>();
    }
    private void FixedUpdate()
    {
        claw.transform.localPosition += clawInput * speed * Time.fixedDeltaTime * Vector3.right;
        claw.transform.localPosition = new Vector3(Mathf.Clamp(claw.transform.localPosition.x,leftBound,rightBound),claw.transform.localPosition.y,claw.transform.localPosition.z);
    }
}
