using UnityEngine;

public class DashTrailEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float lifetime;
    
    public void Initialize(float animationLength)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        lifetime = animationLength;
        
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {

    }
}