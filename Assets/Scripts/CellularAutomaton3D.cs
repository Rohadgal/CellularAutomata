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

    //int iterations = 3;
    int numSurvival = 3;
    int numBirth = 5;
    int state = 0;
    int neighborsNumSurvive;
    int neighborsNumBirth;

    bool canGenerateCell;
    bool hasChangedSize;
    bool canIterate;
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
        bool previousStateOfIteration = canIterate;

        StopAllCoroutines();

        if(canIterate) {
            Debug.Log("canIterate: " + canIterate);
            canIterate = false;
        }

        foreach (GameObject cell in cellsArray) {
            Destroy(cell);
        }

        //if (hasChangedSize) {
        //    ResizeMatrix(gridWidth, gridHeight, gridDepth);
        //    return;
        //}

        if(previousStateOfIteration) {
            canIterate = previousStateOfIteration;
            Debug.Log("reset canIterate to true: " + canIterate);
        }
        
    }

    // Create a new matrix with the desired size
    void ResizeMatrix(int newRows, int newColumns, int newDepth) {

        // Clear the array of prefabs
        Array.Clear(cellsArray, 0, cellsArray.Length);
        // Create new matrix with new dimensions
        cellsArray = new GameObject[newRows, newColumns, newDepth];
        // Clear the array of bools that map the alive or dead condition of the prefabs
        Array.Clear(cellsArrayMap, 0, cellsArrayMap.Length);
        // Create new matrix of bools with the new dimensions of the prefab's matrix
        cellsArrayMap = new bool[cellsArray.GetLength(0), cellsArray.GetLength(1), cellsArray.GetLength(2)];
        // Set bool to false because resizing has finished
        hasChangedSize = false;
        Debug.Log("changed size x: " + newRows + " y: " + newColumns + " z: " + newDepth);
    }

    // Function to create the matrix of prefabs with specified dimensions and in the manner that specific conditions allow it
    public void generateGrid() {
        //canBreakWhile = false;
        if(!canGenerateCell) {
            cellsArray = new GameObject[gridWidth, gridHeight, gridDepth];
            cellsArrayMap = new bool[cellsArray.GetLength(0), cellsArray.GetLength(1), cellsArray.GetLength(2)];
            hasChangedSize = false;
        }

        // Condition to allow creation of matrix when button pressed even if the previous matrix generation is in process
        if (canGenerateCell) {
            canGenerateCell = false;
            //StopAllCoroutines();

            clear();

            if (!isArrayEmpty() && hasChangedSize) {
                ResizeMatrix(gridWidth, gridHeight, gridDepth);
                Debug.Log("resize gen");
            }
        }

        //// Resize array if it is not empty and values of dimensions are different
        //if (hasChangedSize) {
        //    Debug.Log("array resized in grid generation");
        //    ResizeMatrix(gridWidth, gridHeight, gridDepth);
        //}
        if (isArrayEmpty()) {

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
        }

        canGenerateCell = true;
        StartCoroutine(createGridBySteps());
    }

   

    // Coroutine to create matrix of cubes with small breaks of time
    IEnumerator createGridBySteps() {
        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                for (int k = 0; k < gridDepth; k++) {
                    if (canGenerateCell) {
                        setRandomCubes(i, j, k);
                        // Create a little pause before drawing each individual cell on the matrix
                        if (m_isStepped) {
                            yield return new WaitForSeconds(0.02f);
                        }
                    }
                }
            }
        }
        // Copy the array values of the random initial cubes to a bool array to know if they are alive or dead
        copyArray();

        while (canIterate) {
            // Wait a determined number of seconds before changing the state of all the cubes
            yield return new WaitForSeconds(0.5f);
            // Create a tridimensional model made out of cubes
            for (int i = 0; i < gridWidth; i++) {
                for (int j = 0; j < gridHeight; j++) {
                    for (int k = 0; k < gridDepth; k++) {
                        // Check if the cell matrix is empty before creating a new one
                        if (canGenerateCell) {
                            if (isMoore) {
                                // Check direct neighbors in all directions incluiding diagonals
                                checkNeighborsMoore(i, j, k);
                            } else {
                                // Check direct neighbors in all direction without incluiding diagonals
                                checkNeighborsVonNeumann(i, j, k);
                            }
                            // Set color and visibility of cubes depending on rule and conditions - Num of neighbors depending on Moore or Von Neumann Model
                            setCubes(i, j, k);
                            // Create a little pause before drawing each individual cell on the matrix
                            if (m_isStepped) {
                                yield return new WaitForSeconds(0.02f);
                            }
                        }
                    }
                }
            }
            // Copy the array values to a bool array to know if they are alive or dead
            copyArray();
            Debug.Log("iteration");
        }
    }

    // Function that set the first 3x3 matrix of cubes in a random disposition
    void setRandomCubes(int i, int j, int k) {
        bool randomValue = UnityEngine.Random.Range(0, 100) < 50;
        Vector3 cubePos = new Vector3(i - gridWidth * 0.5f, j - gridHeight * 0.5f, k - gridDepth * 0.5f);
        GameObject temp = Instantiate(gridElement, cubePos, Quaternion.identity);
        // set cube state num
        temp.GetComponent<CubeCell>().setState(state);
        // set cube render to active true or false depending on random value
        temp.GetComponent<CubeCell>().setCube(randomValue);
        // parent instance to parent empty gameObject pos
        temp.transform.SetParent(transform);
        // Asign cell to a position in the matrix
        cellsArray[i, j, k] = temp;
    }

    // Function that sets the cubes depending on the number of alive neighbors
    void setCubes(int i, int j, int k) {
        // If the cell is alive returns true
        if (cellsArrayMap[i, j, k]) {
            // Check if rule is met for alive cells and asign color
            cellsArray[i, j, k].GetComponent<CubeCell>().setCube((neighborsNumSurvive >= numSurvival) ? true : false);
            return;
        }
        // Check if rule is met for dead cells and asign color. 26 posible neighbors - the number of live neighbors needed if you are dead
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
            cellsArray[i, j, k].GetComponent<CubeCell>().setCube((neighborsNumBirth >= numBirth) ? true : false);
            // Restore state back to original value if cell is born again
            if (cellsArray[i, j, k].GetComponent<CubeCell>().getIsAlive()) {
                cellsArray[i, j, k].GetComponent<CubeCell>().setState(state);
            } 
        }
    }

    void copyArray() {
        //if (hasChangedSize) {
        //    ResizeMatrix(gridWidth, gridHeight, gridDepth);
        //}
        for (int i = 0; i < cellsArray.GetLength(0); i++) {
            for (int j = 0; j < cellsArray.GetLength(1); j++) {
                for(int k = 0; k < cellsArray.GetLength(2); k++) {
                    cellsArrayMap[i, j, k] = cellsArray[i,j,k].GetComponent<CubeCell>().getIsAlive();
                }
            }
        }
    }
    // Function to check the number of alive neighbors in accordance with Moore's Model
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
                    z + k >= 0 && z + k < gridDepth) {
                        // check if cell is alive
                        if(cellsArrayMap[x, y, z]) { 
                            // Check how many alive neighbors a live cell has
                            if (cellsArrayMap[x + i, y + j, z + k]) {
                                neighborsNumSurvive++;
                                continue;
                            }
                        }
                        // Check how many alive neighbors a dead cell has
                        Debug.LogWarning("ArrayMap it values  x: " + (x + i) + " y: " + (y + j) + " z: " + (z + k));
                        if (cellsArrayMap[x + i, y + j, z + k]) {
                            neighborsNumBirth++;
                        }
                    }
                }
            }
        }
    }
    // Function to check the number of alive neighbors in accordance with Von Neumann's Model
    void checkNeighborsVonNeumann(int x, int y, int z){
        // Reset values for neighborsNumSurvive and neighborsNumBirth for each new iteration
        neighborsNumSurvive = 0;
        neighborsNumBirth = 0;
        // Define offset to check neighbors only in accordance to Von Neumann's Model
        int[] xOffSet = {-1, 1, 0, 0, 0, 0};
        int[] yOffSet = { 0, 0,-1, 1, 0, 0};
        int[] zOffSet = { 0, 0, 0, 0,-1, 1};
        for (int i = 0; i < xOffSet.Length; i++){
            int tempX = xOffSet[i];
            int tempY = yOffSet[i];
            int tempZ = zOffSet[i];
            // Conditional to stay inside the bounds of the matrix
            if(x + tempX >= 0 && x + tempX < gridWidth &&
               y + tempY >= 0 && y + tempY < gridHeight &&
               z + tempZ >= 0 && z + tempZ < gridDepth){
                // Check if cell is alive
                if(cellsArrayMap[x, y, z]) {
                    // Check if neighbors are alive
                    if (cellsArrayMap[x + tempX, y + tempY, z + tempZ]) {
                        neighborsNumSurvive++;
                        continue;
                    }
                } 
                // Check alive neighbors if cell is dead
                if (cellsArrayMap[x + tempX, y + tempY, z + tempZ]) {
                    neighborsNumBirth++;
                }
            }
        }
    }
}
