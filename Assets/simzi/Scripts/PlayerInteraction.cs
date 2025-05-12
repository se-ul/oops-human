using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private string endingSceneName = "Ending";

    public float pickupRange = 3f;
    public float furnaceRange = 3f;
    public Transform holdPoint;

    public AudioSource pickupSound;
    public AudioSource furnaceSound;

    private Pickable2DObject heldItem;

    private int pickedItemCount = 0;

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
        Pickable2DObject[] items = FindObjectsOfType<Pickable2DObject>();
        foreach (var item in items)
        {
            if (Vector3.Distance(transform.position, item.transform.position) <= pickupRange)
            {
                heldItem = item;
                item.transform.SetParent(holdPoint);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;

                Rigidbody rb = item.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.detectCollisions = false;
                }

                Debug.Log($"Picked up: {item.itemId}");
                if (pickupSound != null) pickupSound.Play();
                break;
            }
        }
    }

    void TryUseFurnace()
    {
        Furnace[] furnaces = FindObjectsOfType<Furnace>();
        foreach (var furnace in furnaces)
        {
            if (furnace.isActive && Vector3.Distance(transform.position, furnace.transform.position) <= furnaceRange)
            {
                furnace.isActive = false;

                if (furnace.appearEffect != null)
                {
                    ParticleSystem effect = Instantiate(furnace.appearEffect, furnace.spawnPoint.position, furnace.spawnPoint.rotation);
                    effect.Play();
                    StartCoroutine(SpawnAfterDelay(heldItem.corresponding3DPrefab, furnace.spawnPoint.position, furnace.spawnPoint.rotation, heldItem.spawnScale));
                }

                heldItem.transform.SetParent(null);

                Rigidbody rb = heldItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.detectCollisions = true;
                }

                Destroy(heldItem.gameObject);
                Debug.Log($"Used {heldItem.itemId} at furnace");
                if (furnaceSound != null) furnaceSound.Play();
                heldItem = null;
                pickedItemCount++;
                break;
            }
        }

        if (pickedItemCount >= 2)
        {
            StartCoroutine(GoToEndSceneAfterDelay());
        }
    }

    IEnumerator GrowObject(GameObject obj, Vector3 targetScale)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }
    }

    IEnumerator SpawnAfterDelay(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 targetScale)
    {
        yield return new WaitForSeconds(2.5f);
        GameObject obj = Instantiate(prefab, position, rotation);
        obj.transform.localScale = Vector3.zero;
        StartCoroutine(GrowObject(obj, targetScale));
    }

    IEnumerator GoToEndSceneAfterDelay()
    {
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene(endingSceneName);
    }
}
