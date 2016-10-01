using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using UsefulThings;
using System.Collections.Generic;

public class ElevatorControls : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    public Transform cabinetDoor;

    public float elevatorDoorSpeed;
    public float elevatorFloorInterval;

    public LightControl lightControl;

    public TextMesh floorDisplay;

    public TextMesh passCodeDisplay;

    private bool waitForCodeCheck = false;

    public GameObject floor1; // Hallway covered in rubble
    public GameObject floor2; // Hallway with monster running at you
    public GameObject floor3; // Hallway with jumpscare/death

    public GameObject shepardTone;

    public CameraFade cameraFade;

    public Renderer[] hallLights;
    public ParticleSystem[] elevatorLightSparks;

    private Vector3 leftDoorClosedPosition;
    private Vector3 leftDoorOpenPosition;
    private Vector3 rightDoorClosedPosition;
    private Vector3 rightDoorOpenPosition;

    public float doorPosition { get; private set; }
    public float outerDoorPosition { get; private set; }
    public int floorPosition { get; private set; }

    public AnimationCurve doorPositionCurve;

    private Coroutine moveDoorsRoutine;
    private Coroutine moveFloorsRoutine;

    private bool cabinetOpen = false;
    private bool codeAccepted = false;

    private Color onColor;

    private int floorsVisited;
    private bool canCloseDoors;
    private bool canOpenDoors;

    private int numAttempts = 0;
    private List<int> attempts;

    private bool onRouteToSecret = false;

    private Vector3 floorDisplayPosition;


    void Start()
    {
        leftDoorClosedPosition = leftDoor.localPosition;
        leftDoorOpenPosition = leftDoorClosedPosition + Vector3.left * 0.75f;
        rightDoorClosedPosition = rightDoor.localPosition;
        rightDoorOpenPosition = rightDoorClosedPosition + Vector3.right * 0.75f;

        floorDisplayPosition = floorDisplay.transform.position;

        attempts = new List<int>();

        for (int i= 0;i< elevatorLightSparks.Length; i++)
        {
            elevatorLightSparks[i].Stop();
        }


        floorPosition = 3;
        floorsVisited = 0;
        floorDisplay.text = "  B" + floorPosition;
        passCodeDisplay.text = "";
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
                    if (canCloseDoors && !open && doorPosition - 0.1f < 0.5f)
                    {
                        moveDoorsRoutine = StartCoroutine(MoveDoorsRoutine(doorPosition, 0.5f, waitTime: 1));
                        canCloseDoors = false;
                    }
                    else
                    {
                        moveDoorsRoutine = StartCoroutine(MoveDoorsRoutine(doorPosition, doorPosition + (open ? 0.1f : -0.1f), waitTime: 1));
                    }
                }
            }
        }
    }

    public void EnterCode(int codeValue)
    {
        if (cabinetOpen)
        {
            if (waitForCodeCheck == false)
            {
                SfxManager.PlaySfx(16);

                if (passCodeDisplay.text == "---")
                {
                    passCodeDisplay.text = codeValue.ToString();
                }
                else
                {
                    passCodeDisplay.text += codeValue;
                }

                if (passCodeDisplay.text.Length == 3)
                {
                    waitForCodeCheck = true;
                    if (numAttempts == 9)
                    {
                        //Play right sound and move elevator
                        numAttempts = 0;
                    }
                    else
                    {
                        StartCoroutine(checkCode(int.Parse(passCodeDisplay.text)));
                    }
                }
            }
        }
    }

    private IEnumerator checkCode(int codeValue)
    {
        yield return new WaitForSeconds(2f);
        
        string compareStr = "11";
        string firstTwoDigits = codeValue.ToString().Substring(0, 2);

        Debug.Log(string.Compare(firstTwoDigits, compareStr));

        if (numAttempts >= 5 && !attempts.Contains(codeValue) && string.Compare(firstTwoDigits,compareStr)==0)
        {
            //Right
            codeAccepted = true;
            passCodeDisplay.text = "";
            SfxManager.PlaySfx(18);
            yield return new WaitForSeconds(1f);
            MoveFloor(5);
        }
        else
        {
            //Wrong
            //check for unique attempts
            Debug.Log(codeValue);
            Debug.Log(attempts.Contains(codeValue));
            if (!attempts.Contains(codeValue))
            {
                numAttempts++;
                attempts.Add(codeValue);
            }
            passCodeDisplay.text = "RETRY";
            SfxManager.PlaySfx(17);
            yield return new WaitForSeconds(1f);
            passCodeDisplay.text = "---";
            waitForCodeCheck = false;

        }
    }

    private IEnumerator MoveDoorsRoutine(float start, float end, float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);

        bool opening = start < end;

        if (floorsVisited == 0)
        {
            SfxManager.PlaySfx(opening ? 3 : 2);
        }
        else
        {
            if (opening)
            {
                SfxManager.PlaySfx(3);
            }
            else
            {
                // Play broken door sfx instead
                SfxManager.PlaySfx(Random.Range(34,36));
            }
        }

        while (opening ? doorPosition < end : doorPosition > end)
        {
            doorPosition += elevatorDoorSpeed * ((start < end) ? Time.deltaTime : -Time.deltaTime);
            if (opening && !canOpenDoors)
            {
                leftDoor.localPosition = Vector3.Lerp(leftDoorClosedPosition, leftDoorOpenPosition, doorPositionCurve.Evaluate(doorPosition));
                rightDoor.localPosition = Vector3.Lerp(rightDoorClosedPosition, rightDoorOpenPosition, doorPositionCurve.Evaluate(doorPosition));
            }
            else
            {
                leftDoor.localPosition = Vector3.Lerp(leftDoorClosedPosition, leftDoorOpenPosition, doorPosition);
                rightDoor.localPosition = Vector3.Lerp(rightDoorClosedPosition, rightDoorOpenPosition, doorPosition);
            }
            yield return null;
        }

        doorPosition = opening ? Mathf.Min(doorPosition, end, 1) : Mathf.Max(doorPosition, end, 0);

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

        SfxManager.PlayLoop(1);

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

                SfxManager.PlaySfx(19); // Evacuation announcement
                yield return new WaitForSeconds(20);

                SfxManager.PlaySfx(5); // Lights turn on sound
                yield return new WaitForSeconds(1);

                // Lights back on
                lightControl.AllLightsOn();
                floorDisplay.gameObject.SetActive(true);
                yield return new WaitForSeconds(2);

                //shepardTone.SetActive(true);
                SfxManager.PlayLoop(0);
                SfxManager.PlayLoop(1, 0.1f);
                SfxManager.PlayLoop(3);

                yield return new WaitForSeconds(elevatorFloorInterval * 0.5f);
            }
            else if (floorPosition == 4 && end == 5) // Last cutscene part 1
            {
                onRouteToSecret = true;
                StartCoroutine(AnimateFloorDisplay());

                // 30 seconds
                yield return new WaitForSeconds(5);
                SfxManager.PlaySfx(27);
                SfxManager.PlaySfx(21);
                SfxManager.PlaySfx(22);
                CameraShake.ShakeCamera(0.3f, 3);
                lightControl.CeilingLightOff(0);
                ElevatorLightBurst(0);
                
                yield return new WaitForSeconds(5);
                SfxManager.PlaySfx(29);
                SfxManager.PlaySfx(25);
                SfxManager.PlaySfx(26);
                CameraShake.ShakeCamera(0.3f, 3);
                lightControl.CeilingLightOff(1);
                ElevatorLightBurst(1);

                yield return new WaitForSeconds(5);
                SfxManager.PlaySfx(30);
                SfxManager.PlaySfx(22);
                SfxManager.PlaySfx(25);
                CameraShake.ShakeCamera(0.3f, 3);
                lightControl.CeilingLightOff(2);
                ElevatorLightBurst(2);

                yield return new WaitForSeconds(5);
                SfxManager.PlaySfx(30);
                SfxManager.PlaySfx(22);
                SfxManager.PlaySfx(25);
                CameraShake.ShakeCamera(0.3f, 4);
                lightControl.CeilingLightOff(3);
                ElevatorLightBurst(3);

                yield return new WaitForSeconds(5);
                SfxManager.PlaySfx(29);
                SfxManager.PlaySfx(21);
                SfxManager.PlaySfx(26);
                CameraShake.ShakeCamera(0.3f, 4);
                lightControl.AllLightsOff();

                for (int i = 0; i < elevatorLightSparks.Length; i++)
                {
                    elevatorLightSparks[i].Play();
                }


                yield return new WaitForSeconds(5);
                onRouteToSecret = false;
                CameraShake.ShakeCamera(0.3f, 4);
                SfxManager.PlaySfx(31);
                SfxManager.PlaySfx(21);
                SfxManager.PlaySfx(26);

            }
            else
            {
                yield return new WaitForSeconds(elevatorFloorInterval * 0.5f);
                SfxManager.StopLoop(4);

            }

            floorProgress++;

            SfxManager.PlaySfx(1);

            floorPosition += (start < end) ? 1 : -1;

            floorDisplay.text = directionArrow + "B" + floorPosition;
            
        }

        floorDisplay.text = floorPosition == 0 ? "  1F" : "  B" + floorPosition;

        if (floorPosition == 5)
            floorDisplay.text = "ER1";

        if (floorPosition == 0)
            SfxManager.PlayLoop(4);

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

            Coroutine flickerLightsRoutine = StartCoroutine(FlickerElevatorLights());

            yield return new WaitForSeconds(8);
            canCloseDoors = true;

            yield return new WaitForSeconds(10);
            canCloseDoors = false;

            // Play banging effect/cutscene
            yield return StartCoroutine(MoveDoorsRoutine(doorPosition, 0));

            StopCoroutine(flickerLightsRoutine);

            lightControl.AllLightsOn();
            floor2.SetActive(false);

            SfxManager.PlaySfx(11);
            CameraShake.ShakeCamera(0.3f, 2);
            SfxManager.PlaySfx(25);

            yield return new WaitForSeconds(2);
            SfxManager.PlaySfx(12);
            CameraShake.ShakeCamera(0.3f, 2);
            SfxManager.PlaySfx(23);

            yield return new WaitForSeconds(1);
            SfxManager.PlaySfx(13);
            StartCoroutine(OpenCabinetDoor());
            passCodeDisplay.text = "---";
            InvokeRepeating("MonsterHit", 45.0f, 7.0f);

            /*
            if (doorPosition > 0.5f) // If you don't close door fast enough, you die
            {
                yield return new WaitForSeconds(2);
                cameraFade.CutToBlack();
            }
            else // Play banging effect/cutscene
            {
                yield return StartCoroutine(MoveDoorsRoutine(doorPosition, 0));

                StopCoroutine(flickerLightsRoutine);

                lightControl.AllLightsOn();
                floor2.SetActive(false);

                SfxManager.PlaySfx(11);
                CameraShake.ShakeCamera(0.3f, 2);

                yield return new WaitForSeconds(2);
                SfxManager.PlaySfx(12);
                CameraShake.ShakeCamera(0.3f, 2);

                yield return new WaitForSeconds(1);
                SfxManager.PlaySfx(13);
                StartCoroutine(OpenCabinetDoor());
                passCodeDisplay.text = "---";
                InvokeRepeating("MonsterHit", 45.0f, 7.0f);
            }
            */
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

    private IEnumerator FlickerElevatorLights()
    {
        while (true)
        {
            for (int i = 0; i < Random.Range(1, 4); i++)
            {
                lightControl.AllLightsOff();
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                lightControl.AllLightsOn();
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
            yield return new WaitForSeconds(Random.Range(1, 2.5f));
        }
    }

    IEnumerator AnimateFloorDisplay()
    {
        while (onRouteToSecret) {

            floorDisplay.text = "vvv";
            floorDisplay.transform.position = floorDisplayPosition;

            yield return new WaitForSeconds(0.5f);
            floorDisplay.transform.position += Vector3.down * Time.deltaTime * 0.5f;

            yield return new WaitForSeconds(0.5f);
            floorDisplay.text = "";


        }
    }

    void MonsterHit()
    {
        if (codeAccepted==false)
        {
            SfxManager.PlaySfx(11);
            CameraShake.ShakeCamera(0.3f, 2);
            SfxManager.PlaySfx(Random.Range(21, 26));
        }
    }

    IEnumerator OpenCabinetDoor()
    {
        Transform targetAngle = cabinetDoor.Find("TargetAngle");

        //Play panel open sound
        SfxManager.PlaySfx(15);

        cabinetDoor.forward = Vector3.Slerp(cabinetDoor.forward, targetAngle.forward, 5f);
        cabinetOpen = true;

        yield return null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(OpenCabinetDoor());
        }
    }

    private void ElevatorLightBurst(int i)
    {
        elevatorLightSparks[i].Emit(Random.Range(100, 200));
    }

}
