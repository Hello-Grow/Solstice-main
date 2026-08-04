using UnityEngine;
using Yarn.Unity;

public class BedScript : MonoBehaviour
{
    public SpriteRenderer art;
    public Sprite newArt; //

    [YarnCommand("make_bed")]
    public void MakeBed()
    {
        if (art != null && newArt != null)
        {
            art.sprite = newArt;
        }
    }
}
