using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class EndCredits : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -793);
        GetComponent<RectTransform>().DOAnchorPosY(1789, 20.0f).SetEase(Ease.OutSine);
        StartCoroutine(AddQuitEvent());
    }
    private IEnumerator AddQuitEvent()
    {
        yield return new WaitForSeconds(22f);
        InputSystem.onAnyButtonPress.Call((InputControl control) => {
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                                Application.Quit();
        #endif
                });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
