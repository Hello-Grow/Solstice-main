using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayMainMenuBackground : MonoBehaviour
{
    private Image image;
    private RectTransform rectTransform;
    [SerializeField] private Sprite[] backgroundImages;
    private float switchTimer;
    private bool tweenedBlack = false;
    private int previousIndex;
    int sign = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (switchTimer > 0)
        {
            if (switchTimer < 3f&&!tweenedBlack)
            {
                image.DOColor(Color.black, 2f);
                tweenedBlack = true;
            }
            switchTimer -= Time.deltaTime;
            return;
        }
        tweenedBlack = false;
        switchTimer = Random.Range(15, 20);
        int index = Random.Range(0, backgroundImages.Length);
        for (int attempt = 0; attempt < 100; attempt++)
        {
            if (index != previousIndex)
            {
                previousIndex = index;
                break;
            }
            index = Random.Range(0, backgroundImages.Length);
        }
        image.sprite = backgroundImages[index];
        image.color = Color.black;
        image.DOColor(new Color(0.3f, 0.3f, 0.3f), 3f);
        rectTransform.anchoredPosition = new Vector2(Random.Range(400, 300)*sign, Random.Range(-150, 150));
        rectTransform.DOAnchorPos(new Vector2(Random.Range(400, 300)*-sign, Random.Range(-150, 150)),switchTimer);
        sign *= -1;

    }
}
