using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject startBoat;
    
    [SerializeField] private float spawnRayRange;
    [SerializeField] private float beachHeight;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        MapGenerator.instance.GenerateMap(Random.Range(int.MinValue, int.MaxValue));
        
        SpawnBoat();

        SpawnPlayer();
        
        
    }

    private void Update()
    {
        
    }

    public void SpawnBoat()
    {
        GameObject boat = Instantiate(startBoat, FindStartPoint(), quaternion.identity);
        
        boat.transform.LookAt(new Vector3(0,beachHeight,0));
    }

    public void SpawnPlayer()
    {
        
    }

    Vector3 FindStartPoint()
    {
        Vector3 heightOffset = new Vector3(0, beachHeight, 0);
        
        // Find random point outside map
        Vector3 startPoint = new Vector3(Random.Range(0f, 1f), 0, Random.Range(0f, 1f)).normalized;
        RaycastHit hit;
        if (Physics.Raycast((startPoint * spawnRayRange) + heightOffset, -startPoint, out hit, spawnRayRange))
        {
            Debug.DrawRay(hit.point, Vector3.up * 100f, Color.red, 100f);
        }
        Debug.DrawLine(startPoint + heightOffset, Vector3.zero + heightOffset, Color.blue);

        return hit.point;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(Vector3.zero + new Vector3(0, beachHeight, 0), spawnRayRange);
    }
}
