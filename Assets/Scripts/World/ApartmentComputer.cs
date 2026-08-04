using UnityEngine;
using Yarn.Unity;

public class ApartmentComputer : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Sprite turnedOffComputer;
    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    [YarnCommand("TurnOffComputer")]
    public void TurnOffComputer()
    {
        animator.enabled = false;
        spriteRenderer.sprite = turnedOffComputer;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
