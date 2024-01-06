using System;
using TMPro;
using UnityEngine;


public struct Rectangle
{
    public Vector2 center;
    public Vector2 size;

    public Rectangle(Vector2 center, Vector2 size)
    {
        this.center = center;
        this.size = size;
    }
}

[Serializable]
public struct Zone2D
{
    public float startX;
    public float endX;
    public float startY;
    public float endY;

    private float width;
    private float height;

    private Vector3 center;

    public Zone2D(float startX, float endX, float startY, float endY)
    {
        this.startX = startX;
        this.startY = startY;
        this.endX = endX;
        this.endY = endY;

        width = endX - startX;
        height = endY - startY;

        center = new Vector3((endX + startX)/2, (endY + startY) / 2);
    }

    public void UpdateZoneProperties()
    {
        width = endX - startX;
        height = endY - startY;

        center = new Vector3((endX + startX) / 2, (endY + startY) / 2);
    }

    public readonly Vector3 Center => center;
    public readonly float Width => width;
    public readonly float Height => height;
}

public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    public string targetName;

    [Header("Settings")]
    [SerializeField]
    private Vector2 followSpeed;
    [SerializeField]
    private float followAheadSpeedFactor;
    [SerializeField]
    private float aheadFactor;
    [SerializeField]
    private bool moveYOnPlatformOnly;
    [SerializeField]
    private bool useDeadZone;

    [Header("Offsets and deadzones")]
    [SerializeField]
    private float cameraSize;
    [SerializeField]
    private bool useMaxCameraSize;
    [SerializeField]
    private Vector2 margins;
    [SerializeField]
    private Zone2D cameraZone;
    public Vector3 offset;
    public Vector2 deadZoneFromCenter; // deprecated on X
    public Vector2 deadZoneOffset; // deprecated on X

    [SerializeField]
    private Vector2 lastValidPosition;


    private Vector2 currentAheadOffset;


    private GameObject target;
    private Camera mainCamera;
    [SerializeField]
    private Zone2D cameraBoundsZone;
    private PlayerController player;



#if UNITY_EDITOR

    private void OnValidate()
    {
        player = PlayerController.Instance;

        mainCamera = Camera.main;

        cameraZone.UpdateZoneProperties();

        if(mainCamera != null)
        {
            if(useMaxCameraSize)
                mainCamera.orthographicSize = GetMaxCameraSize();
            else
                mainCamera.orthographicSize = cameraSize;
        }

        UpdateCameraBounds();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            cameraZone.Center,
            new Vector3(cameraZone.Width, cameraZone.Height));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            cameraBoundsZone.Center,
            new Vector3(cameraBoundsZone.Width, cameraBoundsZone.Height));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position + (Vector3)deadZoneOffset,
            new Vector3(deadZoneFromCenter.x, deadZoneFromCenter.y));
    }


#endif


    private void Start()
    {
        mainCamera = Camera.main;

        if (useMaxCameraSize)
            mainCamera.orthographicSize = GetMaxCameraSize();
        else
            mainCamera.orthographicSize = cameraSize;

        target = GameObject.Find(targetName);
        player = PlayerController.Instance;

        currentAheadOffset = Vector2.zero;
        lastValidPosition = transform.position;


        cameraZone.UpdateZoneProperties();
        UpdateCameraBounds();
    }

    private void Update()
    {
        FollowTarget();
        UpdateCameraBounds();
    }

    private float GetMaxCameraSize(bool auto = true, bool isYLarger = false)
    {
        if(auto)
            isYLarger = cameraZone.Width/cameraZone.Height < mainCamera.aspect;

        if(isYLarger)
            return ((cameraZone.endX - cameraZone.startX) / 2 - margins.x)/mainCamera.aspect;
        else
            return (cameraZone.endY - cameraZone.startY) / 2 - margins.y;
    }
    private void UpdateCameraBounds()
    {
        // cameraZone is the renderable zone (given)
        // cameraBoundsZone is the accessible zone by the camera transform
        // mainCamera.size represente the semi-height of the camera (given)
        // 1 size unit = 2 blocks (unit) height = 2*screen_ratio blocks width (given)

        // Calculate new bounds with the camera size.
        // The new bounds shall only allow the camera to render alone and only the movingX and movingY zone.


        if (mainCamera == null)
            return;

        float aspect = mainCamera.aspect;
        float size = mainCamera.orthographicSize;

        // Calculate x bounds :

        cameraBoundsZone.startX = cameraZone.startX + size * aspect + margins.x;
        cameraBoundsZone.endX = cameraZone.endX - size * aspect - margins.x;

        // Calculate y bounds :

        cameraBoundsZone.startY = cameraZone.startY + size + margins.y;
        cameraBoundsZone.endY = cameraZone.endY - size - margins.y;

        if (cameraBoundsZone.startX > cameraBoundsZone.endX + .01f || cameraBoundsZone.startY > cameraBoundsZone.endY + .01f)
            Debug.LogError("Camera is too big for the defined camera zone.");

        cameraBoundsZone.UpdateZoneProperties();
    }

    private void FollowTarget()
    {
        Vector3 computedPosition = transform.position;


        // Compute the desired position 
        Vector2 newDesiredPosition = GetDesiredPosition() + GetAheadXOffset() + (Vector2)offset;

        computedPosition.x = Mathf.Clamp(newDesiredPosition.x, cameraBoundsZone.startX, cameraBoundsZone.endX);
        computedPosition.y = Mathf.Clamp(newDesiredPosition.y, cameraBoundsZone.startY, cameraBoundsZone.endY);

        lastValidPosition = computedPosition;


        // Lerp and move
        computedPosition.x = Mathf.Lerp(transform.position.x, computedPosition.x, Time.deltaTime * followSpeed.x);
        computedPosition.y = Mathf.Lerp(transform.position.y, computedPosition.y, Time.deltaTime * followSpeed.y);

        transform.position = computedPosition;
    }

    private Vector2 GetAheadXOffset()
    {
        Vector2 offset = aheadFactor * player.rb.velocity.x * Vector2.right;

        offset = Vector2.Lerp(currentAheadOffset, offset, Time.deltaTime * followAheadSpeedFactor);

        currentAheadOffset = offset;

        return offset;
    }

    private Vector2 GetDesiredPosition()
    {
        Vector2 desPosition = lastValidPosition - (Vector2)offset; // By default don't change the last valid position

        desPosition.x = IsTargetInDeadzone(RectangleCheck.X) ? desPosition.x : target.transform.position.x ;

        if (moveYOnPlatformOnly && player != null)
            desPosition.y = !player.CanJump ? desPosition.y : target.transform.position.y;
        else
            desPosition.y = IsTargetInDeadzone(RectangleCheck.Y) ? desPosition.y : target.transform.position.y;
        

        return desPosition;
    }

    private bool IsTargetInDeadzone(RectangleCheck check = RectangleCheck.Full)
    {
        if (!useDeadZone)
            return false;

        Rectangle deadzone = new Rectangle(transform.position + (Vector3)deadZoneOffset, new Vector3(deadZoneFromCenter.x, deadZoneFromCenter.y));
        return IsInsideRect((Vector2)target.transform.position, deadzone, check);
    }

    private bool IsInsideRect(Vector2 point, Rectangle rect, RectangleCheck check = RectangleCheck.Full)
    {
        bool checkX =
            point.x > rect.center.x - rect.size.x / 2 &&
            point.x < rect.center.x + rect.size.x / 2;

        bool checkY =
            point.y > rect.center.y - rect.size.y / 2 &&
            point.y < rect.center.y + rect.size.y / 2;

        switch (check)
        {
            case RectangleCheck.Full:
                return checkX && checkY;

            case RectangleCheck.X:
                return checkX;

            case RectangleCheck.Y:
                return checkY;

            default:
                return checkX && checkY;
        }
    }

    private enum RectangleCheck
    {
        Full,
        X,
        Y
    }
}
