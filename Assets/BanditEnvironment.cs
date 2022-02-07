using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class BanditEnvironment : MonoBehaviour
{

    # region Resources
    private const string ResourceBanditChest = "bandit_chest";
    private const string ResourceChestValue = "value";
    private const string ResourceChestTrueValue = "true_value";
    private const string ResourceBanditStates = "state_D";
    private const string ResourceAgentSpeed = "speed_S";
    private const string ResourceBanditArms = "arms_D";
    private const string ResourceOptimisticToggle = "optToggle";
    private const string ResourceDifficulty = "diff_D";
    private const string ResourceRestartButton = "restartButton";
    private const string ResourceLoading = "loading";
    public const string ResourceSlime = "slime";
    public const string ResourceDiamond = "diamond";
    # endregion

    public List<GameObject> chests; // List of chest objects.
    public List<GameObject> estimatedValues; // List of visualized value estimates (green orbs).
    public List<GameObject> trueValues; // List of visualized true values (clear orbs).
    public float totalRewards; // Total rewards obtained over the course of all trials.
    int trial; // Trial index.
    int numberArms; // Number of chests in a given trial.
    int totalStates; // Number of possible rooms with unique chest reward probabilities. 
    int state; // Index of current room.
    public float actSpeed; // Speed at which actions are chosen.
    float[][] armProbabilities; // True probability values for each chest in each room.
    Agent agent; // The agent which learns to pick actions.

    // Use this for initialization
    void Start()
    {
    }

    private void SetupGame()
    {

    }
    /// <summary>
    /// Initialized the bandit game. Called when "Start Learning" button in clicked.
    /// </summary>
    public void BeginLearning()
    {
        trial = 0;
        totalRewards = 0;
        int stateMode = GameObject.Find(BanditEnvironment.ResourceBanditStates).GetComponent<Dropdown>().value;
        actSpeed = 0.5f - GameObject.Find(BanditEnvironment.ResourceAgentSpeed).GetComponent<Slider>().value;
        totalStates = stateMode == 0 ? 1 : 3;
        numberArms = GameObject.Find(BanditEnvironment.ResourceBanditArms).GetComponent<Dropdown>().value + 2;
        bool optimistic = GameObject.Find(BanditEnvironment.ResourceOptimisticToggle).GetComponent<Toggle>().isOn;

        agent = new Agent(totalStates, numberArms, optimistic);

        CreateTrueArmProbabilities();

        GameObject[] startUI = (GameObject.FindGameObjectsWithTag(BanditEnvironment.ResourceLoading));
        foreach (GameObject obj in startUI)
        {
            Destroy(obj);
        }

        chests = new List<GameObject>();
        estimatedValues = new List<GameObject>();
        trueValues = new List<GameObject>();

        GameObject.Find(BanditEnvironment.ResourceRestartButton).GetComponent<Button>().interactable = true;
        LoadTrial();
    }

    // Update is called once per frame
    void Update()
    {
        actSpeed = 0.5f - GameObject.Find(BanditEnvironment.ResourceAgentSpeed).GetComponent<Slider>().value;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    /// <summary>
    /// Gets an action from the agent, selects the chest accordingly,
    /// and updates the agent's value estimates based on the reward.
    /// </summary>
    IEnumerator Act()
    {
        yield return new WaitForSeconds(actSpeed);
        int action = agent.PickAction(state);
        float reward = chests[action].GetComponent<SelectAction>().Selected();
        totalRewards += reward;
        agent.UpdatePolicy(state, action, reward);
    }

    /// <summary>
    /// Resets chests for new trial.
    /// </summary>
    public void LoadTrial()
    {
        trial += 1;
        state = Random.Range(0, totalStates);

        if (totalStates == 1)
        {
            GameObject.Find("Directional Light").GetComponent<Light>().color = new Color(1f, 1f, 1f);
        }
        else
        {
            if (state == 0)
            {
                GameObject.Find("Directional Light").GetComponent<Light>().color = new Color(1f, 0f, 0f);
            }
            if (state == 1)
            {
                GameObject.Find("Directional Light").GetComponent<Light>().color = new Color(0f, 1f, 0f);
            }
            if (state == 2)
            {
                GameObject.Find("Directional Light").GetComponent<Light>().color = new Color(0f, 0f, 1f);
            }
        }

        GameObject.Find("Text").GetComponent<Text>().text = "Trial: " + trial.ToString() + "\nTotal Reward: " + totalRewards.ToString();

        foreach (GameObject chest in chests)
        {
            DestroyImmediate(chest);
        }
        foreach (GameObject value in estimatedValues)
        {
            DestroyImmediate(value);
        }
        foreach (GameObject value in trueValues)
        {
            DestroyImmediate(value);
        }

        chests = new List<GameObject>();
        estimatedValues = new List<GameObject>();
        trueValues = new List<GameObject>();


        DestroyImmediate(GameObject.Find(BanditEnvironment.ResourceSlime));
        DestroyImmediate(GameObject.Find(BanditEnvironment.ResourceDiamond));

        int totalArms = numberArms + 1;
        for (int armIndex = 0; armIndex < numberArms; armIndex++)
        {
            chests.Add(CreateChest(chestIndex: armIndex, totalChests: totalArms));

            estimatedValues.Add(
                CreateSphere(chestIndex: armIndex, totalChests: totalArms, sphereValue: agent.valueTable, sphereResource: BanditEnvironment.ResourceChestValue)
            );

            trueValues.Add(
                CreateSphere(chestIndex: armIndex, totalChests: totalArms, sphereValue: armProbabilities, sphereResource: BanditEnvironment.ResourceChestTrueValue)
            );
        }
        StartCoroutine(Act());
    }

    /// <summary>
    /// Creates chest game object and position in line.
    /// </summary>
    /// <param name="chestIndex">Chest index within all chests</param>
    /// <param name="totalChests">Total chests on game</param>
    /// <returns>Chest game object in position</returns>
    private GameObject CreateChest(int chestIndex, int totalChests)
    {
        GameObject chest = (GameObject)GameObject.Instantiate(Resources.Load(BanditEnvironment.ResourceBanditChest));
        chest.transform.position = new Vector3((chestIndex + 1) * (12.5f / (totalChests - 1)) - ((12.5f / (totalChests - 1)) * totalChests) / 2, 0f, 1.5f);
        chest.GetComponent<SelectAction>().myProbability = this.armProbabilities[this.state][chestIndex];

        return chest;
    }

    private GameObject CreateSphere(int chestIndex, int totalChests, float[][] sphereValue, string sphereResource)
    {
        GameObject sphere = (GameObject)GameObject.Instantiate(Resources.Load(sphereResource));
        sphere.transform.position = new Vector3((chestIndex + 1) * (12.5f / (totalChests - 1)) - ((12.5f / (totalChests - 1)) * totalChests) / 2, 3f, 1.5f);
        float inflation = 2 * sphereValue[state][chestIndex] + 0.25f;

        inflation = Mathf.Clamp(inflation, 0, 2.5f);
        sphere.transform.localScale = new Vector3(inflation, inflation, inflation);

        return sphere;
    }

    private void CreateTrueArmProbabilities()
    {
        int difficulty = GameObject.Find(BanditEnvironment.ResourceDifficulty).GetComponent<Dropdown>().value;
        float difficultyAdjustment = ((float)difficulty) * 0.1f;
        armProbabilities = new float[totalStates][];
        for (int i = 0; i < totalStates; i++)
        {
            armProbabilities[i] = new float[numberArms];
            int winner = Random.Range(0, numberArms);
            for (int j = 0; j < numberArms; j++)
            {
                if (j == winner)
                {
                    armProbabilities[i][j] = Random.Range(0.6f, 1.0f - difficultyAdjustment);
                }
                else
                {
                    armProbabilities[i][j] = Random.Range(0.0f + difficultyAdjustment, 0.4f);
                }
            }
        }
    }
    public void ReloadEnvironment()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
