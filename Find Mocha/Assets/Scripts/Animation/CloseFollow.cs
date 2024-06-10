using UnityEngine;

public class CloseFollow : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float speed = 1.0f;
    [SerializeField, Tooltip("The size of the box in which the target can freely move without being followed.")]
    private Vector2 looseOffset = Vector2.zero;
    [SerializeField, Tooltip("Offset the center of the followed transform")]
    private Vector2 offset = Vector2.up;


    private bool isFollowing = false;
    private Vector3 currentTargetPosition; // Position to focus on

    public void StartFollowingPlayer()
    {
        target = PlayerController.Instance.transform;
        StartFollowing();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetLooseOffset(Vector2 newOffset)
    {
        looseOffset = newOffset;
    }


    public void StartFollowing()
    {
        isFollowing = true;
        currentTargetPosition = target.position + (Vector3)offset;
    }

    public void StopFollowing()
    {
        isFollowing = false;
    }


    private void Update()
    {
        if (!isFollowing)
            return;

        // Update the current position to approach the currentTargetPosition.
        transform.position = Vector3.Lerp(transform.position, currentTargetPosition, speed * Time.deltaTime);

        // If the target is in the loose box, don't update the current target.
        if(IsTargetInLooseBox())
            return;

        // Update the current target position.
        currentTargetPosition = target.position + (Vector3)offset;
    }

    private bool IsTargetInLooseBox()
    {
        Vector2 targetPosition = target.position + (Vector3)offset;
        Vector2 currentPosition = transform.position;

        return targetPosition.x >= currentPosition.x - looseOffset.x && targetPosition.x <= currentPosition.x + looseOffset.x &&
            targetPosition.y >= currentPosition.y - looseOffset.y && targetPosition.y <= currentPosition.y + looseOffset.y;
    }

}
