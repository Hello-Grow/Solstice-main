using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using Yarn.Unity;
[System.Serializable]
public class TextBubble
{
    public float typingTime = 1.0f;
    public string content = "";
    public bool isFriendText = false;
}
public class PhoneManager : MonoBehaviour
{
    [SerializeField] private TextBubble[] textBubbles;
    [SerializeField] private GameObject textBubblePrefab;
    [SerializeField] private Transform textBubblesContainer;
    [SerializeField] private Sprite friendTextSprite;
    [SerializeField] private Sprite yourTextSprite;
    private RectTransform rectTransform;
    private bool pullDownPhone = false;
    private IDisposable anyButtonCall;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
    }
    [YarnCommand("Phone")]
    public IEnumerator PullUpPhone()
    {
        pullDownPhone = false;
        rectTransform.anchoredPosition = Vector3.down * 2000;
        rectTransform.DOAnchorPosY(0, 0.5f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < textBubbles.Length; i++)
        {
            GameObject textBubbleGameObject = Instantiate(textBubblePrefab, textBubblesContainer);
            textBubbleGameObject.GetComponent<HorizontalLayoutGroup>().childAlignment = textBubbles[i].isFriendText ? TextAnchor.UpperLeft : TextAnchor.UpperRight;
            textBubbleGameObject.GetComponentInChildren<TextMeshProUGUI>().text = textBubbles[i].content;
            textBubbleGameObject.GetComponentInChildren<Image>().sprite = textBubbles[i].isFriendText ? friendTextSprite : yourTextSprite;
            LayoutRebuilder.ForceRebuildLayoutImmediate(textBubbleGameObject.transform.GetChild(0).GetComponent<RectTransform>());

            yield return new WaitForSeconds(textBubbles[i].typingTime);
        }
        anyButtonCall=InputSystem.onAnyButtonPress.Call((InputControl control) => { pullDownPhone = true; });
        yield return new WaitUntil(() => { return pullDownPhone; });
        anyButtonCall.Dispose();
        rectTransform.DOAnchorPosY(-2000, 0.5f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(0.5f);


    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
