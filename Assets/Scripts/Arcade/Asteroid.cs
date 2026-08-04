using UnityEngine;
using UnityEngine.Rendering;
[System.Serializable]
public class AsteroidLevelArray
{
    public Sprite[] sprites;
    public AudioClip[] deathSounds;
}

public class Asteroid : MonoBehaviour
{
    private int stage = 2;
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private AsteroidLevelArray[] asteroidLevelArrays;
    private AsteroidsManager asteroidsManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, 0f, currentEuler.z);
    }
    public void Initialize(int startStage,AsteroidsManager asteroidsManager)
    {
        this.asteroidsManager = asteroidsManager;
        transform.SetParent(asteroidsManager.transform);
        stage = startStage;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        spriteRenderer.sprite = asteroidLevelArrays[stage].sprites[Random.Range(0, asteroidLevelArrays[stage].sprites.Length)];
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        sphereCollider.radius = Mathf.Max(spriteSize.x, spriteSize.y) / 2;
    }
    // Update is called once per frame
    void Update()
    {
        transform.localPosition += 2 * (4 - stage)*transform.right*Time.deltaTime;
        transform.localPosition= asteroidsManager.WarpBounds(transform.localPosition);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AsteroidBullet"))
        {
            asteroidsManager.GetComponent<AudioSource>().PlayOneShot(asteroidLevelArrays[stage].deathSounds[Random.Range(0, asteroidLevelArrays[stage].deathSounds.Length)]);
            if (stage > 0)
            {
                for(int i = 0; i < 2; i++)
                {
                    GameObject splitAsteroid = Instantiate(asteroidPrefab,transform.position,Random.rotationUniform);
                    splitAsteroid.GetComponent<Asteroid>().Initialize(stage - 1,asteroidsManager);

                }
            }
            else
            {
                FindFirstObjectByType<AsteroidsManager>().asteroidsPresent -= 1;
            }
            int scoreAmount = 0;
            switch (stage)
            {
                case 2:
                    scoreAmount = 20;
                    break;
                case 1:
                    scoreAmount = 50;
                    break;
                case 0:
                    scoreAmount = 100;
                    break;
            }
            FindFirstObjectByType<AsteroidsManager>().AddScore(scoreAmount);
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
