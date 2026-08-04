using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Yarn.Unity;

public class WishingSlip : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Sprite wishingNoteBack;
    [SerializeField] private InputActionReference normalSubmit;
    [SerializeField] private InputActionReference inputFieldSubmit;
    private bool isWishingSlipOnScreen = false;

    private bool doneTyping = false;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        inputField.onSubmit.AddListener((string userInput) => {
            StartCoroutine(PullDownWishingSlip());
        });
    }
    private void Update()
    {
        if (!inputField.isFocused&&isWishingSlipOnScreen)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
    [YarnCommand("WishingSlip")]
    public IEnumerator PullUpWishingSlip()
    {
        FindFirstObjectByType<InputSystemUIInputModule>().submit = inputFieldSubmit;
        isWishingSlipOnScreen = true;
        doneTyping = false;
        rectTransform.anchoredPosition = new Vector2(0, -1500);
        rectTransform.DOAnchorPosY(0, 1.0f).SetEase(Ease.InSine);
        yield return new WaitUntil(() => doneTyping);

    }
    private IEnumerator PullDownWishingSlip()
    {
        FindFirstObjectByType<InputSystemUIInputModule>().submit = normalSubmit;
        isWishingSlipOnScreen = false;
        rectTransform.DORotate(new Vector3(0, 180, 0), 1.0f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.5f);
        GetComponent<Image>().sprite = wishingNoteBack;
        transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        rectTransform.DOAnchorPosY(-1500, 1.0f).SetEase(Ease.InSine);
        yield return new WaitForSeconds(1.0f);
        doneTyping = true;
    }
    
}
