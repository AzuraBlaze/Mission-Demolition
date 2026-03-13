using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Inscribed")]
    public float explosionRadius = 5f;
    public float explosionForce = 800f;
    public float triggerImpulse = 2f;       // Minimum hit force to detonate
    public GameObject explosionVFXPrefab;   // Optional: particle effect prefab

    private bool _detonated = false;

    // Called when another Rigidbody hits this object
    void OnCollisionEnter(Collision collision)
    {
        // Only detonate if hit hard enough (avoids gentle-touch triggers)
        if (collision.impulse.magnitude >= triggerImpulse)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        if (_detonated) return;  // Guard: only explode once
        _detonated = true;

        // 1. Spawn optional VFX at this position
        if (explosionVFXPrefab != null)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        // 2. Find every Rigidbody within the blast radius and apply force
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

            // 3. Chain-detonate any nearby Explosives
            Explosive nearbyExplosive = hit.GetComponent<Explosive>();
            if (nearbyExplosive != null && nearbyExplosive != this)
                nearbyExplosive.Detonate();
        }

        // 4. Destroy this explosive after physics is applied
        Destroy(gameObject, 0.1f);
    }

    // Visualize the blast radius in the Scene view (editor only)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}