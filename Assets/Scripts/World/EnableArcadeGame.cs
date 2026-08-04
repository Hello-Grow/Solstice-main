using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class EnableArcadeGame : MonoBehaviour
{
    [SerializeField] private GameObject DisplayScreen;
    [SerializeField] private GameObject Game;
    public bool isGameOver = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [YarnCommand("EnableGame")]
    public IEnumerator EnableGame()
    {
        isGameOver = false;
        Game.SetActive(true);
        Game.GetComponent<IArcadeGame>().Initalize(this);
        DisplayScreen.SetActive(true);
        yield return new WaitUntil(() => { return isGameOver; });
        Game.SetActive(false);
        DisplayScreen.SetActive(false);
    }
}
