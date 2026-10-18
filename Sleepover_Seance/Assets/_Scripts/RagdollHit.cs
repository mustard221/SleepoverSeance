using UnityEngine;

public class RagdollHit : MonoBehaviour
{
    public GameObject ragdoll;
    public WeaponObject weaponObject;

    public void OnCollisionEnter(Collision collision)
    {
        weaponObject = weaponObject.GetComponent<WeaponObject>();

        if (weaponObject.isHeld == false)
        {
            if (collision.gameObject.CompareTag("Weapon"))
            {
                Debug.Log("Person hit by weapon");
                ragdoll.SetActive(true);
            }
        }
        else
        {
            return;
        }
    }
}
