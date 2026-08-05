using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CharacterInput
{
    public Quaternion Rotation;

    public Vector2 Move;

    public bool Jump;
}
public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float gravity = -90f;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;

    public void Initialize()
    {
        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        _requestedMovement = input.Rotation * _requestedMovement;
        _requestedJump = _requestedJump || input.Jump;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
    {
        if (motor.GroundingStatus.IsStableOnGround)
        {
            var groundedMovement = motor.GetDirectionTangentToSurface
                (
                    direction: _requestedMovement,
                    surfaceNormal: motor.GroundingStatus.GroundNormal
                ) * _requestedMovement.magnitude;

            currentVelocity = groundedMovement * walkSpeed;
        }
        else
        {
            currentVelocity += motor.CharacterUp * gravity * deltaTime;
        }

        if (_requestedJump)
        {
            _requestedJump = false;

            motor.ForceUnground(time: 0f);

            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
    }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) 
    {
        var forward = Vector3.ProjectOnPlane
            (

            _requestedRotation * Vector3.forward,
            motor.CharacterUp
            );
        if (forward != Vector3.zero)
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void BeforeCharacterUpdate(float deltaTime) {}
    public void PostGroundingUpdate(float deltaTime) {}
    public void AfterCharacterUpdate(float deltaTime) {}

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {}

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {}

    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnDiscreteCollisionDetected(Collider hitCollider) {}

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    public Transform GetCameraTarget() => cameraTarget;
}
