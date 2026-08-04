using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WrapAroundInjection : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    private List<Selectable> optionViews = new List<Selectable>();
    private List<bool> optionsEnabled = new List<bool>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(InjectNavigation());
    }
    private int Wrap(int index, int size)
    {
        return ((index % size) + size) % size;
    }
    private void OnTransformChildrenChanged()
    {
        optionViews.Clear();
        optionsEnabled.Clear();
        for (int i = 2; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<Selectable>(out var view))
            {
                optionViews.Add(view);
                optionsEnabled.Add(false);
            }
        }
    }
    private IEnumerator InjectNavigation()
    {
        int endIndex = 0;
        for (int i = 0; i < optionViews.Count; i++)
        {
            if (optionViews[i].gameObject.activeSelf == optionsEnabled[i]) continue;
            optionsEnabled[i] = optionViews[i].gameObject.activeSelf;
            if (optionsEnabled[i])
            {
                endIndex = i+1;
            }
        }
        for (int i = 0; i < endIndex;i++) {

            UnityEngine.UI.Navigation customNav = optionViews[i].navigation;
            optionViews[i].GetComponent<TextMeshProUGUI>().font = font;
            optionViews[i].GetComponent<TextMeshProUGUI>().fontSize = 64;
            customNav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            customNav.selectOnDown = optionViews[Wrap(i + 1, endIndex)];
            customNav.selectOnUp = optionViews[Wrap(i - 1, endIndex)];
            optionViews[i].navigation = customNav;
            if (i == 0)
            {
                EventSystem.current.SetSelectedGameObject(null);
                yield return null;
                optionViews[i].Select();
            }
        }
    }
}
;