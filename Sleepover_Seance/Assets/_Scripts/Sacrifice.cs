using UnityEngine;
using TMPro;

public class Sacrifice : MonoBehaviour
{
    public AudioSource vanish;
    public AudioSource laugh;
    public AudioSource laugh2;
    public GameObject model;
    public GameObject blueModel;
    public GameObject greenModel;
    public GameObject endText;
    public TMP_Text friendsSacrificedCounter;
    public int friendsSacrificed = 0;

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

            friendsSacrificed = friendsSacrificed += 1;
            if (friendsSacrificed > 2) //JUST FOR VIDEO PURPOSES
            {
                friendsSacrificed = 2;
            }

            friendsSacrificedCounter.text = "Friends Sacrificed: " + friendsSacrificed.ToString() + " / 3";
        }
        else if (other.CompareTag("BlueSacrifice"))
        {
            Debug.Log("Blue person entered sacrifice area");

            blueModel.SetActive(false);
            laugh.Play();
            vanish.Play();

            endText.SetActive(true);
            laugh2.Play();

            friendsSacrificed = friendsSacrificed += 1;
            if (friendsSacrificed > 3)
            {
                friendsSacrificed = 3;
            }

            friendsSacrificedCounter.text = "Friends Sacrificed: " + friendsSacrificed.ToString() + " / 3";
        }
        else if (other.CompareTag("GreenSacrifice"))
        {
            Debug.Log("Green person entered sacrifice area");

            greenModel.SetActive(false);
            laugh.Play();
            vanish.Play();

            friendsSacrificed = friendsSacrificed += 1;
            if (friendsSacrificed > 1)
            {
                friendsSacrificed = 1;
            }

            friendsSacrificedCounter.text = "Friends Sacrificed: " + friendsSacrificed.ToString() + " / 3";
        }
    }
}
