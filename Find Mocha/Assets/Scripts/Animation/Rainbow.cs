using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rainbow : MonoBehaviour, IAnimation
{

    [SerializeField]
    private float speed;
    [SerializeField]
    [Range(0f,1f)]
    private float saturation;
    [SerializeField]
    private bool spriteRendererMode = false;
    [SerializeField]
    private bool enableOnAwake = false;

    private bool activated;

    [SerializeField]
    private Image targetImage;
    [SerializeField]
    private SpriteRenderer targetSpriteRenderer;

    private Color color;
    private Color previousColor;

    private float hue;

    public void Activate()
    {
        if(spriteRendererMode)
            previousColor = targetSpriteRenderer.color;
        else
            previousColor = targetImage.color;

        activated = true;
    }

    public void Deactivate()
    {
        activated = false;

        if (spriteRendererMode)
            targetSpriteRenderer.color = previousColor;
        else
            targetImage.color = previousColor;
    }


    private void Start()
    {
        hue = 0;

        if(targetImage == null && targetSpriteRenderer == null)
        {
            if(spriteRendererMode)
                TryGetComponent(out targetSpriteRenderer);
            else
                TryGetComponent(out targetImage);
        }

        if(enableOnAwake)
            Activate();
    }

    private void Update()
    {
        /*if (spriteRendererMode)
        {
            if (targetSpriteRenderer == null)
                Destroy(this);
        }
        else
        {
            if (targetImage == null)
                Destroy(this);
        }*/

        if (activated)
        {
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        hue += speed * Time.deltaTime;

        if (hue > 1.0f)
        {
            hue -= 1.0f;
        }

        color = Color.HSVToRGB(hue, saturation, 1.0f);

        if (spriteRendererMode)
            targetSpriteRenderer.color = color;
        else
            targetImage.color = color;
    }
}
