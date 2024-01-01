using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis
{
    Default, X, Y
}

public class SineAnimation : MonoBehaviour
{
    [Header("Animation parameters")]
    [SerializeField]
    private float speed;
    [SerializeField]
    private float depth;
    [SerializeField]
    private Axis animationAxis;

    [Header("Special parameters")]
    [SerializeField]
    private bool useRealTime = false;
    [SerializeField]
    private bool activateOnAwake = false;


    [Header("Override")]
    [SerializeField]
    private Transform overrideObjectToAnimate;


    [Header("Debug")]
    [SerializeField]
    private bool activated;
    private float _time;
    private Vector2 basePosition;



    private Transform toAnimate;



    private void Start()
    {
        _time = 0;
        activated = activateOnAwake;

        if (overrideObjectToAnimate != null)
            toAnimate = overrideObjectToAnimate;
        else
            toAnimate = transform;


        basePosition = toAnimate.position;
    }

    private void Update()
    {
        if (!activated)
            return;

        if (useRealTime)
            _time += Time.unscaledDeltaTime;
        else
            _time += Time.deltaTime;

        AnimatePosition();
    }

    public void Activate(bool reset = true)
    {
        if (reset)
            _time = 0;

        activated = true;
    }

    public void Deactivate() => activated = false;

    private void AnimatePosition()
    {
        Vector2 dir = animationAxis == Axis.X ? Vector2.right : Vector2.up;
        Vector2 desiredPosition = dir * Mathf.Sin(_time * speed) * depth;
        toAnimate.position = basePosition + desiredPosition;
    }
}

