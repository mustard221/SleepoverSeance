using UnityEngine;

public class Sacrifice : MonoBehaviour
{
    public AudioSource vanish;
    public AudioSource laugh;
    public GameObject pinkModel;
    public GameObject greenModel;
    public GameObject newSacrificeArea;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Sacrifice"))
        {
            Debug.Log("Person entered sacrifice area");

            /* var person = other.gameObject;
            person.SetActive(false); */ 

            pinkModel.SetActive(false);
            //newSacrificeArea.SetActive(true);
            this.gameObject.SetActive(false);

            laugh.Play();
            vanish.Play();
        }
    }
}
