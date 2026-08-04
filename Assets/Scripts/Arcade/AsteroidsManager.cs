using System.Collections;
using TMPro;
using UnityEngine;
using Yarn.Unity;

public class AsteroidsManager : MonoBehaviour,IArcadeGame
{
    [SerializeField] private GameObject asteroid;
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private Vector2 screenSize;
    [SerializeField] private GameObject background;
    private InMemoryVariableStorage variableStorage;
    private int score;
    public int asteroidsPresent;
    private int level = 4;
    private EnableArcadeGame enableArcadeGame;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        variableStorage = FindFirstObjectByType<InMemoryVariableStorage>();
    }
    public void Initalize(EnableArcadeGame enableArcadeGame)
    {
        this.enableArcadeGame = enableArcadeGame;
        score = 0;
        level = 4;
        asteroidsPresent = 0;
        foreach(GameObject asteroid in GameObject.FindGameObjectsWithTag("Asteroid"))
        {
            Destroy(asteroid);
        }
        scoreText.text = "Score: 0";
    }

    // Update is called once per frame
    void Update()
    {
        background.transform.localScale = screenSize*3;
        if(asteroidsPresent <= 0)
        {
            StartCoroutine(SpawnAsteroids());
            asteroidsPresent = level*4;

        }
        scoreText.transform.localPosition = new Vector3(-screenSize.x+3.5f, screenSize.y-0.5f, 0);
    }
    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score.ToString();
    }
    private IEnumerator SpawnAsteroids()
    {
        yield return new WaitForSeconds(2);
        for(int i = 0; i < level; i++)
        {
            float xSpawn = transform.position.x + screenSize.x;
            float ySpawn = transform.position.y + screenSize.y;
            GameObject currAsteroid = Instantiate(asteroid,new Vector3(Random.Range(0,2)==0 ? xSpawn:-xSpawn, Random.Range(0, 2) == 0 ? ySpawn: -ySpawn,transform.position.z),Random.rotationUniform);
            currAsteroid.GetComponent<Asteroid>().Initialize(2,this);
        }
    }
    public Vector3 WarpBounds(Vector3 currPosition)
    {
        Vector3 returnPos = currPosition;
        Vector2 warpSize = screenSize +Vector2.one*2;
        if (Mathf.Abs(currPosition.y) > warpSize.y)
        {
            returnPos = new Vector3(currPosition.x, -currPosition.y, currPosition.z);
        }
        if (Mathf.Abs(currPosition.x) > warpSize.x )
        {
            returnPos = new Vector3(-currPosition.x, currPosition.y, currPosition.z);
        }
        returnPos = new Vector3(Mathf.Clamp(returnPos.x, -warpSize.x, warpSize.x), Mathf.Clamp(returnPos.y, -warpSize.y , warpSize.y ), returnPos.z);
        return returnPos;
    }
    public void GameOver()
    {
        float prevScore = 0;
        float tryScore;
        if(variableStorage.TryGetValue("$PlayerScore",out tryScore))
        {
            prevScore = tryScore;
        }
        variableStorage.SetValue("$PlayerScore", Mathf.Max(tryScore,score));

        enableArcadeGame.isGameOver = true;
    }
}
