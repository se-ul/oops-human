using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float pickupRange = 2f;
    public float furnaceRange = 2f;
    public Transform holdPoint;

    private Pickable2DObject heldItem;

    void Update()
    {
        if (heldItem == null)
        {
            TryPickup();
        }
        else
        {
            TryUseFurnace();
        }
    }

    void TryPickup()
    {
        Pickable2DObject[] items = GameObject.FindObjectsOfType<Pickable2DObject>();
        foreach (var item in items)
        {
            if (Vector3.Distance(transform.position, item.transform.position) <= pickupRange)
            {
                heldItem = item;
                item.transform.SetParent(holdPoint);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
                Debug.Log($"Picked up: {item.itemId}");
                break;
            }
        }
    }

    void TryUseFurnace()
    {
        Furnace[] furnaces = GameObject.FindObjectsOfType<Furnace>();
        foreach (var furnace in furnaces)
        {
            if (Vector3.Distance(transform.position, furnace.transform.position) <= furnaceRange)
            {
                Instantiate(heldItem.corresponding3DPrefab, furnace.spawnPoint.position, Quaternion.identity);
                Destroy(furnace.gameObject); // optional
                heldItem.transform.SetParent(null);
                Destroy(heldItem.gameObject);
                Debug.Log($"Used {heldItem.itemId} at furnace");
                heldItem = null;
                break;
            }
        }
    }
}