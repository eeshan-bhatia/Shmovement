using UnityEngine;
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 8f;

    [Header("Damage")]
    [SerializeField] private float damage = 20f;

    [Header("Reflection")]
    [SerializeField] private float reflectedDamage = 40f;
    [SerializeField] private float reflectedRange = 200f;
    [SerializeField] private LayerMask reflectedHitLayers;

    private Vector3 _direction;
    private bool _reflected;
    private float _lifeTimer;

    public bool CanBeParried => !_reflected;

    public void Initialize(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    private void Update()
    {
        _lifeTimer += Time.deltaTime;

        if (_lifeTimer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (_reflected)
            return;

        transform.position +=
            _direction * speed * Time.deltaTime;
    }

    public void Parry(Vector3 direction)
    {
        if (_reflected)
            return;

        _reflected = true;

        FireReflectedShot(direction);

        Destroy(gameObject);
    }

    private void FireReflectedShot(Vector3 direction)
    {
        Vector3 origin = transform.position;

        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            reflectedRange,
            reflectedHitLayers,
            QueryTriggerInteraction.Ignore))
        {
            hit.collider.SendMessage(
                "TakeDamage",
                reflectedDamage,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    public float GetDamage()
    {
        return damage;
    }
}