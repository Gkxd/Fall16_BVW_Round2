using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;

using UsefulThings;

public class ElevatorControls : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    
    public float elevatorDoorSpeed;
    public float elevatorFloorInterval;

    public ColorCorrectionCurves dimLights;

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

    private Color onColor;

    private int floorsVisited;

    private bool moveMonster = false;
    private bool monsterStopped = false;

    private GameObject[] hallLights;

    public GameObject monster;
    private GameObject monsterObj;

    public Curve monsterBobbingMotion;

    void Start()
    {
        leftDoorClosedPosition = leftDoor.localPosition;
        leftDoorOpenPosition = leftDoorClosedPosition + Vector3.left * 0.75f;
        rightDoorClosedPosition = rightDoor.localPosition;
        rightDoorOpenPosition = rightDoorClosedPosition + Vector3.right * 0.75f;

        floorPosition = 6;
        floorsVisited = 0;

        hallLights = GameObject.FindGameObjectsWithTag("HallLight");
        onColor = GameObject.FindWithTag("HallLight").GetComponent<Renderer>().material.GetColor("_EmissionColor");

    }

    public void MoveDoors(float percentOpen) // 0 is closed, 1 is open
    {
        if (moveDoorsRoutine == null && moveFloorsRoutine == null)
        {
            moveDoorsRoutine = StartCoroutine(MoveDoorsRoutine(doorPosition, percentOpen, waitTime: 3));
        }
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
            yield return StartCoroutine(MoveDoorsRoutine(doorPosition, 0, waitTime: 5));
        }

        SfxManager.PlayLoop(1, 0.1f);

        int floorProgress = 0;

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
                    yield return new WaitForSeconds(0.15f);
                    dimLights.enabled = false;
                    yield return new WaitForSeconds(0.1f);
                }

                // Lights turn off
                dimLights.enabled = true;
                yield return new WaitForSeconds(15);

                SfxManager.PlaySfx(5);
                yield return new WaitForSeconds(1);

                // Lights back on
                dimLights.enabled = false;
                yield return new WaitForSeconds(2);

                SfxManager.PlayLoop(0);
                SfxManager.PlayLoop(1, 0.1f);

                yield return new WaitForSeconds(elevatorFloorInterval * 0.5f);
            }
            if(floorsVisited == 1)
            {

                for(int i = 0; i < hallLights.Length; i++)
                {
                    Renderer renderer = hallLights[i].GetComponent<Renderer>();
                    DynamicGI.SetEmissive(renderer, Color.black);
                    renderer.material.SetColor("_EmissionColor", Color.black);
                }

                yield return new WaitForSeconds(elevatorFloorInterval * 0.5f);
            }
            else
            {

                for (int i = 0; i < hallLights.Length; i++)
                {
                    Renderer renderer = hallLights[i].GetComponent<Renderer>();
                    DynamicGI.SetEmissive(renderer, onColor);
                    renderer.material.SetColor("_EmissionColor", onColor);
                }

                yield return new WaitForSeconds(elevatorFloorInterval);
            }

            floorProgress++;

            SfxManager.PlaySfx(1);

            floorPosition += (start < end) ? 1 : -1;
        }

        SfxManager.StopLoop(1);

        yield return new WaitForSeconds(1);
        yield return StartCoroutine(MoveDoorsRoutine(0, 1));

        if (floorsVisited == 1) //Floor#2 Cut Scene
        {
            monsterObj = Instantiate(monster);
            SfxManager.PlaySfx(6);

            yield return new WaitForSeconds(2);
            SfxManager.PlaySfx(7);

            moveMonster = true;
            StartCoroutine(Rise());
            yield return new WaitForSeconds(20);

            SfxManager.PlaySfx(start < end ? 2 : 3);

            monsterStopped = true; 

            while ((doorPosition < 0) ? doorPosition < 0 : doorPosition > 0)
            {
                doorPosition += elevatorDoorSpeed * ((doorPosition < 0) ? Time.deltaTime : -Time.deltaTime);
                leftDoor.localPosition = Vector3.Lerp(leftDoorClosedPosition, leftDoorOpenPosition, doorPosition);
                rightDoor.localPosition = Vector3.Lerp(rightDoorClosedPosition, rightDoorOpenPosition, doorPosition);
                
                
                yield return null;
            }

            doorPosition = (doorPosition < 0) ? Mathf.Min(doorPosition, 0) : Mathf.Max(doorPosition, 0);

            
            //monsterDoorInterrupt = false;
            //Debug.Log(monsterDoorInterrupt);

            


            


        }

        floorsVisited++;
        moveFloorsRoutine = null;
    }

    void Update()
    {
        if (moveMonster)
        {
            if (monsterObj)
            {

                monsterObj.transform.Translate((Vector3.forward) * Time.deltaTime * 1.3f);
                //monsterObj.transform.Translate(Vector3.up * Time.deltaTime * 2f);
                //monsterObj.transform.Translate(Vector3.down * Time.deltaTime * 2f);
            }
        }
        
        if(monsterStopped==true)
        {
            StartCoroutine(PlayBangEffect());
            monsterStopped = false;
        }

    }

    IEnumerator Rise()
    {
        if (monsterObj)
        {
            while(true)
            {
                monsterObj.transform.position = Vector3.up * monsterBobbingMotion.Evaluate(Time.time);
                yield return null;
            }
        }
    }

    IEnumerator Fall()
    {
        if (monsterObj)
        {
            monsterObj.transform.Translate(Vector3.down * Time.deltaTime * 10f);
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Rise());
        }
    }

    IEnumerator PlayBangEffect()
    {
        yield return new WaitForSeconds(3);
        SfxManager.PlaySfx(8);
        CameraShake.ShakeCamera(0.3f, 2);
        moveMonster = false;
        Destroy(monsterObj, 1f);

        yield return new WaitForSeconds(2);
        SfxManager.PlaySfx(8);
        CameraShake.ShakeCamera(0.3f, 2);
    }
}
