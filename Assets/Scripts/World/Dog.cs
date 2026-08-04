using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class Dog : MonoBehaviour
{
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("DogIdleAnimation");
    }
    [YarnCommand("PetDog")]
    public IEnumerator PetDog() {
        animator.Play("DogAnimation");
        yield return new WaitForSeconds(2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
