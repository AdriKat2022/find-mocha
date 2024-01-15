using System.Collections;
using UnityEngine;

public class RidePlatform : MonoBehaviour
{
	[SerializeField]
	private LayerMask riders;

	//[SerializeField]
	//private Rigidbody2D platform;

	//private List<Rigidbody2D> targets;

	private Rigidbody2D target_rb;

	//[SerializeField]
	//private Rigidbody2D rb;



	private void Start() {

		target_rb = null;
		//StartCoroutine(CarryTarget());
	}

	private void OnTriggerStay2D(Collider2D col){

		if (!Helper.IsLayerInLayerMask(col.gameObject.layer, riders))
			return;

		col.gameObject.TryGetComponent(out target_rb);
	}

	private void OnTriggerExit2D(Collider2D col){

		target_rb = null;

	}


	public void MoveTarget(float xDisplacement)
	{
		if(target_rb != null)
		{
			Debug.Log(target_rb.transform.position + xDisplacement * Vector3.right);

            target_rb.transform.position = target_rb.transform.position + xDisplacement * Vector3.right;
		}
	}

}
