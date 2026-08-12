using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem;

public class Recall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KinematicCharacterMotor motor;

    [Header("Recall")]
    [SerializeField] private float recallDuration = 3f;
    [SerializeField] private float snapshotInterval = 0.05f;

    [Header("Input")]
    [SerializeField] private Key recallKey = Key.E;

    private struct Snapshot
    {
        public float time;
        public KinematicCharacterMotorState motorState;
    }

    private readonly List<Snapshot> _snapshots =
        new List<Snapshot>();

    private float _snapshotTimer;

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<KinematicCharacterMotor>();
    }

    private void Update()
    {
        RecordSnapshot();

        if (Keyboard.current != null &&
            Keyboard.current[recallKey].wasPressedThisFrame)
        {
            TryRecall();
        }
    }

    private void RecordSnapshot()
    {
        _snapshotTimer += Time.deltaTime;

        if (_snapshotTimer < snapshotInterval)
            return;

        _snapshotTimer = 0f;

        _snapshots.Add(new Snapshot
        {
            time = Time.time,
            motorState = motor.GetState()
        });

        RemoveOldSnapshots();
    }

    private void RemoveOldSnapshots()
    {
        float oldestAllowedTime =
            Time.time - recallDuration;

        while (
            _snapshots.Count > 0 &&
            _snapshots[0].time < oldestAllowedTime
        )
        {
            _snapshots.RemoveAt(0);
        }
    }

    private void TryRecall()
    {
        if (_snapshots.Count == 0)
            return;

        Snapshot target = FindRecallSnapshot();

        motor.ApplyState(
            target.motorState,
            true
        );

        // Remove snapshots newer than the restored point.
        RemoveSnapshotsAfter(target.time);
    }

    private Snapshot FindRecallSnapshot()
    {
        float targetTime =
            Time.time - recallDuration;

        Snapshot closest = _snapshots[0];

        float closestDifference =
            Mathf.Abs(closest.time - targetTime);

        for (int i = 1; i < _snapshots.Count; i++)
        {
            float difference =
                Mathf.Abs(
                    _snapshots[i].time - targetTime
                );

            if (difference < closestDifference)
            {
                closest = _snapshots[i];
                closestDifference = difference;
            }
        }

        return closest;
    }

    private void RemoveSnapshotsAfter(float time)
    {
        for (int i = _snapshots.Count - 1; i >= 0; i--)
        {
            if (_snapshots[i].time > time)
                _snapshots.RemoveAt(i);
        }
    }

    public float GetRemainingHistory()
    {
        if (_snapshots.Count == 0)
            return 0f;

        return Time.time - _snapshots[0].time;
    }
}
