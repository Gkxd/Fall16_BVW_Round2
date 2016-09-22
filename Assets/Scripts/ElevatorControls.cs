using UnityEngine;
using System.Collections;

public class ElevatorControls : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;

    public float elevatorDoorSpeed;

    public GameObject floor1; // Hallway covered in rubble
    public GameObject floor2; // Hallway with monster running at you
    public GameObject floor3; // Hallway with jumpscare/death
    
    private Vector3 leftDoorClosedPosition;
    private Vector3 leftDoorOpenPosition;
    private Vector3 rightDoorClosedPosition;
    private Vector3 rightDoorOpenPosition;

    public float doorPosition { get; private set; }

    private Coroutine moveDoorsRoutine;

    void Start()
    {
        leftDoorClosedPosition = leftDoor.localPosition;
        leftDoorOpenPosition = leftDoorClosedPosition + Vector3.left * 2.045f;
        rightDoorClosedPosition = rightDoor.localPosition;
        rightDoorOpenPosition = rightDoorClosedPosition + Vector3.right * 2.045f;
    }

    void Update()
    {
        #region DEBUG

        if (Input.GetKeyDown(KeyCode.Q))
        {
            MoveDoors(1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveDoors(0);
        }

        #endregion
    }

    private void MoveDoors(float percentOpen) // 0 is closed, 1 is open
    {
        if (moveDoorsRoutine == null)
        {
            moveDoorsRoutine = StartCoroutine(MoveDoorsRoutine(doorPosition, percentOpen));
        }
    }

    private IEnumerator MoveDoorsRoutine(float start, float end)
    {
        while ((start < end) ? doorPosition < end : doorPosition > end)
        {
            doorPosition += elevatorDoorSpeed * ((start < end) ? Time.deltaTime : -Time.deltaTime);
            leftDoor.localPosition = Vector3.Lerp(leftDoorClosedPosition, leftDoorOpenPosition, doorPosition);
            rightDoor.localPosition = Vector3.Lerp(rightDoorClosedPosition, rightDoorOpenPosition, doorPosition);
            yield return null;
        }
        doorPosition = (start < end) ? Mathf.Min(doorPosition, end) : Mathf.Max(doorPosition, end);
        moveDoorsRoutine = null;
    }
}
