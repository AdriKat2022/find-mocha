using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PowerUp
{
    public PowerUpType powerUpType;
    public float value;
    public float duration;

    public PowerUp(PowerUpType powerUpType, float value, float duration)
    {
        this.powerUpType = powerUpType;
        this.value = value;
        this.duration = duration;
    }
}
