using UnityEngine;
using Yarn.Unity;

public class MonsterTower : MonoBehaviour
{
    [SerializeField] private Sprite fullMonsterTower;
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    [YarnCommand("PlaceMonster")]
    public void PlaceMonster()
    {
        spriteRenderer.sprite = fullMonsterTower;
        transform.localPosition = new Vector3(transform.localPosition.x, 0.23f, transform.localPosition.z);
    }
}
