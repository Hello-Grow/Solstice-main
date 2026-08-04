using UnityEngine;
using Yarn.Unity;

public class PickupItem : MonoBehaviour
{
    [YarnCommand("Pickup")]
    public void Pickup()
    {
        Destroy(gameObject);
    }
}
