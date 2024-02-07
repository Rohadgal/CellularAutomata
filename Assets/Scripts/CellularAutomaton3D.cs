using System;
using System.Collections;
using System.Collections.Generic;
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
    int numAlive = 3;
    int numDead = 5;

    float prefabWidth;
    float prefabHeight;
    float prefabDepth;

    Vector3 cubeSize;
    bool canGenerateCell;
    bool hasChangedSize;
    bool canIterate;

    Vector3 topLeftCorner;

    bool m_isStepped = false;

    GameObject[,,] cellsArray = new GameObject[0, 0, 0];

    GameObject[,,] cellsArrayOne = new GameObject[0, 0, 0];

    bool[,,] cellsArrayMap = new bool[0, 0, 0];

    private void Start() {
        // Check if cell has been defined and check and assign it's dimension
        if (gridElement != null) {
            cubeSize = gridElement.GetComponent<Renderer>().bounds.size;
            prefabWidth = cubeSize.x;
            prefabHeight = cubeSize.y;
            prefabDepth = cubeSize.z;
            //generateGrid();
            return;
        }
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

    public void getNumAlive(string input) {
        numAlive = Convert.ToInt32(input);
    }

    public void getNumDead(string input) {
        numDead = Convert.ToInt32(input);
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
        int it = 0;

        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                for (int k = 0; k < gridDepth; k++) {
                    if (canGenerateCell) {
                        bool randomValue = UnityEngine.Random.Range(0, 100) < 50;
                        Vector3 cubePos = new Vector3(i - gridWidth * 0.5f, j - gridHeight * 0.5f, k - gridDepth * 0.5f);
                        GameObject temp = Instantiate(gridElement, cubePos, Quaternion.identity);
                        temp.GetComponent<CubeCell>().setCube(randomValue);
                        temp.transform.SetParent(transform);

                        // Asign cell to a position in the matrix
                        cellsArray[i, j, k] = temp;
                    }
                }
            }
        }
        copyArray(it);

        hasChangedSize = false;


        while (canIterate) {
            // Destroy and clear cells array for the next iteration
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < gridWidth; i++) {
                for (int j = 0; j < gridHeight; j++) {
                    for (int k = 0; k < gridDepth; k++) {
                        // Check if the cell matrix is empty before creating a new one
                        if (canGenerateCell) {
                            
                            // Check neigbors
                            //Vector3 cubePos = new Vector3(i - gridWidth * 0.5f, j - gridHeight * 0.5f, k - gridDepth * 0.5f);
                            //GameObject temp = Instantiate(gridElement, cubePos, Quaternion.identity);
                            int numberOfNeighbors = checkNeighbors(i, j, k);
                            // If the cell is alive returns true
                            if (cellsArrayMap[i, j, k]) {
                                // Check if rule is met for alive cells and asign color
                                cellsArray[i, j, k].GetComponent<CubeCell>().setCube((numberOfNeighbors >= numAlive) ? true : false);
                            } else {
                                // Check if rule is met for dead cells and asign color. 26 posible neighbors - the number of live neighbors needed if you are dead
                                cellsArray[i, j, k].GetComponent<CubeCell>().setCube((numberOfNeighbors >= 26 - numDead) ? false : true);
                            }
                            //// To keep cells in a closed grid
                            //if (i == 0 || j == 0 || i == gridHeight - 1 || j == gridWidth - 1) {
                            //    temp.GetComponent<Cell>().setCellColor(false);
                            //}
                            // Add temp cell to the original cells array
                            //cellsArray[i, j, k] = temp;
                            

                            // Create a little pause before drawing each individual cell on the matrix
                            if (m_isStepped) {
                                yield return new WaitForSeconds(0.02f);
                            }
                        }
                    }
                }
            }

            copyArray(it);

            hasChangedSize = false;

            if (it > 10) {
                it = 1;
            }
            it++;
    }
        //for (int it = 0; it <= iterations; it++) {  // change <= to < to do just once, then change again
           
        //}
}
    void copyArray(int iterator) {
        if (hasChangedSize) {
            Array.Clear(cellsArrayMap, 0, cellsArrayMap.Length);
            cellsArrayMap = new bool[cellsArray.GetLength(0), cellsArray.GetLength(1), cellsArray.GetLength(2)];
        }

        //if(iterator == 0) {
        //    cellsArrayOne = cellsArrayOne = new GameObject[cellsArray.GetLength(0), cellsArray.GetLength(1), cellsArray.GetLength(2)];
        //}


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
        if (x == 0 || y == 0 || z == 0 || x == gridWidth - 1 || y == gridHeight - 1 || z == gridDepth - 1) {
            return 0;
        }
        // Go through the neighbors of the cell and check if they are the same color as the center cell
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                for(int k = -1; k <= 1; k++) {
                    if (cellsArrayMap[x + i, y + j, z + k] == cellsArrayMap[x, y, z]) {
                        num++;
                    }
                }
            }
        }
        // Minus one to ignore the center cell comparing with itself
        return num - 1;
    }
}
