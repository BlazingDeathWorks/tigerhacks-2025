using UnityEngine;

public class EnemyLazer : MonoBehaviour, IObjectPoolable<EnemyLazer>
{
    public IObjectPooler<EnemyLazer> ParentObjectPooler { get; set; }

    public void OnReturn() => gameObject.SetActive(false);

    public EnemyLazer ReturnComponent() => this;
    private float _lifetime = 2f;
    private float _timeSinceAlive = 0;

    private void Update()
    {
        _timeSinceAlive += Time.deltaTime;
        if (_timeSinceAlive >= _lifetime)
        {
            _timeSinceAlive = 0;
            ObjectPool.Return(this);
        }
    }
}
