using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField]
    private bool useRideModule;
    [SerializeField]
    private RidePlatform ridePlatform;

	[Header("Speed")]
    [SerializeField]
	private float maxSpeed;
	[SerializeField]
	private float acceleration;

	[Header("Spawns and dispawns")]
	[SerializeField]
	private Axis dispawnAxis;
	[SerializeField]
	private float xDispawn;
	[SerializeField]
	private float yDispawn;
	[SerializeField]
	private bool respawn;
	[SerializeField]
	private float disappearingTime;
	[SerializeField]
	private float respawnTime;

	[Header("Deceleration")]
	[SerializeField]
	private bool useDecelerate;
	[SerializeField]
	private Vector2 brakeDistance;
	[SerializeField]
	private float brakeForce;

	[Header("Direction")]

	[SerializeField]
	private bool useSpecialDirection;
	[SerializeField]
	private Vector2 specialDirection;

	private Vector2 speedDirection;
	private float speed;
	private bool isFalling;
	private Vector2 startPosition;

	[Header("References")]
	//[SerializeField]
	//private Transform slime;
	[SerializeField]
	private Animator animator;
	private PlayerController playerController;
	private Rigidbody2D rb;



#if UNITY_EDITOR

	private void OnDrawGizmosSelected()
	{
		Vector3 destination = transform.position + Vector3.right * xDispawn + Vector3.up * yDispawn;

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, destination);
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(destination, destination - (Vector3)brakeDistance);
	}

#endif



	private void Start()
	{
		if (useRideModule && ridePlatform == null)
			Debug.Log("Warning: using ride module but no rideplatform assigned");

		rb = GetComponent<Rigidbody2D>();

		isFalling = false;
		playerController = PlayerController.Instance;
		speed = 0;

		startPosition = transform.position;
		speedDirection = Vector2.down;

		if (useSpecialDirection)
			speedDirection = specialDirection.normalized;

	}

	private void Reset()
	{
		//Debug.Log(playerController.transform.parent);

		//if(playerController.transform.parent == slime)
		//    playerController.transform.SetParent(null, true);

		rb.velocity = Vector2.zero;
		speed = 0;
		isFalling = false;
		transform.position = startPosition;
		animator.SetBool("isFalling", false);
		animator.SetTrigger("reset");
	}

	private IEnumerator PlatformFall()
	{
		animator.SetBool("isFalling", true);

		while(!MustPlatformStop())
		{
			if (useRideModule)
			{
				rb.velocity = new Vector2(0, speedDirection.y * speed);

				float xDisplacement = speed * Time.deltaTime * speedDirection.x;

				transform.Translate(xDisplacement * Vector2.right);

				ridePlatform.MoveTarget(xDisplacement);
			}
			else
			{
                rb.velocity = speedDirection * speed;
            }


            speed += acceleration * Time.deltaTime;

			speed = Mathf.Clamp(speed, 0, maxSpeed);

			yield return null;
		}

		yield return PlatformDecelerate();
	}

	private IEnumerator PlatformDecelerate()
	{
		while(useDecelerate && rb.velocity.magnitude > .01f)
		{
			//speed = Mathf.Lerp(speed, 0, Time.deltaTime * brakeForce);

			//transform.Translate(speed * speedDirection * Time.deltaTime);

			rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * brakeForce);

			yield return null;
		}

		rb.velocity = Vector3.zero;

		yield return new WaitForSeconds(disappearingTime);

		if(!respawn)
			Destroy(gameObject);

		yield return new WaitForSeconds(respawnTime);

		Reset();
	}

	private bool MustPlatformStop()
	{
		if (!useSpecialDirection)
			return transform.position.y < yDispawn;


		bool isOutXBounds = specialDirection.x >= 0 ?
			transform.position.x > startPosition.x + xDispawn - brakeDistance.x :
			transform.position.x < startPosition.x + xDispawn + brakeDistance.x ;

		bool isOutYBounds = specialDirection.y >= 0 ?
			transform.position.y > startPosition.y + yDispawn - brakeDistance.y :
			transform.position.y < startPosition.y + yDispawn + brakeDistance.y ;


		return dispawnAxis switch
		{
			Axis.Default => isOutXBounds || isOutYBounds,
			Axis.X => isOutXBounds,
			Axis.Y => isOutYBounds,
			_ => false,
		};
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (isFalling)
			return;
		if (collision?.gameObject == playerController.gameObject) {
			if (collision.gameObject.transform.position.y - collision.collider.bounds.size.y/2 > transform.position.y)
			{
				isFalling = true;
				StartCoroutine(PlatformFall());
			}
		}
	}
}
