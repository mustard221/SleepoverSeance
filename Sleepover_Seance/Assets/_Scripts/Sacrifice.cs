using UnityEngine;

public class Sacrifice : MonoBehaviour
{
    public AudioSource vanish;
    public AudioSource laugh;
    public GameObject model;
    public void OnTriggerEnter(UnityEngine.Collider other)
    {
        if (other.gameObject.CompareTag("Sacrifice"))
        {
            Debug.Log("Person entered sacrifice area");

            /* var person = other.gameObject;
            person.SetActive(false); */ 


            this.model.SetActive(false);

            laugh.Play();
            vanish.Play();
        }
    }
}
