using UnityEngine;

public class ProgressScript : MonoBehaviour
{
    public static ProgressScript Instance { get; private set; }

    public int roomCount = 5;
    public int branchMinCount = 1;
    public int branchMaxCount = 2;
    public int endDistance = 4;
    public float HealthMultiplier = 0.5f;

    private void Awake()
    {
        Instance = this;
    }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { 
        
        DontDestroyOnLoad(gameObject);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
