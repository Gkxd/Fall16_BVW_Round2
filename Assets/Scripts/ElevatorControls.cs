using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;

using UsefulThings;

public class ElevatorControls : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    public Transform cabinetDoor;

    public float elevatorDoorSpeed;
    public float elevatorFloorInterval;

    public LightControl lightControl;

    public TextMesh floorDisplay;

    public GameObject floor1; // Hallway covered in rubble
    public GameObject floor2; // Hallway with monster running at you
    public GameObject floor3; // Hallway with jumpscare/death

    public CameraFade cameraFade;

    public Renderer[] hallLights;

    private Vector3 leftDoorClosedPosition;
    private Vector3 leftDoorOpenPosition;
    private Vector3 rightDoorClosedPosition;
    private Vector3 rightDoorOpenPosition;

    public float doorPosition { get; private set; }
    public float outerDoorPosition { get; private set; }
    public int floorPosition { get; private set; }

    private Coroutine moveDoorsRoutine;
    private Coroutine moveFloorsRoutine;

    private int floorsVisited;
    private bool canCloseDoors;

    void Start()
    {
        leftDoorClosedPosition = leftDoor.localPosition;
        leftDoorOpenPosition = leftDoorClosedPosition + Vector3.left * 0.75f;
        rightDoorClosedPosition = rightDoor.localPosition;
        rightDoorOpenPosition = rightDoorClosedPosition + Vector3.right * 0.75f;

        floorPosition = 3;
        floorsVisited = 0;
        floorDisplay.text = "  B" + floorPosition;
    }

    public void MoveDoors(bool open) // true = open, false = close
    {
        if (moveDoorsRoutine == null)
        {
            if (moveFloorsRoutine == null || canCloseDoors)
            {
                if (floorsVisited == 0)
                {
                    moveDoorsRoutine = StartCoroutine(MoveDoorsRoutine(doorPosition, open ? 1 : 0, waitTime: 3));
                }
                else
                {
                    moveDoorsRoutine = StartCoroutine(MoveDoorsRoutine(doorPosition, doorPosition + (open ? 0.1f : -0.1f), waitTime: 1));
                }
            }
        }
    }

    private IEnumerator MoveDoorsRoutine(float start, float end, float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);

        if (floorsVisited == 0)
        {
            SfxManager.PlaySfx(start < end ? 3 : 2);
        }
        else
        {
            if (start < end)
            {
                SfxManager.PlaySfx(3);
            }
            else
            {
                // Play broken door sfx instead
            }
        }

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
            if (floorsVisited == 0)
            {
                yield return StartCoroutine(MoveDoorsRoutine(doorPosition, 0, waitTime: 3));
            }
            else
            {
                yield return StartCoroutine(MoveDoorsRoutine(doorPosition, doorPosition - 0.1f, waitTime: 1));
                if (doorPosition > 0)
                {
                    moveFloorsRoutine = null;
                    yield break;
                }
            }
        }

        yield return new WaitForSeconds(1);

        if (floorsVisited > 0) // Disable lights on later floors
        {
            foreach (Renderer light in hallLights)
            {
                FlickerLights flicker = light.GetComponent<FlickerLights>();
                if (flicker)
                {
                    flicker.enabled = false;
                }

                DynamicGI.SetEmissive(light, Color.black);
                light.material.SetColor("_EmissionColor", Color.black);
            }
        }

        SfxManager.PlayLoop(1, 0.1f);

        int floorProgress = 0;

        string directionArrow = (start < end) ? "v " : "^ ";
        floorDisplay.text = directionArrow + (floorPosition == 0 ? "1F" : "B" + floorPosition);

        while ((start < end) ? floorPosition < end : floorPosition > end)
        {
            if (floorsVisited == 0 && floorProgress == 2) // Elevator crash cutscene
            {
                yield return new WaitForSeconds(elevatorFloorInterval * 0.5f);

                SfxManager.StopLoop(0);
                SfxManager.StopLoop(1);

                SfxManager.PlaySfx(4);
                CameraShake.ShakeCamera(0.3f, 5); // Shake Camera for 5 seconds

                for (int i = 0; i < 5; i++) // Lights flash for a bit
                {
                    lightControl.AllLightsOff();
                    floorDisplay.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.15f);

                    lightControl.AllLightsOn();
                    floorDisplay.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.1f);
                }

                // Lights turn off
                lightControl.AllLightsOff();
                floorDisplay.gameObject.SetActive(false);
                yield return new WaitForSeconds(5);

                SfxManager.PlaySfx(5);
                yield return new WaitForSeconds(1);

                // Lights back on
                lightControl.AllLightsOn();
                floorDisplay.gameObject.SetActive(true);
                yield return new WaitForSeconds(2);

                SfxManager.PlayLoop(0);
                SfxManager.PlayLoop(1, 0.1f);

                yield return new WaitForSeconds(elevatorFloorInterval * 0.5f);
            }
            else
            {
                yield return new WaitForSeconds(elevatorFloorInterval * 0.5f);
            }

            floorProgress++;

            SfxManager.PlaySfx(1);

            floorPosition += (start < end) ? 1 : -1;

            floorDisplay.text = directionArrow + "B" + floorPosition;
        }

        floorDisplay.text = floorPosition == 0 ? "  1F" : "  B" + floorPosition;

        SfxManager.StopLoop(1);

        yield return new WaitForSeconds(1);

        // Remove floor 1 props
        if (floorsVisited == 1)
        {
            floor1.SetActive(false);
        }

        // Open doors after you reach the floor
        yield return StartCoroutine(MoveDoorsRoutine(0, 1));

        if (floorsVisited == 1) //Floor#2 Cut Scene
        {
            floor2.SetActive(true);
            SfxManager.PlaySfx(9);

            yield return new WaitForSeconds(2);
            SfxManager.PlaySfx(10);

            yield return new WaitForSeconds(2);
            canCloseDoors = true;
            yield return new WaitForSeconds(18);
            canCloseDoors = false;

            if (doorPosition > 0) // If you don't close door fast enough, you die
            {
                cameraFade.CutToBlack();
            }
            else // Play banging effect/cutscene
            {
                floor2.SetActive(false);

                SfxManager.PlaySfx(11);
                CameraShake.ShakeCamera(0.3f, 2);

                yield return new WaitForSeconds(2);
                SfxManager.PlaySfx(12);
                CameraShake.ShakeCamera(0.3f, 2);

                yield return new WaitForSeconds(1);
                SfxManager.PlaySfx(13);
            }
        }
        else if (floorsVisited == 2) //Floor 3 Cutscene
        {
            Debug.Log("You Reached Floor 8");

            yield return new WaitForSeconds(2);
            SfxManager.PlaySfx(9);
        }

        floorsVisited++;
        moveFloorsRoutine = null;
    }

    /*
    IEnumerator OpenCabinetDoor()
    {
        floor8Button.SetActive(true);
        Transform targetAngle = cabinetDoor.Find("TargetAngle");
        while (true)
        {
            cabinetDoor.forward = Vector3.Slerp(cabinetDoor.forward, targetAngle.forward, 0.1f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(OpenCabinetDoor());
        }
    }
    */
}
