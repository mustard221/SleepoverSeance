using UnityEngine;

public class Sacrifice : MonoBehaviour
{
    public AudioSource vanish;
    public AudioSource laugh;
    public GameObject model;
    public GameObject blueModel;
    public GameObject greenModel;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            $"Entered object: {other.name}, " +
            $"tag: {other.tag}, " +
            $"root: {other.transform.root.name}, " +
            $"root tag: {other.transform.root.tag}");

        if (other.CompareTag("Sacrifice"))
        {
            Debug.Log("Pink person entered sacrifice area");

            model.SetActive(false);
            laugh.Play();
            vanish.Play();
        }
        else if (other.CompareTag("BlueSacrifice"))
        {
            Debug.Log("Blue person entered sacrifice area");

            blueModel.SetActive(false);
            laugh.Play();
            vanish.Play();
        }
        else if (other.CompareTag("GreenSacrifice"))
        {
            Debug.Log("Green person entered sacrifice area");

            greenModel.SetActive(false);
            laugh.Play();
            vanish.Play();
        }
    }
}
