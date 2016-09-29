using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;

using UsefulThings;
using System.Collections.Generic;
using System;

public class ElevatorControls : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    public Transform leftDoorOuter;
    public Transform rightDoorOuter;
    public Transform cabinetDoor;

    public float elevatorDoorSpeed;
    public float elevatorFloorInterval;

    public ColorCorrectionCurves dimLights;

    public TextMesh floorDisplay;

    public TextMesh passCodeDisplay;

    private bool waitForCodeCheck = false;

    public GameObject floor1; // Hallway covered in rubble
    public GameObject floor2; // Hallway with monster running at you
    public GameObject floor3; // Hallway with jumpscare/death
    public GameObject floor8Button;
    public GameObject endMonster;

    private Vector3 leftDoorClosedPosition;
    private Vector3 leftDoorOpenPosition;
    private Vector3 rightDoorClosedPosition;
    private Vector3 rightDoorOpenPosition;

    private Vector3 leftDoorOuterClosedPosition;
    private Vector3 leftDoorOuterOpenPosition;
    private Vector3 rightDoorOuterClosedPosition;
    private Vector3 rightDoorOuterOpenPosition;

    public float doorPosition { get; private set; }
    public float outerDoorPosition { get; private set; }
    public int floorPosition { get; private set; }

    private Coroutine moveDoorsRoutine;
    private Coroutine moveFloorsRoutine;

    private bool cabinetOpen = false;
    private bool codeAccepted = false;

    private Color onColor;

    private int floorsVisited;

    private bool moveMonster = false;
    private bool monsterStopped = false;

    private GameObject[] hallLights;

    public GameObject jumpScare;

    public GameObject monster;
    private GameObject monsterObj;

    public Curve monsterBobbingMotion;

    private int numAttempts = 0;
    private List<int> attempts;
    

    void Start()
    {
        leftDoorClosedPosition = leftDoor.localPosition;
        leftDoorOpenPosition = leftDoorClosedPosition + Vector3.left * 0.75f;
        rightDoorClosedPosition = rightDoor.localPosition;
        rightDoorOpenPosition = rightDoorClosedPosition + Vector3.right * 0.75f;

        attempts = new List<int>();

        leftDoorOuterClosedPosition = leftDoorOuter.localPosition;
        leftDoorOuterOpenPosition = leftDoorOuterClosedPosition + Vector3.left * 0.75f;
        rightDoorOuterClosedPosition = rightDoorOuter.localPosition;
        rightDoorOuterOpenPosition = rightDoorOuterClosedPosition + Vector3.right * 0.75f;

        floorPosition = 6;
        floorsVisited = 0;

        hallLights = GameObject.FindGameObjectsWithTag("HallLight");
        onColor = GameObject.FindWithTag("HallLight").GetComponent<Renderer>().material.GetColor("_EmissionColor");

        floorDisplay.text = "  " + floorPosition + "F";
    }

    public void MoveDoors(float percentOpen) // 0 is closed, 1 is open
    {
        if (moveDoorsRoutine == null && moveFloorsRoutine == null)
        {
            moveDoorsRoutine = StartCoroutine(MoveDoorsRoutine(doorPosition, percentOpen, waitTime: 3));
            StartCoroutine(MoveOuterDoorsRoutine(outerDoorPosition, percentOpen, waitTime: 3));
        }
    }

    public void EnterCode(int codeValue)
    {
        if (cabinetOpen)
        {
            if (waitForCodeCheck == false)
            {
                SfxManager.PlaySfx(16);

                if (passCodeDisplay.text.CompareTo("---") == 0)
                    passCodeDisplay.text = codeValue.ToString();
                else
                    passCodeDisplay.text += codeValue;

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
                        StartCoroutine(checkCode(Int32.Parse(passCodeDisplay.text)));
                    }
                }
            }   
        }
    }

    private IEnumerator checkCode(int codeValue)
    {
        yield return new WaitForSeconds(2f);

        //attempts.Add(codeValue);

        if (numAttempts == 5)
        {
            //Right
            codeAccepted = true;
            passCodeDisplay.text = "";
            SfxManager.PlaySfx(18);
            yield return new WaitForSeconds(1f);
            MoveFloor(-5);
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

    public bool GetCabinetOpen()
    {
        return cabinetOpen;
    }

    private IEnumerator MoveDoorsRoutine(float start, float end, float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);

        SfxManager.PlaySfx(start < end ? 2 : 3);

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

    private IEnumerator MoveOuterDoorsRoutine(float start, float end, float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);

        while ((start < end) ? doorPosition < end : doorPosition > end)
        {
            outerDoorPosition += elevatorDoorSpeed * ((start < end) ? Time.deltaTime : -Time.deltaTime);
            leftDoorOuter.localPosition = Vector3.Lerp(leftDoorOuterClosedPosition, leftDoorOuterOpenPosition, outerDoorPosition);
            rightDoorOuter.localPosition = Vector3.Lerp(rightDoorOuterClosedPosition, rightDoorOuterOpenPosition, outerDoorPosition);
            yield return null;
        }

        outerDoorPosition = (start < end) ? Mathf.Min(outerDoorPosition, end) : Mathf.Max(outerDoorPosition, end);
    }

    public void MoveFloor(int floor)
    {
        Debug.Log(moveDoorsRoutine + " " + moveFloorsRoutine + " " + floorPosition);
        if (moveDoorsRoutine == null && moveFloorsRoutine == null && floor != floorPosition)
        {
            moveFloorsRoutine = StartCoroutine(MoveFloorsRoutine(floorPosition, floor));
        }
    }

    private IEnumerator MoveFloorsRoutine(int start, int end)
    {
        if (doorPosition > 0)
        {
            yield return StartCoroutine(MoveDoorsRoutine(doorPosition, 0, waitTime: 5));
        }

        if (floorsVisited > 0)
        {
            for (int i = 0; i < hallLights.Length; i++)
            {
                Renderer renderer = hallLights[i].GetComponent<Renderer>();

                FlickerLights flicker = renderer.GetComponent<FlickerLights>();
                if (flicker)
                {
                    flicker.enabled = false;
                }

                DynamicGI.SetEmissive(renderer, Color.black);
                renderer.material.SetColor("_EmissionColor", Color.black);
            }
        }

        SfxManager.PlayLoop(1, 0.1f);

        int floorProgress = 0;

        string directionArrow = (start < end) ? "^ " : "v ";
        floorDisplay.text = directionArrow + floorPosition + "F";

        while ((start < end) ? floorPosition < end : floorPosition > end)
        {
            if (floorsVisited == 0 && floorProgress == 2) // Intro cutscene
            {
                yield return new WaitForSeconds(elevatorFloorInterval * 0.5f);

                SfxManager.StopLoop(0);
                SfxManager.StopLoop(1);

                SfxManager.PlaySfx(4);
                CameraShake.ShakeCamera(0.3f, 5); // Shake Camera for 5 seconds

                for (int i = 0; i < 5; i++) // Lights flash for a bit
                {
                    dimLights.enabled = true;
                    floorDisplay.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.15f);

                    dimLights.enabled = false;
                    floorDisplay.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.1f);
                }

                // Lights turn off
                dimLights.enabled = true;
                floorDisplay.gameObject.SetActive(false);
                yield return new WaitForSeconds(3);

                SfxManager.PlaySfx(6); // Screaming
                yield return new WaitForSeconds(8);

                SfxManager.PlaySfx(5);
                yield return new WaitForSeconds(1);

                // Lights back on
                dimLights.enabled = false;
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

            floorDisplay.text = directionArrow + floorPosition + "F";
        }

        floorDisplay.text = "  " + floorPosition + "F";

        SfxManager.StopLoop(1);

        yield return new WaitForSeconds(1);

        // Remove floor 1 props
        if (floorsVisited == 1)
        {
            floor1.SetActive(false);
        }

        if (floorsVisited == 0)
        {
            StartCoroutine(MoveOuterDoorsRoutine(0, 0.35f));
            SfxManager.PlaySfx(7, 0.5f);
        }
        else
        {
            StartCoroutine(MoveOuterDoorsRoutine(0, 1));
        }
        yield return StartCoroutine(MoveDoorsRoutine(0, 1));

        if (floorsVisited == 1) //Floor#2 Cut Scene
        {
            monsterObj = Instantiate(monster);
            SfxManager.PlaySfx(9);

            yield return new WaitForSeconds(2);
            SfxManager.PlaySfx(10);
            
            StartCoroutine(Rise());
            yield return new WaitForSeconds(16.5f);

            SfxManager.PlaySfx(start < end ? 2 : 3);

            StartCoroutine(PlayBangEffect());

            yield return StartCoroutine(MoveOuterDoorsRoutine(1, 0));
            yield return StartCoroutine(MoveDoorsRoutine(1, 0));
        }
        else if (floorsVisited == 2) //Floor 3 Cutscene
        {
            Debug.Log("You Reached Floor 8");
            jumpScare.SetActive(true);

            yield return new WaitForSeconds(2);
            endMonster.SetActive(true);
            SfxManager.PlaySfx(9);
        }

        floorsVisited++;
        moveFloorsRoutine = null;

        Debug.Log("Hello " + floorsVisited);
    }

    IEnumerator Rise()
    {
        float startTime = Time.time;
        while (true)
        {
            if (monsterObj)
            {
                Vector3 position = monsterObj.transform.position;
                position -= Vector3.forward * Time.deltaTime * 1.62f;
                position.y = monsterBobbingMotion.Evaluate(Time.time - startTime);
                monsterObj.transform.position = position;
                yield return null;
            }
            else
            {
                break;
            }
        }
    }

    IEnumerator PlayBangEffect()
    {
        yield return new WaitForSeconds(3);
        SfxManager.PlaySfx(11);
        CameraShake.ShakeCamera(0.3f, 2);
        moveMonster = false;
        Destroy(monsterObj, 1f);

        yield return new WaitForSeconds(2);
        SfxManager.PlaySfx(12);
        CameraShake.ShakeCamera(0.3f, 2);

        yield return new WaitForSeconds(1);
        SfxManager.PlaySfx(13);
        StartCoroutine(OpenCabinetDoor());

        moveFloorsRoutine = null;
        moveDoorsRoutine = null;
        doorPosition = 0;

        InvokeRepeating("MonsterHit", 10.0f, 7.0f);
    }

    void MonsterHit()
    {
        if (!codeAccepted)
        {
            SfxManager.PlaySfx(11);
            CameraShake.ShakeCamera(0.3f, 2);
        }
    }
    
    IEnumerator OpenCabinetDoor()
    {
        floor8Button.SetActive(true);
        Transform targetAngle = cabinetDoor.Find("TargetAngle");

        //Play panel open sound
        SfxManager.PlaySfx(15);

        while (!cabinetOpen)
        {
            cabinetDoor.forward = Vector3.Slerp(cabinetDoor.forward,targetAngle.forward, 5f);
            yield return new WaitForSeconds(3f);
            cabinetOpen = true;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(OpenCabinetDoor());
        }
    }
    
}
