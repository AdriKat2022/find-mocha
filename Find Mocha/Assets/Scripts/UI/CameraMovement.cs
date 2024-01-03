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

public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    public string targetName;

    [Header("Settings")]
    [SerializeField]
    private float followSpeed;
    [SerializeField]
    private float followAheadSpeed;
    [SerializeField]
    private float aheadFactor;

    [Header("Offsets and deadzones")]
    public Vector3 offset;
    public Vector2 movingZoneX;
    public Vector2 movingZoneY;
    public Vector2 deadZoneFromCenter;
    public Vector2 deadZoneOffset;


    [SerializeField]
    private Vector2 lastTargetPosition;

    [SerializeField]
    private GameObject target;

    [SerializeField]
    private Vector2 currentAheadOffset;


    //private Camera mainCamera;
    //private Vector2 cameraZoneX;
    //private Vector2 cameraZoneY;
    //private Vector2 cameraBoundsX;
    //private Vector2 cameraBoundsY;


#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((movingZoneX.x + movingZoneX.y) / 2, (movingZoneY.x + movingZoneY.y) / 2),
            new Vector3(movingZoneX.y - movingZoneX.x, movingZoneY.y - movingZoneY.x));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position + (Vector3)deadZoneOffset,
            new Vector3(deadZoneFromCenter.x, deadZoneFromCenter.y));
    }


#endif


    private void Start()
    {
        mainCamera = Camera.main;

        currentAheadOffset = Vector2.zero;
        //desiredAheadOffset = Vector2.zero;

        target = GameObject.Find(targetName);
        lastTargetPosition = transform.position;
    }

    private void Update()
    {
        FollowTarget();
    }

    private void UpdateMovingZone()
    {
        // Calculate new bounds with the camera size.
        // The new bounds shall only allow the camera to render alone and only the movingX and movingY zone.



    }

    private void FollowTarget()
    {
        Vector3 computedPosition = transform.position;

        lastTargetPosition = GetDesiredPosition() + GetAheadOffset();

        computedPosition.x = Mathf.Clamp(lastTargetPosition.x, movingZoneX.x, movingZoneX.y);
        computedPosition.y = Mathf.Clamp(lastTargetPosition.y, movingZoneY.x, movingZoneY.y);


        transform.position = Vector3.Lerp(transform.position, computedPosition+offset, Time.deltaTime * followSpeed);

        lastTargetPosition = computedPosition;
    }

    private Vector2 GetAheadOffset()
    {
        Vector2 offset = Vector2.right * PlayerController.Instance.rb.velocity.x * aheadFactor;

        offset = Vector2.Lerp(currentAheadOffset, offset, Time.deltaTime * followAheadSpeed);


        currentAheadOffset = offset;

        return offset;
    }

    private Vector2 GetDesiredPosition()
    {
        Vector2 desPosition = lastTargetPosition;

        desPosition.x = IsTargetInDeadzone(RectangleCheck.X) ? lastTargetPosition.x : target.transform.position.x;
        desPosition.y = IsTargetInDeadzone(RectangleCheck.Y) ? lastTargetPosition.y : target.transform.position.y;

        return desPosition;
    }

    private bool IsTargetInDeadzone(RectangleCheck check = RectangleCheck.Full)
    {
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
