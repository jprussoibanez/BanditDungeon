﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Agent
{
    public float[][] valueTable;   // The matrix containing the values estimates.
    float learning_rate = 0.005f;   // The rate at which to update the value estimates given a reward.

    /// <summary>
    /// Initializes a new instance of the <see cref="Agent"/> class.
    /// </summary>
    /// <param name="stateSize">Number of possible states in the environment.</param>
    /// <param name="actionSize">Number of possible actions possible in the environment.</param>
    /// <param name="optimistic">If set to <c>true</c>, then initialize the value estimates as optimistically.</param>
    public Agent(int stateSize, int actionSize, bool optimistic)
    {
        valueTable = new float[stateSize][];
        for (int i = 0; i < stateSize; i++)
        {
            valueTable[i] = new float[actionSize];
            for (int j = 0; j < actionSize; j++)
            {
                valueTable[i][j] = optimistic ? 1.0f : 0.0f;
            }
        }
    }

    /// <summary>
    /// Picks an action to taken given a state.
    /// </summary>
    /// <returns>The action chosen by the agent's policy</returns>
    /// <param name="state">The current environment state</param>
    public int PickAction(int state)
    {
        float confidence = 2.1f - GameObject.Find("confidence_S").GetComponent<Slider>().value;
        float[] probabilities = softmax(valueTable[state], confidence);
        float cumulative = 0.0f;
        int selectedElement = 0;
        float diceRoll = Random.Range(0f, 1f);
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (diceRoll < cumulative)
            {
                selectedElement = i;
                break;
            }
        }
        return selectedElement;
    }

    /// <summary>
    /// Updates the value estimate matrix given a new experience (state, action, reward).
    /// </summary>
    /// <param name="state">The environment state the experience happened in.</param>
    /// <param name="action">The action chosen by the agent in the state.</param>
    /// <param name="reward">The reward received by the agent from the environment for it's action.</param>
    public void UpdatePolicy(int state, int action, float reward)
    {
        valueTable[state][action] += learning_rate * (reward - valueTable[state][action]);
    }

    /// <summary>
    /// Softmax the specified values with a given temperature.
    /// </summary>
    /// <param name="values">The values to be normalized using softmax.</param>
    /// <param name="temp">The desired temperature of the softmax distribution.</param>
    /// <returns name="softmax_values">A set of values with normalized probabilities which sum to 1.</returns>
    float[] softmax(float[] values, float temp)
    {
        float[] softmax_values = new float[values.Length];
        float[] exp_values = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            exp_values[i] = Mathf.Exp(values[i] / temp);
        }

        for (int i = 0; i < values.Length; i++)
        {
            softmax_values[i] = exp_values[i] / exp_values.Sum();
        }
        return softmax_values;
    }
}
