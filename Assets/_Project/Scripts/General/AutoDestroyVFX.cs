using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoDestroyVFX : MonoBehaviour
{
    private ParticleSystem _ps;

    private void Start()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (_ps != null && !_ps.IsAlive(true))
        {
            Destroy(gameObject);
        }
    }
}
