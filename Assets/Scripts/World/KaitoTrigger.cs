using UnityEngine;
using Yarn.Unity;

public class KaitoTrigger : MonoBehaviour
{
    [SerializeField] private GameObject arcadeKaito;
    [SerializeField] private GameObject parkKaito;
    private bool talkedToKaito = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [YarnCommand("TalkedToKaito")]
    public void TalkedToKaito()
    {
        talkedToKaito = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&&talkedToKaito)
        {
            arcadeKaito.SetActive(false);
            parkKaito.SetActive(true);
        }
    }
}
