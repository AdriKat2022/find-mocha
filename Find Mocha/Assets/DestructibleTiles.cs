using UnityEngine;
using UnityEngine.Tilemaps;


public class Helper
{
    public static bool IsLayerInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask & (1 << layer)) != 0;
    }
}

public class DestructibleTiles : MonoBehaviour
{
    [SerializeField]
    private Team team;

    [SerializeField]
    private Tilemap destructibleTileMap;
    [SerializeField]
    private LayerMask projectiles;


    [SerializeField]
    private GameObject destructionEffect;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision);

        if (collision == null)
            return;

        if(Helper.IsLayerInLayerMask(collision.gameObject.layer, projectiles))
        {
            Vector3 hitPosition = Vector3.zero;

            ContactPoint2D[] hits = new ContactPoint2D[10];

            collision.GetContacts(hits);
 
            foreach(ContactPoint2D hit in hits)
            {
                // This ensures we are deleting the correct tile
                hitPosition.x = hit.point.x - .01f * hit.normal.x;
                hitPosition.y = hit.point.y - .01f * hit.normal.y;

                Debug.Log(hitPosition);
            }

            DestroyTile(hitPosition);
            Destroy(collision.gameObject);
        }
    }



    private void DestroyTile(Vector3 hitPosition)
    {
        Instantiate(destructionEffect, hitPosition, Quaternion.identity);
        destructibleTileMap.SetTile(destructibleTileMap.WorldToCell(hitPosition), null);
    }
}
