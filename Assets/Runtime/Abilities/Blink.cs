using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem;

public class Blink : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTransform;

    [Header("Blink")]
    [SerializeField] private float blinkDistance = 7f;
    [SerializeField] private int maxCharges = 3;
    [SerializeField] private float rechargeTime = 3f;

    [Header("Collision")]
    [SerializeField] private float collisionPadding = 0.05f;

    [Header("Input")]
    [SerializeField] private Key blinkKey = Key.LeftShift;

    private int _charges;
    private float[] _rechargeTimers;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<KinematicCharacterMotor>();

        _charges = maxCharges;
        _rechargeTimers = new float[maxCharges];
    }

    private void Update()
    {
        UpdateRechargeTimers();

        if (Keyboard.current != null &&
            Keyboard.current[blinkKey].wasPressedThisFrame)
        {
            TryBlink();
        }
    }

    private void UpdateRechargeTimers()
    {
        for (int i = 0; i < maxCharges; i++)
        {
            if (_rechargeTimers[i] <= 0f)
                continue;

            _rechargeTimers[i] -= Time.deltaTime;

            if (_rechargeTimers[i] <= 0f)
            {
                _charges++;
                _rechargeTimers[i] = 0f;
            }
        }
    }

    private void TryBlink()
    {
        if (_charges <= 0)
            return;

        Vector3 direction = GetBlinkDirection();

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        Vector3 startPosition = motor.TransientPosition;

        float distance = GetSafeBlinkDistance(
            startPosition,
            direction,
            blinkDistance
        );

        if (distance <= 0.01f)
            return;

        Vector3 horizontalDestination =
            startPosition + direction * distance;

        if (!TryFindGroundAtDestination(
            horizontalDestination,
            out Vector3 destination))
        {
            return;
        }

        Quaternion rotation = motor.TransientRotation;

        motor.SetPositionAndRotation(
            destination,
            rotation,
            true
        );

        ConsumeCharge();
    }

    private Vector3 GetBlinkDirection()
    {
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                moveInput.y += 1f;

            if (Keyboard.current.sKey.isPressed)
                moveInput.y -= 1f;

            if (Keyboard.current.aKey.isPressed)
                moveInput.x -= 1f;

            if (Keyboard.current.dKey.isPressed)
                moveInput.x += 1f;
        }

        if (moveInput.sqrMagnitude > 0.001f)
        {
            moveInput.Normalize();

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward = Vector3.ProjectOnPlane(
                forward,
                motor.CharacterUp
            ).normalized;

            right = Vector3.ProjectOnPlane(
                right,
                motor.CharacterUp
            ).normalized;

            return (
                forward * moveInput.y +
                right * moveInput.x
            ).normalized;
        }

        // No WASD input:
        // Blink in the direction the player is looking.
        return Vector3.ProjectOnPlane(
            cameraTransform.forward,
            motor.CharacterUp
        ).normalized;
    }

    private float GetSafeBlinkDistance(
    Vector3 startPosition,
    Vector3 direction,
    float maxDistance)
    {
        CapsuleCollider capsule = motor.Capsule;

        Vector3 up = motor.CharacterUp;

        Vector3 center =
            startPosition +
            motor.TransientRotation * capsule.center;

        float radius = capsule.radius;

        float halfHeight =
            Mathf.Max(
                capsule.height * 0.5f - radius,
                0f
            );

        Vector3 bottom =
            center - up * halfHeight;

        Vector3 top =
            center + up * halfHeight;

        int layerMask = motor.CollidableLayers;

        if (Physics.CapsuleCast(
            bottom,
            top,
            radius,
            direction,
            out RaycastHit hit,
            maxDistance + collisionPadding,
            layerMask,
            QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(
                0f,
                hit.distance - collisionPadding
            );
        }

        return maxDistance;
    }

    private bool TryFindGroundAtDestination(
    Vector3 destination,
    out Vector3 groundedPosition)
    {
        Vector3 up = motor.CharacterUp;

        // Start well above the destination so we can find
        // an uphill/downhill floor beneath it.
        Vector3 castOrigin =
            destination + up * 2f;

        float castDistance = 5f;

        int layerMask = motor.CollidableLayers;

        if (Physics.Raycast(
            castOrigin,
            -up,
            out RaycastHit hit,
            castDistance,
            layerMask,
            QueryTriggerInteraction.Ignore))
        {
            // Make sure the surface is actually traversable.
            float slopeAngle =
                Vector3.Angle(hit.normal, up);

            if (slopeAngle > motor.MaxStableSlopeAngle)
            {
                groundedPosition = default;
                return false;
            }

            // The ray hits the floor at the point where the
            // capsule should stand. Move the character upward
            // by the capsule's half-height.
            float capsuleHalfHeight =
                motor.Capsule.height * 0.5f;

            groundedPosition =
                hit.point +
                up * capsuleHalfHeight;

            return true;
        }

        groundedPosition = default;
        return false;
    }

    private void ConsumeCharge()
    {
        _charges--;

        // Start recharging this charge.
        for (int i = 0; i < maxCharges; i++)
        {
            if (_rechargeTimers[i] <= 0f)
            {
                _rechargeTimers[i] = rechargeTime;
                break;
            }
        }
    }

    public int GetCharges()
    {
        return _charges;
    }

    public int GetMaxCharges()
    {
        return maxCharges;
    }
}

