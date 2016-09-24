using UnityEngine;
using System.Collections;

using UsefulThings;

public class ElevatorControls : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    
    public float elevatorDoorSpeed;
    public float elevatorFloorInterval;
    public float elevatorEnableTime;

    public GameObject floor1; // Hallway covered in rubble
    public GameObject floor2; // Hallway with monster running at you
    public GameObject floor3; // Hallway with jumpscare/death
    
    private Vector3 leftDoorClosedPosition;
    private Vector3 leftDoorOpenPosition;
    private Vector3 rightDoorClosedPosition;
    private Vector3 rightDoorOpenPosition;
    
    public float doorPosition { get; private set; }
    public int floorPosition { get; private set; }

    private Coroutine moveDoorsRoutine;
    private Coroutine moveFloorsRoutine;

    void Start()
    {
        leftDoorClosedPosition = leftDoor.localPosition;
        leftDoorOpenPosition = leftDoorClosedPosition + Vector3.left * 2.045f;
        rightDoorClosedPosition = rightDoor.localPosition;
        rightDoorOpenPosition = rightDoorClosedPosition + Vector3.right * 2.045f;

        floorPosition = 4;
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

    public void MoveDoors(float percentOpen) // 0 is closed, 1 is open
    {
        if (moveDoorsRoutine == null && moveFloorsRoutine == null)
        {
            moveDoorsRoutine = StartCoroutine(MoveDoorsRoutine(doorPosition, percentOpen));
        }
    }

    private IEnumerator MoveDoorsRoutine(float start, float end)
    {
        yield return new WaitForSeconds(1);

        while ((start < end) ? doorPosition < end : doorPosition > end)
        {
            doorPosition += elevatorDoorSpeed * ((start < end) ? Time.deltaTime : -Time.deltaTime);
            leftDoor.localPosition = Vector3.Lerp(leftDoorClosedPosition, leftDoorOpenPosition, doorPosition);
            rightDoor.localPosition = Vector3.Lerp(rightDoorClosedPosition, rightDoorOpenPosition, doorPosition);
            yield return null;
        }
        doorPosition = (start < end) ? Mathf.Min(doorPosition, end) : Mathf.Max(doorPosition, end);

        // Wait a few seconds before closing re-enabling elevator doors
        yield return new WaitForSeconds(elevatorEnableTime);

        // Code to execute after the delay

        moveDoorsRoutine = null;
    }

    public void MoveFloor(int floor)
    {
        if (moveDoorsRoutine == null && moveFloorsRoutine == null && floor != floorPosition)
        {
            moveFloorsRoutine = StartCoroutine(MoveFloorsRoutine(floorPosition, floor));
        }
    }

    private IEnumerator MoveFloorsRoutine(int start, int end)
    {
        if (doorPosition > 0)
        {
            yield return StartCoroutine(MoveDoorsRoutine(doorPosition, 0));
        }

        while ((start < end) ? floorPosition < end : floorPosition > end)
        {
            yield return new WaitForSeconds(elevatorFloorInterval);

            SfxManager.PlaySfx(1);

            floorPosition += (start < end) ? 1 : -1;
        }

        yield return new WaitForSeconds(2 * elevatorFloorInterval);

        yield return StartCoroutine(MoveDoorsRoutine(0, 1));

        moveFloorsRoutine = null;
    }
}
