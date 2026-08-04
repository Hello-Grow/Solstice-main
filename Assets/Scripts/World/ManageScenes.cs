using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManageScenes : MonoBehaviour
{
    [SerializeField] private string MainMenuSceneName;
    [SerializeField] private string GameSceneName;
    [SerializeField] private CanvasGroup blackOverlay;
    private void Start()
    {
        StartCoroutine(SceneFadeStart());
    }
    public void LoadGameScene()
    {
        if (SceneManager.GetActiveScene().name != MainMenuSceneName) return;
        StartCoroutine(SceneFadeTransition());

    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
    private IEnumerator SceneFadeStart()
    {
        blackOverlay.alpha = 1.0f;
        blackOverlay.blocksRaycasts = true;
        for (int i = 0; i < 50; i++)
        {
            blackOverlay.alpha -= 0.02f;
            yield return new WaitForSeconds(0.04f);

        }
        blackOverlay.blocksRaycasts = false;
    }
    private IEnumerator SceneFadeTransition()
    {
        blackOverlay.alpha = 0.0f;
        for (int i = 0; i < 50; i++)
        {
            blackOverlay.alpha += 0.02f;
            yield return new WaitForSeconds(0.02f);

        }
        SceneManager.LoadScene(GameSceneName);
    }
}
