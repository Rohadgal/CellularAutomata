using System;
using System.Collections;
using UnityEngine;

public class CellularAutomaton : MonoBehaviour
{
    [SerializeField]
    GameObject gridElement;

    int gridWidth = 0;
    int gridHeight = 0;
    int rule;

    float prefabWidth;
    float prefabHeight;
    Vector3 initialPosition;
    Vector3 topLeftCorner;

    bool canGenerateCell;

    bool cellValue;

    GameObject[,] cellsArray = new GameObject[0,0];

    private void Awake() {
        UpdatePosition();
    }


    private void Start() {
        // UpdatePosition();
        //generateGrid();
        if(gridElement != null) {
            prefabWidth = gridElement.GetComponent<SpriteRenderer>().bounds.size.x;
            prefabHeight = gridElement.GetComponent<SpriteRenderer>().bounds.size.y;
            return;
        }
        Debug.Log("Missing cell prefab");
    }

    void UpdatePosition() {
        // Set the position to the top-left corner of the screen
        topLeftCorner = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
    }


    public void generateGrid() {

        if (canGenerateCell) {
            canGenerateCell = false;
            StopAllCoroutines();
        }

        //if (cells.Count != 0) {
        //    foreach(GameObject cell in cells) {
        //        Destroy(cell);
        //    }
        //    cells.Clear();
        //}

        // Check if array is empty.
        if (cellsArray.Length == 0) {
            if(gridWidth == 0 || gridHeight == 0) {
                gridWidth = 20;
                gridHeight = 20;
            }
            cellsArray = new GameObject[gridHeight, gridWidth];
        }

        // Resize array if it is not empty.
        if(cellsArray.Length != 0) {
            foreach(GameObject cell in cellsArray) {
                Destroy(cell);
            }
            GameObject[,] tempArray = new GameObject[gridHeight, gridWidth];
            ResizeMatrix(gridHeight, gridWidth);
        }

        if(gridWidth == 0) {
            gridWidth = 20;
        }
        if(gridHeight == 0) {
            gridHeight = 20;
        }
        canGenerateCell=true;
        StartCoroutine(createGridBySteps());
    }

    void ResizeMatrix(int newRows, int newColumns) {
        // Create a new matrix with the desired size
        GameObject[,] newMatrix = new GameObject[newRows, newColumns];

        // Copy elements from the original matrix to the new matrix
        //for (int i = 0; i < Mathf.Min(cellsArray.GetLength(0), newRows); i++) {
        //    for (int j = 0; j < Mathf.Min(cellsArray.GetLength(1), newColumns); j++) {
        //        newMatrix[i, j] = cellsArray[i, j];
        //    }
        //}

        // Update the original matrix reference
        cellsArray = newMatrix;
    }

    public void generateRule(string input) {
        rule = Convert.ToInt32(input);
        Debug.Log(rule);
    }

    public void getWidthInput(string input) {
        gridWidth = Convert.ToInt32(input);
        //if(gridWidth % 2 == 0) {
        //    gridWidth += 1;
        //}
    }

    public void getHeightInput(string input) {
        gridHeight = Convert.ToInt32(input);
    }

    IEnumerator createGridBySteps() {
        int middlePos = (int)(gridWidth * 0.5f);
        for (int i = 0; i < gridHeight; i++) {
            for (int j = 0; j < gridWidth; j++) {
                if(canGenerateCell) {
                    GameObject temp = Instantiate(gridElement, new Vector3((j * prefabWidth) + (topLeftCorner.x + prefabWidth),
                     (topLeftCorner.y - prefabHeight) - (i * prefabHeight), 0), Quaternion.identity);
                    cellsArray[i, j] = temp;
                    //cells.Add(Instantiate    
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
        
    }
}
