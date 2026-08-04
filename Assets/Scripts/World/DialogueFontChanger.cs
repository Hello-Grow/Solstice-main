using TMPro;
using UnityEngine;

public class DialogueFontChanger : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTransformChildrenChanged()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach(TextMeshProUGUI text in texts)
        {
            text.font = font;
        }
    }
}
