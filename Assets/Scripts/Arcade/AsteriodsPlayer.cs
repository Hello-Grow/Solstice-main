using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class AsteriodsPlayer : MonoBehaviour
{
    private AsteroidControls asteroidsControls;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private AsteroidsManager asteroidsManager;
    [SerializeField] private GameObject[] LivesGameObjects;
    private Animator animator;
    private Rigidbody rigidbody;
    private float moveInput;
    private float turnInput;
    private GameObject[] bullets = new GameObject[4];
    private float bulletSpeed = 10f;
    private int Lives = 3;
    private float spawnImmunity;
    private bool isDead;
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip dieSFX;
    private AudioSource audioSource;

    private void FixedUpdate()
    {
        rigidbody.AddForce(transform.right * moveInput, ForceMode.VelocityChange);
        transform.Rotate(new Vector3(0, 0, turnInput * turnSpeed));
        transform.localPosition = asteroidsManager.WarpBounds(transform.localPosition);
        asteroidsControls.Asteroids.Shoot.performed += (InputAction.CallbackContext callbackContext) =>
        {
            if (isDead) return;
            int bulletIndex = -1;
            for (int i = 0; i < bullets.Length; i++)
            {
                if (bullets[i] == null)
                {
                    bulletIndex = i;
                    break;
                }
            }
            if (bulletIndex != -1)
            {
                audioSource.PlayOneShot(shootSFX);
                GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                bullet.transform.SetParent(asteroidsManager.transform);
                bullets[bulletIndex] = bullet;
                Destroy(bullet, 2.0f);
            }

        };
    }
    private void Awake()
    {
        audioSource =asteroidsManager.GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        asteroidsControls = new AsteroidControls();
        asteroidsControls.Asteroids.LeaveGame.performed += (InputAction.CallbackContext callback) => { asteroidsManager.GameOver(); };
    }
    private void OnEnable() { 
        asteroidsControls.Enable();
        Lives = 3;
        for(int i = 0; i < LivesGameObjects.Length; i++)
        {
            LivesGameObjects[i].SetActive(true);
        }
        rigidbody.maxLinearVelocity = maxSpeed;
        isDead = false;
        spawnImmunity = 0.0f;
        transform.localPosition = Vector3.zero;
        isDead = false;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<BoxCollider>().enabled = true;
    }
    private void OnDisable() => asteroidsControls.Disable();

    void Update()
    {
        moveInput = asteroidsControls.Asteroids.Movement.ReadValue<float>();
        turnInput = asteroidsControls.Asteroids.Turn.ReadValue<float>();
        if (moveInput == 0)
        {
            animator.Play("Squishy Idle");
        }
        else
        {
            animator.Play("Squishy Move");
        }
        foreach (GameObject bullet in bullets)
        {
            if (bullet == null) continue;
            bullet.transform.localPosition += bullet.transform.right * bulletSpeed * Time.deltaTime;
            bullet.transform.localPosition = asteroidsManager.WarpBounds(bullet.transform.localPosition);
        }
        if (spawnImmunity >= 0)
        {
            spawnImmunity -= Time.deltaTime;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Asteroid")&&spawnImmunity<=0)
        {
            audioSource.PlayOneShot(dieSFX);
            Lives -= 1;
            LivesGameObjects[Lives].SetActive(false);
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            isDead = true;
            if (Lives <= 0) asteroidsManager.GameOver();
            StartCoroutine(Respawn());
        }
    }
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(3);
        isDead = false;
        GetComponent<SpriteRenderer>().enabled= true;
        GetComponent<BoxCollider>().enabled = true;
        transform.localPosition = Vector3.zero;
        spawnImmunity = 3.0f;
    }
}
