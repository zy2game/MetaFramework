using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyButton : Button
{
    public Action onUp;
    public Action onDown;
    public Action onEnter;
    public Action onExit;

    [SerializeField]
    private string anim_normal = "Normal";
    [SerializeField]
    private string anim_down = "Down";
    [SerializeField]
    private string anim_up = "Up";

    private string lastAnimName;

    //是否穿透点击
    public bool isThroughClick = false;

    protected override void Start()
    {
        base.Start();
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
     
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (transition != Transition.Animation)
        {
            base.DoStateTransition(state, instant);
        }     
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown?.Invoke();
        base.OnPointerDown(eventData);

        if (transition == Transition.Animation)
            TriggerAnimation(anim_down);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp?.Invoke();
        base.OnPointerUp(eventData);

        if (transition == Transition.Animation)
            TriggerAnimation(anim_up);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (isThroughClick)
            ThroughClick(eventData);      
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter?.Invoke();
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit?.Invoke();
        base.OnPointerExit(eventData);

    }


    private void ThroughClick(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);       
        foreach (var result in results)
        {
            if (result.gameObject != null)
            {
                if (result.gameObject.Equals(this.gameObject))
                    continue;
                ExecuteEvents.Execute(result.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
                break;
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (onUp != null)
            onUp = null;
        if (onDown != null)
            onDown = null;
        if (onEnter != null)
            onEnter = null;
        if (onExit != null)
            onExit = null;

    }

    void TriggerAnimation(string triggername)
    {
        if (!interactable) return;

        if (transition != Transition.Animation || animator == null || !animator.isActiveAndEnabled || !animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
            return;
        if(!string.IsNullOrEmpty(lastAnimName))
        animator.ResetTrigger(lastAnimName);     
        animator.SetTrigger(triggername);
        lastAnimName = triggername;
    }

    public void ResetAnim()
    {
        TriggerAnimation(anim_normal);
    }
}

