using System;
using System.Collections;
using UnityEngine;

public class WaitFinished : CustomYieldInstruction
{
    public bool IsDone { get; private set; }
    public Action onUpdate;
    public Action onFinished;

    public override bool keepWaiting 
    {
        get
        {
            if (onUpdate != null)
                onUpdate();
            return !IsDone;
        }
    }

    public override void Reset()
    {
        IsDone = false;
    }

    public virtual void Finished()
    {
        IsDone = true;
        onFinished?.Invoke();
    }
}
