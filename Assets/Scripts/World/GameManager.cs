using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CanvasGroup blackOverlay;
    [SerializeField] private Light mainLight;
    [SerializeField] private Light fakeBounceLight;
    [SerializeField] private Material atmosphereMaterial;
    [SerializeField] private Transform SunTransform;
    private InMemoryVariableStorage inMemoryVariableStorage;
    private Bloom bloom;
    private Volume volume;
    private DialogueRunner dialogueRunner;
    private float timer = 0;
    private float startTime = 300;
    private bool gameStarted = false;
    private Color startSunColor = new Color(1, 0.95f, 0.83f);
    private Color endSunColor = new Color(1, 0.5f, 0.0f);
    private bool startedEndTransition = false;
    private void Start()
    {
        timer = startTime;
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        dialogueRunner.onDialogueComplete.AddListener(StartGame);
        playerController.enabled = false;
        StartCoroutine(PlayStartingSequence());
        volume = FindFirstObjectByType<Volume>();
        volume.profile.TryGet(out bloom);
        inMemoryVariableStorage = FindFirstObjectByType<InMemoryVariableStorage>();
        UpdateSunset();
    }
    public float GetSunsetLerp()
    {
        return (1.0f-timer / startTime);
    }
    private IEnumerator PlayStartingSequence()
    {
        blackOverlay.alpha= 1.0f;
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 50; i++)
        {
            blackOverlay.alpha -= 0.02f;
            yield return new WaitForSeconds(0.05f);

        }
        yield return new WaitForSeconds(1.0f);
        PlayStartingCutscene();
    }
    private IEnumerator TransitionToEnd()
    {
        yield return new WaitForSeconds(3f);
        blackOverlay.alpha= 0.0f;
        for (int i = 0; i < 50; i++)
        {
            blackOverlay.alpha += 0.02f;
            yield return new WaitForSeconds(0.02f);

        }
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("End Credits");
    }
    public async void PlayStartingCutscene()
    {
        if (!dialogueRunner.IsDialogueRunning)
        {
            await dialogueRunner.StartDialogue("StartingCutscene");
        }
    }
    private void StartGame()
    {
        gameStarted = true;
        playerController.enabled = true;
        dialogueRunner.onDialogueComplete.RemoveListener(StartGame);
    }
    private void UpdateSunset()
    {
        float sunsetLerp = GetSunsetLerp();
        if (bloom != null)
        {
            bloom.tint.value = Color.Lerp(Color.white, new Color(1, 0.5652905f, 0), sunsetLerp);
            bloom.intensity.value = Mathf.Lerp(5, 14, sunsetLerp);
        }
        SunTransform.position = new Vector3(SunTransform.position.x, Mathf.Lerp(-1650, -350, sunsetLerp), SunTransform.position.z);
        mainLight.color = Color.Lerp(startSunColor, endSunColor, sunsetLerp);
        fakeBounceLight.color = Color.Lerp(startSunColor, endSunColor, sunsetLerp);
        atmosphereMaterial.SetFloat("_SunsetLerp", sunsetLerp);
        atmosphereMaterial.SetVector("_SunPos", Camera.main.WorldToViewportPoint(SunTransform.position));
    }
    private void Update()
    {
        if (!gameStarted) return;
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        minutes = Mathf.Max(0, minutes);
        seconds = Mathf.Max(0, seconds);
        string timeString = string.Format("{0:0}:{1:00}", minutes, seconds);


        if (timer <= 0&&!startedEndTransition)
        {
            startedEndTransition = true;
            StartCoroutine(TransitionToEnd());
            playerController.enabled = false;
        }
        timerText.text = timeString;
        timer -= Time.deltaTime;
        inMemoryVariableStorage.SetValue("$current_time", timer);
        UpdateSunset();
        //Debug.Log(Camera.main.WorldToViewportPoint(SunTransform.position));
    }
}
