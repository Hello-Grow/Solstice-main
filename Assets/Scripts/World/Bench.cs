using UnityEngine;
using Yarn.Unity;

public class Bench : MonoBehaviour
{

    [SerializeField] private PlayerController playerController;
    [SerializeField] private Sprite faceBackwardSprite;
    [YarnCommand("SitOnBench")]
    public void SitOnBench()
    {
        FindFirstObjectByType<InteractionSingleton>().endOfGame = true;
        float prevY = playerController.transform.position.y;
        playerController.transform.position = transform.position + transform.forward * 0.3f;
        playerController.transform.position = new Vector3(playerController.transform.position.x,prevY,playerController.transform.position.z);
        playerController.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = faceBackwardSprite;
    }
}
