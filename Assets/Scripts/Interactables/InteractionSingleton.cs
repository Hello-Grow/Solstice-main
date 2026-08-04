using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
[System.Serializable]
public class SoundNameKeyValPair
{
    public string soundName;
    public AudioClip audioClip;
}
public class InteractionSingleton : Singleton<InteractionSingleton>
{
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SoundNameKeyValPair[] soundNameKeyValPairs;
    private Dictionary<string,AudioClip> sounds = new Dictionary<string,AudioClip>();
    private AudioSource audioSource;
    private PlayerController playerController;
    private DialogueRunner dialogueRunner;
    private float interactionCooldown = 0;
    public bool endOfGame;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        foreach(SoundNameKeyValPair soundNameKeyValPair in soundNameKeyValPairs)
        {
            sounds.Add(soundNameKeyValPair.soundName, soundNameKeyValPair.audioClip);
        }
    }
    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += FindInteractable;
        playerController = FindFirstObjectByType<PlayerController>();
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        dialogueRunner.onDialogueStart.AddListener(() => { playerController.enabled = false; });
        dialogueRunner.onDialogueComplete.AddListener(() => { if (!endOfGame) { playerController.enabled = true; } interactionCooldown = 0.5f; });
    }
    private void Update()
    {
        if(interactionCooldown>0) interactionCooldown-= Time.deltaTime;
    }
    private void OnDestroy()
    {
        interactAction.action.Disable();
        interactAction.action.performed -= FindInteractable;
    }
    [YarnCommand("PlaySound")]
    public void PlaySound(string soundName)
    {
        if (sounds.ContainsKey(soundName))
        {
            audioSource.PlayOneShot(sounds[soundName]);
        }
        else
        {
            Debug.LogWarning("Didn't find sound " + soundName + " in the dictionary");
        }
    }
    [YarnCommand("PlaySoundAndWait")]
    public IEnumerator PlaySoundAndWait(string soundName,float time)
    {
        if (sounds.ContainsKey(soundName))
        {
            audioSource.PlayOneShot(sounds[soundName]);
        }
        else
        {
            Debug.LogWarning("Didn't find sound " + soundName + " in the dictionary");
        }
        yield return new WaitForSeconds(time);
    }
    private void FindInteractable(InputAction.CallbackContext callbackContext) 
    {
        if (interactionCooldown > 0) return;
        float minDistance = int.MaxValue;
        Interactable closestInteractable = null;
        Interactable[] interactables = FindObjectsByType<Interactable>(FindObjectsSortMode.None);

        foreach (Interactable interactable in interactables) 
        {
            Vector3 interactionPosition = interactable.transform.position + interactable.interactionOffset;
            float currDistance = Vector3.Distance(interactionPosition, playerTransform.position);

            if (interactable == null || currDistance > interactable.InteractionDistance)
            {
                continue;
            }
            RaycastHit hitInfo;
            bool hitSomething = Physics.Raycast(interactionPosition, playerTransform.position - interactionPosition, out hitInfo);
            if(hitSomething && hitInfo.collider.CompareTag("Wall")){
                continue;
            }
            if (currDistance < minDistance)
            {
                minDistance = currDistance;
                closestInteractable = interactable;
            }
        }

        if (closestInteractable != null && minDistance < closestInteractable.InteractionDistance) 
        {
            closestInteractable.OnInteract();
        }
    }
}
