using UnityEngine;
using System.Collections.Generic;

public class EnemyLazerIndicator : MonoBehaviour, IObjectPoolable<EnemyLazerIndicator>, IObjectPooler<EnemyLazer>
{
    public EnemyLazer Prefab => _prefab;
    [SerializeField] private EnemyLazer _prefab;
    public IObjectPooler<EnemyLazerIndicator> ParentObjectPooler { get; set; }
    private EnemyLazerIndicator _instance;
    private float _spawnTime = 1f;
    private float _timeSinceAlive = 0;
    public Queue<EnemyLazer> Pool { get; } = new Queue<EnemyLazer>();

    public void OnReturn()
    {
        gameObject.SetActive(false);
        transform.parent = null;
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        _timeSinceAlive += Time.deltaTime;
        if (_timeSinceAlive >= _spawnTime)
        {
            _timeSinceAlive = 0;
            ObjectPool.Pool(this);
        }
    }

    public void OnPooled(EnemyLazer instance)
    {
        instance.transform.parent = ((MonoBehaviour)ParentObjectPooler).transform;
        instance.transform.position = transform.position;
        instance.transform.localEulerAngles = transform.localEulerAngles;
        instance.transform.localScale = transform.localScale;
        Debug.Log($"Scale: {transform.localScale}");
        instance.gameObject.SetActive(true);
        ObjectPool.Return(this);
    }

    public EnemyLazerIndicator ReturnComponent() => _instance;
}
