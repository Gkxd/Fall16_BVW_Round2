using UnityEngine;
using System.Collections;
using System.Diagnostics;

public delegate void Delegate();
public delegate void DelegateWithOneParam(GameObject obj);
public delegate void DelegateWithTwoParam(GameObject obj1, GameObject obj2);

public class Timer
{

    private float duration;
    private float currentTime;
    private bool stopped;
    private bool repeat;
    Delegate callback;
    DelegateWithOneParam callback1;
    DelegateWithTwoParam callback2;
    GameObject obj1;
    GameObject obj2;

    //timer structure
    public Timer(float duration, Delegate _callback, bool repeat)
    {
        this.duration = duration;
        this.currentTime = duration;
        this.callback = _callback;
        this.repeat = repeat;
        this.callback1 = null;
    }

    public Timer(float duration, DelegateWithOneParam _callback1, GameObject obj, bool repeat)
    {
        this.duration = duration;
        this.currentTime = duration;
        this.callback1 = _callback1;
        this.repeat = repeat;
        this.obj1 = obj;
        this.callback = null;
    }

    public Timer(float duration, DelegateWithTwoParam _callback2, GameObject obj1, GameObject obj2, bool repeat)
    {
        this.duration = duration;
        this.currentTime = duration;
        this.callback2 = _callback2;
        this.repeat = repeat;
        this.obj1 = obj1;
        this.obj2 = obj2;
        this.callback = null;
    }

    public void RunTimer()
    {
        if (!stopped)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                if (this.callback != null)
                {
                    callback();
                }
                else if (this.callback1 != null)
                {
                    callback1(obj1);
                }
                else
                {
                    callback2(obj1, obj2);
                }

                if (repeat)
                {
                    currentTime = duration;
                }
                else
                {
                    stopped = true;
                }
            }
        }
    }

    public void Reset()
    {
        currentTime = duration;
    }

    public void Pause()
    {
        this.stopped = true;
    }

    public void Unpause()
    {
        this.stopped = false;
    }
}

