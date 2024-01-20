using System;
using TMPro;
using UnityEngine;

public class PanelModel : MonoBehaviour
{
    // This panel was made with the help of BigFoot Codes video: https://www.youtube.com/watch?v=texonivDsy0&ab_channel=BigfootCodes

    // ID of the panel.
    [SerializeField]
    string panelId;

    // Prefab of the panel.
    public GameObject panelPrefab;

    int heightInput;

    string inputddd;

    public void getInput(string input) {
        heightInput = Convert.ToInt32(input);
        Debug.Log(heightInput);
    }
}
