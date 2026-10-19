using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public GameObject ragdoll;

    // grab icon image
    public GameObject grabIMG;

    public Transform hand;

    public bool isHeld;

    public float throwForce = 5f;

    public AudioSource throwSound;

    public void PickUpWeapon()
    {
        if (grabIMG != null)
        {
            grabIMG.SetActive(false);
        }

        // disable physics and rb stuff
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // parent to the hand and snap to exact position
        transform.SetParent(hand, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        isHeld = true;
    }

    public void ThrowWeapon()
    {
        if (!isHeld)
            return;

        var rb = GetComponent<Rigidbody>();
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = transform.forward * throwForce;
            rb.angularVelocity = Vector3.zero;
        }

        isHeld = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isHeld && Input.GetMouseButtonDown(0))
        {
            ThrowWeapon();
            throwSound.Play();
            return;
        }
    }
}
