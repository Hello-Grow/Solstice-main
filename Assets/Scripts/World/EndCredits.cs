using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Fixed capitalization here
using UnityEngine.InputSystem.Utilities; // Fixed capitalization here

public class EndCredits : MonoBehaviour
{
    [Header("Credit Animation Settings")]
    [SerializeField] private float startPositionY = -2500f;
    [SerializeField] private float endPositionY = 1789f;
    [SerializeField] private float scrollDuration = 20.0f;
    [SerializeField] private Ease scrollEase = Ease.OutSine;

    [Header("Quit Settings")]
    [SerializeField] private float delayBeforeQuitEnabled = 22f;

    private System.IDisposable _anyButtonPressAction;
    private RectTransform _rectTransform;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        // Initialize position and start tween
        _rectTransform.anchoredPosition = new Vector2(0, startPositionY);
        _rectTransform.DOAnchorPosY(endPositionY, scrollDuration).SetEase(scrollEase);
        
        StartCoroutine(AddQuitEvent());
    }

    private IEnumerator AddQuitEvent()
    {
        yield return new WaitForSeconds(delayBeforeQuitEnabled);
        
        // Store the action reference so we can safely dispose of it later
        _anyButtonPressAction = InputSystem.onAnyButtonPress.Call((InputControl control) => 
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
    }

    private void OnDestroy()
    {
        // Clean up the event listener if the scene changes or object is destroyed
        _anyButtonPressAction?.Dispose();
    }
}
