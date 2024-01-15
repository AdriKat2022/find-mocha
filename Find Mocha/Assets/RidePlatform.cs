using System.Collections.Generic;
using UnityEngine;

public class RidePlatform : MonoBehaviour
{
    [SerializeField]
    private LayerMask riders;

    [SerializeField]
    private Rigidbody2D platform;

    private List<Rigidbody2D> targets;

/*
    private void Start()
    {
        targets = new List<Rigidbody2D>();
    }


    private void LateUpdate()
    {
        //MoveRiders();
    }
*/

    private void MoveRiders()
    {
        float speed = platform.velocity.x;

        Debug.Log(speed);

        foreach (Rigidbody2D rb in targets)
        {
            rb.MovePosition(rb.position + Time.deltaTime * speed * Vector2.right);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //other.gameObject.transform.SetParent(transform, true);

        //return;
/*
        if (!Helper.IsLayerInLayerMask(other.gameObject.layer, riders))
            return;

        if(other.gameObject.TryGetComponent(out Rigidbody2D rb)){
            targets.Add(rb);
        }*/
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //other.gameObject.transform.SetParent(null, true);

        //return;

        /*if (!Helper.IsLayerInLayerMask(other.gameObject.layer, riders))
            return;

        if (other.gameObject.TryGetComponent(out Rigidbody2D rb))
        {
            if (!targets.Contains(rb))
                return;

            targets.Remove(rb);
        }*/
    }
}
