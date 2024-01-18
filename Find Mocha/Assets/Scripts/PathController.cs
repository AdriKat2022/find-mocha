using System.Collections;
using UnityEngine;

public class PathController : MonoBehaviour
{
	private readonly float Time_before_instantiation = 1f;
	private readonly float precision = .2f;

	[Header("Speed")]
	[SerializeField]
    private float startSpeed;
	[SerializeField]
	private bool noLeftoverSpeed;
	[Header("Options")]
	[SerializeField]
	private float totalTime;
	[SerializeField, Tooltip("If true, uses totalTime as reference for one cycle and ignores startSpeed.\nIf false, uses startSpeed and ignores totalTime.")]
	private bool useTotalTime;
	[SerializeField]
	private bool activateOnAwake = true;
	[SerializeField, Range(0f,1f)]
	private float phaseNormalized = 0;

	[Header("Path")]
	[SerializeField]
	private float endLag;
	[SerializeField]
	private Vector2 startPath;
	[SerializeField]
	private Vector2 endPath;

	private Rigidbody2D rb;

	private Vector2 startingPosition;
	private Vector2 pointA;
	private Vector2 pointB;
	private bool activated;
	private bool started;
	private float speed;
	private float phaseTime;


#if UNITY_EDITOR

	private void OnValidate()
	{
		Vector2 pos;

		if (!started)
			pos = transform.position;
		else
			pos = startingPosition;

		pointB = pos + endPath;
		pointA = pos + startPath;
	}
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(pointA, pointB);
	}

#endif

	public void Activate()
	{
		if (activated)
			return;

		activated = true;
		StartCoroutine(PathProceed());
	}

	public void Deactivate() => activated = false;

	public void SetSpeed(float speed) {
		if(useTotalTime)
		{
			Debug.LogWarning("A script tries to modify the speed of the PathController script but it's using totalTime cycle.");
			return;
		}
		this.speed = speed;
		ComputePhase();
	}

	private void Awake()
	{
		TryGetComponent(out rb);

		started = true;
		startingPosition = transform.position;

		pointB = (Vector2)transform.position + endPath;
		pointA = (Vector2)transform.position + startPath;

		speed = startSpeed;

		if(useTotalTime)
			speed = GetSpeedFromTotalTime();

		ComputePhase();

		if (activateOnAwake)
		{
			activated = true;
			StartCoroutine(PathProceed());
		}
		else
			activated = false;
	}

	private IEnumerator PathProceed()
	{
		yield return new WaitForSeconds(Time_before_instantiation);

		yield return new WaitForSeconds(phaseTime);

		while(activated)
		{
			rb.velocity = new Vector2(pointB.x - transform.position.x, pointB.y - transform.position.y).normalized * speed;


			if (Mathf.Abs(pointB.x - transform.position.x) < precision && Mathf.Abs(pointB.y - transform.position.y) < precision)
			{
				(pointA, pointB) = (pointB, pointA);
				if(noLeftoverSpeed)
					rb.velocity = Vector2.zero;

				yield return new WaitForSeconds(endLag);
				
			}

			yield return null;
		}
	}

	private void ComputePhase()
	{
		if (useTotalTime)
			phaseTime = totalTime * phaseNormalized;
		else
			phaseTime = speed == 0 ? 0 : (2 * (endPath - startPath).magnitude / speed + 2 * endLag) * phaseNormalized;
	}

	private float GetSpeedFromTotalTime()
	{
		if(totalTime - 2 * endLag <= 0)
		{
			Debug.LogError("The path of "+gameObject+" has an end lag that is too large for the totalTime.\nSpeed value has to be unexpected.");
			return totalTime <= 0 ? 0 : 1/totalTime;
		}

		return (endPath-startPath).magnitude/(totalTime - 2 * endLag)/2;
	}
}
