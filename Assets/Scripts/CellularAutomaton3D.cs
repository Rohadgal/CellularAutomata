using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class CellularAutomaton3D : MonoBehaviour
{
    [SerializeField]
    GameObject gridElement;

    int gridWidth = 0;
    int gridHeight = 0;
    int gridDepth = 0;

    int currentWidth;
    int currentHeight;
    int currentDepth;

    //int iterations = 3;
    int numSurvival = 3;
    int numBirth = 5;
    int state = 0;

    Vector3 cubeSize;
    bool canGenerateCell;
    bool hasChangedSize;
    bool canIterate;
    bool isDying;

    Vector3 topLeftCorner;

    bool m_isStepped = false;

    GameObject[,,] cellsArray = new GameObject[0, 0, 0];

    GameObject[,,] cellsArrayOne = new GameObject[0, 0, 0];

    bool[,,] cellsArrayMap = new bool[0, 0, 0];

    private void Start() {
        // Check if cell has been defined and check and assign it's dimension
        if (gridElement != null) {
            return;
        }
        Debug.Log("Missing Cube Prefab");
    }

    // Assign grid width
    public void getWidthInput(string input) {
        gridWidth = Convert.ToInt32(input);
    }
    // Assign grid height
    public void getHeightInput(string input) {
        gridHeight = Convert.ToInt32(input);
    }
    // Assign grid depth
    public void getHeightDepth(string input) {
        gridDepth = Convert.ToInt32(input);
    }

    public void getnumSurvival(string input) {
        numSurvival = Convert.ToInt32(input);
    }

    public void getnumBirth(string input) {
        numBirth = Convert.ToInt32(input);
    }

    public void getState(string input) {
        state = Convert.ToInt32(input);
    }

    bool isArrayEmpty() {
        return cellsArray.Length == 0;
    }

    public void isStepped(bool input) {
        m_isStepped = input;
    }

    public void checkCanIterate(bool input) {
        canIterate = input;
    }

    public void generateGrid() {
        if (canGenerateCell) {
            canGenerateCell = false;
            StopAllCoroutines();

            if (!isArrayEmpty()) {
                ResizeMatrix(gridWidth, gridHeight, gridDepth);
            }
        }

        //// Resize array if it is not empty and values of dimensions are different
        if (!isArrayEmpty()) {
            Debug.Log("aaa");
            if(gridWidth != currentWidth ||
            gridHeight != currentHeight ||
             gridDepth != currentDepth) {
                //Debug.Log("bbb");
                ResizeMatrix(gridWidth, gridHeight, gridDepth);
            }
        }

        if (gridWidth != 0 && gridHeight == 0 && gridDepth == 0) {
            gridHeight = gridWidth;
            gridDepth = gridWidth;
            ResizeMatrix(gridWidth, gridHeight, gridDepth);
        }else if (gridWidth == 0 && gridHeight != 0 && gridDepth == 0) {
            gridWidth = gridHeight;
            gridDepth = gridHeight;
            ResizeMatrix(gridWidth, gridHeight, gridDepth);
        }else if (gridWidth == 0 && gridHeight == 0 && gridDepth != 0) {
            gridWidth = gridDepth;
            gridHeight = gridDepth;
            ResizeMatrix(gridWidth, gridHeight, gridDepth);
        }else if (cellsArray.Length == 0) {
            if (gridWidth == 0 || gridHeight == 0 || gridDepth == 0) {
                gridWidth = 10;
                gridHeight = 10;
                gridDepth = 10;
            }
            ResizeMatrix(gridWidth, gridHeight, gridDepth);
        }
        currentWidth = gridWidth;
        currentHeight = gridHeight;
        currentDepth = gridDepth;

        canGenerateCell = true;
        StartCoroutine(createGridBySteps());
    }

    // Create a new matrix with the desired size
    void ResizeMatrix(int newRows, int newColumns, int newDepth) {
        hasChangedSize = true;
        foreach (GameObject cell in cellsArray) {
            Destroy(cell);
        }
        Array.Clear(cellsArray, 0, cellsArray.Length);
        GameObject[,,] newMatrix = new GameObject[newRows, newColumns, newDepth];
        cellsArray = newMatrix;
    }

    IEnumerator createGridBySteps() {
        int nameNum = 0;

        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                for (int k = 0; k < gridDepth; k++) {
                    if (canGenerateCell) {
                        bool randomValue = UnityEngine.Random.Range(0, 100) < 50;
                        Vector3 cubePos = new Vector3(i - gridWidth * 0.5f, j - gridHeight * 0.5f, k - gridDepth * 0.5f);
                        GameObject temp = Instantiate(gridElement, cubePos, Quaternion.identity);
                        temp.name = "Inst " + nameNum;
                        // set cube state num
                        temp.GetComponent<CubeCell>().setState(state);
                        // set cube render to active true or false depending on random value
                        temp.GetComponent<CubeCell>().setCube(randomValue);
                        // parent instance to parent empty gameObject pos
                        temp.transform.SetParent(transform);

                        // Asign cell to a position in the matrix
                        cellsArray[i, j, k] = temp;
                        
                        // Create a little pause before drawing each individual cell on the matrix
                        if (m_isStepped) {
                            yield return new WaitForSeconds(0.02f);
                        }
                        nameNum++;
                    }
                }
            }
        }
        // Copy the array values to a bool array to know if they are alive or dead
        copyArray();

        hasChangedSize = false;


        while (canIterate) {
            // Destroy and clear cells array for the next iteration
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < gridWidth; i++) {
                for (int j = 0; j < gridHeight; j++) {
                    for (int k = 0; k < gridDepth; k++) {
                        // Check if the cell matrix is empty before creating a new one
                        if (canGenerateCell) {
                            int numberOfNeighbors = checkNeighbors(i, j, k);
                            // If the cell is alive returns true
                            if (cellsArrayMap[i, j, k]) {
                                // Check if rule is met for alive cells and asign color
                                cellsArray[i, j, k].GetComponent<CubeCell>().setCube((numberOfNeighbors >= numSurvival) ? true : false);

                            } else { // Check if rule is met for dead cells and asign color. 26 posible neighbors - the number of live neighbors needed if you are dead
                                //Debug.Log("dead with neigbors amount: " + numberOfNeighbors);
                                // update state of dead cells
                                int tempState = cellsArray[i, j, k].GetComponent<CubeCell>().getState() - 1;
                                cellsArray[i, j, k].GetComponent<CubeCell>().setState(tempState);

                                // check for survival only of cell has completely disappeared
                                if (cellsArray[i, j, k].GetComponent <CubeCell>().getState() == 0) {
                                    // set cube to false to turn if off
                                   // Debug.LogWarning("died with neigbors amount: " + numberOfNeighbors);
                                    cellsArray[i, j, k].GetComponent<CubeCell>().setCube(false);
                                }

                                if(cellsArray[i, j, k].GetComponent<CubeCell>().getState() < 0) {
                                    // check if birth is possible with enough live cell neighbors
                                    cellsArray[i, j, k].GetComponent<CubeCell>().setCube((26 - numberOfNeighbors >= numBirth) ? true : false);
                                    // Restore state back to original value if cell is born again
                                    if (cellsArray[i, j, k].GetComponent<CubeCell>().getIsAlive()) {
                                        cellsArray[i, j, k].GetComponent<CubeCell>().setState(state);
                                        Debug.Log("Reborn with number of alive neighbors: " + (26 - numberOfNeighbors) + " when needed: " + numBirth);
                                        Debug.Log("name: " + cellsArray[i, j, k].name);
                                    } else {
                                        //Debug.LogWarning("stay dead");
                                    }
                                }

                                
                                
                            }

                            // Create a little pause before drawing each individual cell on the matrix
                            if (m_isStepped) {
                                yield return new WaitForSeconds(0.02f);
                            }
                        }
                    }
                }
            }

            copyArray();

            hasChangedSize = false;
        }
    }
    void copyArray() {
        //Debug.LogWarning("array copied");
        if (hasChangedSize) {
            Array.Clear(cellsArrayMap, 0, cellsArrayMap.Length);
            cellsArrayMap = new bool[cellsArray.GetLength(0), cellsArray.GetLength(1), cellsArray.GetLength(2)];
        }

        for (int i = 0; i < cellsArray.GetLength(0); i++) {
            for (int j = 0; j < cellsArray.GetLength(1); j++) {
                for(int k = 0; k < cellsArray.GetLength(2); k++) {
                    cellsArrayMap[i, j, k] = cellsArray[i,j,k].GetComponent<CubeCell>().getIsAlive();
                }
            }
        }
    }

    int checkNeighbors(int x, int y, int z) {
        int num = 0;
        // Exclude the edges of the grid from the math
        //if (x == 0 || y == 0 || z == 0 || x == gridWidth - 1 || y == gridHeight - 1 || z == gridDepth - 1) {
        //    return 0;
        //}
        // Go through the neighbors of the cell and check if they are the same color as the center cell
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                for (int k = -1; k <= 1; k++) {

                    if (x + i >= 0 && x + i < gridWidth &&
                    y + j >= 0 && y + j < gridHeight &&
                    z + k >= 0 && z + k < gridWidth) {
                        if (cellsArrayMap[x + i, y + j, z + k] == cellsArrayMap[x, y, z]) {
                            //Debug.LogWarning("neigbor: " + cellsArrayMap[x + i, y + j, z + k] + " Local: " + cellsArrayMap[x, y, z]);
                            num++;
                        }
                    }
                }
            }
        }
        // Minus one to ignore the center cell comparing with itself
        return num - 1;
    }
}
