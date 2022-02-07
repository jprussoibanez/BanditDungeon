using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectAction : MonoBehaviour
{
    public float myProbability;
    private const float rewardWin = 1.0f;
    private const float rewardLose = -0.5f;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float Selected()
    {
        float reward = 0f;
        if (myProbability > Random.Range(0.0f, 1.0f))
        {
            reward = rewardWin;
            ShowResult(BanditEnvironment.ResourceDiamond);
        }
        else
        {
            reward = rewardLose;
            ShowResult(BanditEnvironment.ResourceSlime);
        }
        StartCoroutine(Example());
        return reward;
    }

    private void ShowResult(string resource)
    {
        GameObject slime = (GameObject)GameObject.Instantiate(Resources.Load(resource));
        slime.transform.position = this.transform.position;
        slime.name = resource;
        foreach (GameObject chest in GameObject.Find("Main Camera").GetComponent<BanditEnvironment>().chests)
        {
            if (chest == this.gameObject)
            {
                chest.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            chest.gameObject.GetComponent<Collider>().enabled = false;
        }
    }

    void OnMouseDown()
    {
        float r = Selected();
        GameObject.Find("Main Camera").GetComponent<BanditEnvironment>().totalRewards += r;
    }

    IEnumerator Example()
    {
        yield return new WaitForSeconds(GameObject.Find("Main Camera").GetComponent<BanditEnvironment>().actSpeed);
        GameObject.Find("Main Camera").GetComponent<BanditEnvironment>().LoadTrial();
    }


}
