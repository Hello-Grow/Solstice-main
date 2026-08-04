using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

public class YarnSpinnerEventHandler : ActionMarkupHandler
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip textBlip;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public async override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
    {
        await YarnTask.CompletedTask;
    }

    public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {
        audioSource.Play();
    }

    public override void OnLineDisplayComplete()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(textBlip);
    }

    public override void OnLineWillDismiss()
    {
    }

    public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
