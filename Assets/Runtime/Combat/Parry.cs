using UnityEngine;

public class Parry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Parry")]
    [SerializeField] private float parryRadius = 2.0f;
    [SerializeField] private float parryWindow = 0.15f;
    [SerializeField] private float parryCooldown = 0.2f;

    [Header("Layers")]
    [SerializeField] private LayerMask projectileLayers;

    private float _cooldownTimer;

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    public void TryParry()
    {
        if (_cooldownTimer > 0f)
            return;

        _cooldownTimer = parryCooldown;

        EnemyProjectile projectile = FindProjectileToParry();

        if (projectile == null)
            return;

        Vector3 direction = GetAimDirection();

        projectile.Parry(direction);
    }

    private EnemyProjectile FindProjectileToParry()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            parryRadius,
            projectileLayers,
            QueryTriggerInteraction.Collide
        );

        EnemyProjectile closestProjectile = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            EnemyProjectile projectile =
                hit.GetComponentInParent<EnemyProjectile>();

            if (projectile == null)
                continue;

            if (!projectile.CanBeParried)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    projectile.transform.position
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestProjectile = projectile;
            }
        }

        return closestProjectile;
    }

    private Vector3 GetAimDirection()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        return ray.direction.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            parryRadius
        );
    }
}