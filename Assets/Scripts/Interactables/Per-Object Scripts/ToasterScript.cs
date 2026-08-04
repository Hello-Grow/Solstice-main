using UnityEngine;
using System.Collections;
using Yarn.Unity; 

public class ToasterScript : MonoBehaviour
{
    public SpriteRenderer art;
    public Sprite oldArt;
    public Sprite newArt;
    
    public float time = 0.5f;
    public float amount = 15f;
    [YarnCommand("take_toast")]
    public void TakeToast()
    {
        art.sprite = oldArt;
    }
    [YarnCommand("toast")]
    public void Go()
    {
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        Quaternion startRot = transform.rotation;
        Vector3 startAngles = transform.eulerAngles;
        
        float timer = 0f;

        // Shake
        while (timer < time)
        {
            float y = Random.Range(-amount, amount);
            float z = Random.Range(-amount, amount);

            transform.eulerAngles = startAngles + new Vector3(0f, y, z);
            
            timer += Time.deltaTime;
            yield return null; 
        }

        transform.rotation = startRot;
        art.sprite = newArt;
    }
}
