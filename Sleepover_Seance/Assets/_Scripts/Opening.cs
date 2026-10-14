using UnityEngine;

public class Opening : MonoBehaviour
{
    public Behaviour movementScript;

    public GameObject candle;
    public GameObject placeArea;
    public GameObject demon;

    public Camera playerCam;

    public Vector3 placeOffset = Vector3.zero;

    public AudioSource demonSpawn;
    public AudioSource place;

    bool placed = false;

    void Start()
    {
        if (movementScript != null)
        {
            movementScript.enabled = false; // only disable movement, keep look/camera active
        }

        if (demon != null)
        {
            demon.SetActive(false); // demon starts inactive
        }

        if (playerCam == null)
        {
            playerCam = Camera.main;
        }
    }

    void Update()
    {
        if (movementScript == null) return;

        if (placed)
        {
            if (!movementScript.enabled)
                movementScript.enabled = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPlaceCandle();
            Debug.Log("pressed E");
        }
    }

    void TryPlaceCandle()
    {
        if (candle == null || placeArea == null || playerCam == null) return;

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = playerCam.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider != null && hit.collider.gameObject == placeArea)
            {
                PlaceCandleAt(hit.point, hit.normal); // place candle if camera is hitting place area
                Debug.Log("candle placed");
                place.Play();
            }
        }
    }

    void PlaceCandleAt(Vector3 position, Vector3 normal)
    {
        candle.transform.SetParent(placeArea.transform, true); // re parent & align candle with place area
        candle.transform.position = position + placeOffset;

        placed = true;

        if (movementScript != null)
        {
            movementScript.enabled = true; // allow movement after placing
        }

        if (demon != null)
        {
            demon.SetActive(true); // spawn demon 
            demonSpawn.Play();
        }
    }
}
