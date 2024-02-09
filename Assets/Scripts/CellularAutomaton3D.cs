using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
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
    int neighborsNumSurvive;
    int neighborsNumBirth;

    bool canGenerateCell;
    bool hasChangedSize;
    bool canIterate;
    bool canBreakWhile;
    bool m_isStepped;
    bool isMoore;

    GameObject[,,] cellsArray = new GameObject[0, 0, 0];

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
        hasChangedSize = true;
    }
    // Assign grid height
    public void getHeightInput(string input) {
        gridHeight = Convert.ToInt32(input);
        hasChangedSize = true;
    }
    // Assign grid depth
    public void getHeightDepth(string input) {
        gridDepth = Convert.ToInt32(input);
        hasChangedSize = true;
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

    public void checkIsMoore(bool input) {
        isMoore = input;
    }

    public void clear() {
        StopAllCoroutines();
       
        ResizeMatrix(gridWidth, gridHeight, gridDepth);
        //canBreakWhile = true;
        
    }

    public void generateGrid() {
        //canBreakWhile = false;

        if (canGenerateCell) {
            canGenerateCell = false;
            StopAllCoroutines();

            if (!isArrayEmpty() && hasChangedSize) {
                ResizeMatrix(gridWidth, gridHeight, gridDepth);
            }
            clear();
        }

        //// Resize array if it is not empty and values of dimensions are different
        if (hasChangedSize) {
            Debug.Log("aaa");
            ResizeMatrix(gridWidth, gridHeight, gridDepth);
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

        canGenerateCell = true;
        StartCoroutine(createGridBySteps());
    }

    // Create a new matrix with the desired size
    void ResizeMatrix(int newRows, int newColumns, int newDepth) {
       
        foreach (GameObject cell in cellsArray) {
            Destroy(cell);
        }
        Array.Clear(cellsArray, 0, cellsArray.Length);
        cellsArray = new GameObject[newRows, newColumns, newDepth];
        Array.Clear(cellsArrayMap, 0, cellsArrayMap.Length);
        cellsArrayMap = new bool[cellsArray.GetLength(0), cellsArray.GetLength(1), cellsArray.GetLength(2)];
        hasChangedSize = false;
    }

    IEnumerator createGridBySteps() {
        int itt = 0;
        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                for (int k = 0; k < gridDepth; k++) {
                    if (canGenerateCell) {
                        bool randomValue = UnityEngine.Random.Range(0, 100) < 50;
                        Vector3 cubePos = new Vector3(i - gridWidth * 0.5f, j - gridHeight * 0.5f, k - gridDepth * 0.5f);
                        GameObject temp = Instantiate(gridElement, cubePos, Quaternion.identity);
                        temp.name = "Instance_" + itt;
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
                        itt++;
                    }
                }
            }
        }
        // Copy the array values to a bool array to know if they are alive or dead
        copyArray();

        while (canIterate) {
            //if (canBreakWhile) {
            //    canIterate = false;
            //}
            // Destroy and clear cells array for the next iteration
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < gridWidth; i++) {
                for (int j = 0; j < gridHeight; j++) {
                    for (int k = 0; k < gridDepth; k++) {
                        // Check if the cell matrix is empty before creating a new one
                        if (canGenerateCell) {
                            if (isMoore) {
                                checkNeighborsMoore(i, j, k);
                            } else {
                                checkNeighborsVonNeumann(i, j, k);
                            }

                            // If the cell is alive returns true
                            if (cellsArrayMap[i, j, k]) {
                                // Check if rule is met for alive cells and asign color
                                Debug.Log("Num survive: " + neighborsNumSurvive);
                                cellsArray[i, j, k].GetComponent<CubeCell>().setCube((neighborsNumSurvive >= numSurvival) ? true : false);
                            } else { // Check if rule is met for dead cells and asign color. 26 posible neighbors - the number of live neighbors needed if you are dead
                                // Update the state number
                                int tempState = cellsArray[i, j, k].GetComponent<CubeCell>().getState() - 1;
                                // Assign updated state nmber
                                cellsArray[i, j, k].GetComponent<CubeCell>().setState(tempState);

                                // check if state number has reached zero to turn off the mesh renderer
                                if (cellsArray[i, j, k].GetComponent<CubeCell>().getState() == 0) {
                                    // set cube to false to turn if off
                                    cellsArray[i, j, k].GetComponent<CubeCell>().setCube(false);
                                }

                                if (cellsArray[i, j, k].GetComponent<CubeCell>().getState() < 0) {
                                    // check if birth is possible with enough live cell neighbors
                                    Debug.Log("Num birth: " + neighborsNumBirth);
                                    cellsArray[i, j, k].GetComponent<CubeCell>().setCube((neighborsNumBirth >= numBirth) ? true : false);

                                    // Restore state back to original value if cell is born again
                                    if (cellsArray[i, j, k].GetComponent<CubeCell>().getIsAlive()) {
                                        cellsArray[i, j, k].GetComponent<CubeCell>().setState(state);
                                        Debug.LogWarning("born with: " + neighborsNumBirth);
                                        //Debug.Log("Reborn with number of alive neighbors: " + (neighborsNumBirth) + " when needed: " + numBirth);
                                        // Debug.Log("name: " + cellsArray[i, j, k].name);
                                    } else {
                                        Debug.LogWarning(cellsArray[i, j, k].name + " stayed dead with: " + neighborsNumBirth);
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

    void checkNeighborsMoore(int x, int y, int z) {
        // Reset values for neighborsNumSurvive and neighborsNumBirth for each new iteration
        // Values are set to -1 to ignore the addition of the cell considering itself
        neighborsNumSurvive = -1;
        neighborsNumBirth = -1;
   
        // Go through the neighbors of the cell
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                for (int k = -1; k <= 1; k++) {
                    // Procure to stay inside the bounds of the matrix
                    if (x + i >= 0 && x + i < gridWidth &&
                    y + j >= 0 && y + j < gridHeight &&
                    z + k >= 0 && z + k < gridWidth) {
                        // check if cell is alive
                        if(cellsArrayMap[x, y, z]) { 
                            // Check how many alive neighbors a live cell has
                            if (cellsArrayMap[x + i, y + j, z + k]) {
                                neighborsNumSurvive++;
                                continue;
                            }
                        }
                        // Check how many alive neighbors a dead cell has
                        if (cellsArrayMap[x + i, y + j, z + k]) {
                            neighborsNumBirth++;
                        }
                       
                    }
                }
            }
        }
    }

    void checkNeighborsVonNeumann(int x, int y, int z){
        neighborsNumSurvive = 0;
        neighborsNumBirth = 0;

        int[] xOffSet = {-1, 1, 0, 0, 0, 0};
        int[] yOffSet = { 0, 0,-1, 1, 0, 0};
        int[] zOffSet = { 0, 0, 0, 0,-1, 1};

        for (int i = 0; i < xOffSet.Length; i++){
            int tempX = xOffSet[i];
            int tempY = yOffSet[i];
            int tempZ = zOffSet[i];

            if(x + tempX >= 0 && x + tempX < gridWidth &&
               y + tempY >= 0 && y + tempY < gridHeight &&
               z + tempZ >= 0 && z + tempZ < gridDepth){
                // Check if cell is alive
                if(cellsArrayMap[x, y, z]) {
                    // Check if neighbors are alive
                    Debug.LogWarning("thas");
                    if (cellsArrayMap[x + tempX, y + tempY, z + tempZ]) {
                        Debug.LogWarning("this");
                        neighborsNumSurvive++;
                        continue;
                    }
                } 
                if (cellsArrayMap[x + tempX, y + tempY, z + tempZ]) {
                    neighborsNumBirth++;
                    Debug.LogWarning("dead cell with num of alive neigh: " + neighborsNumBirth);
                }
            }
        }
    }
}
