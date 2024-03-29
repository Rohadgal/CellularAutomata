using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomaton2D : MonoBehaviour
{
    [SerializeField]
    GameObject gridElement;

    int gridWidth = 0;
    int gridHeight = 0;
    int iterations = 1;
    int numAlive = 3;
    int numDead = 5;

    float prefabWidth;
    float prefabHeight;
    bool canGenerateCell;
    Vector3 topLeftCorner;

    bool m_isStepped;

    GameObject[,] cellsArray = new GameObject[0, 0];

    GameObject[,] cellsArrayOne = new GameObject[0, 0];

    private void Awake() {
        UpdatePosition();
    }

    private void Start() {
        // Check if cell has been defined and check and assign it's dimension
        if (gridElement != null) {
            prefabWidth = gridElement.GetComponent<SpriteRenderer>().bounds.size.x;
            prefabHeight = gridElement.GetComponent<SpriteRenderer>().bounds.size.y;
            return;
        }
        Debug.Log("Missing cell prefab");
    }

    void Update() {
        // Check if the screen size has changed and update the position
        if (Screen.width != transform.position.x || Screen.height != transform.position.y) {
            UpdatePosition();
        }
    }

    public void generateGrid() {
        if (canGenerateCell) {
            canGenerateCell = false;
            StopAllCoroutines();
        }

        // Check if matrix array size has not been defined and set a default size if it hasn�t.
        if (cellsArray.Length == 0) {
            if (gridWidth == 0 || gridHeight == 0) {
                gridWidth = 20;
                gridHeight = 20;
            }
            cellsArray = new GameObject[gridHeight, gridWidth];
        }

        // Resize array if it is not empty.
        if (cellsArray.Length != 0) {
            foreach (GameObject cell in cellsArray) {
                Destroy(cell);
            }
            Array.Clear(cellsArray, 0, cellsArray.Length);
            // somthing deleted
            ResizeMatrix(gridHeight, gridWidth);
        }

        if (gridWidth == 0) {
            gridWidth = 20;
        }
        if (gridHeight == 0) {
            gridHeight = 20;
        }
        canGenerateCell = true;
        StartCoroutine(createGridBySteps());
    }

    // Create a new matrix with the desired size
    void ResizeMatrix(int newRows, int newColumns) {
        GameObject[,] newMatrix = new GameObject[newRows, newColumns];
        cellsArray = newMatrix;
    }

    // Assign grid width
    public void getWidthInput(string input) {
        gridWidth = Convert.ToInt32(input);
    }
    // Assign grid height
    public void getHeightInput(string input) {
        gridHeight = Convert.ToInt32(input);
    }
    // Assign number of similar cells for alive ones to continue living
    public void getAliveNum(string input) {
        numAlive = Convert.ToInt32(input);
    }
    // Assign number of similar cells for dead ones to continue dead
    public void getDeadNum(string input) {
        numDead = Convert.ToInt32(input);
    }

    public void getIterations(string input) {
        iterations = Convert.ToInt32(input);
    }

    public void isStepped(bool input) {
        m_isStepped = input;
    }

    // Set the position to the top-left corner of the screen
    void UpdatePosition() {
        topLeftCorner = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
    }

    IEnumerator createGridBySteps() {
        for(int it = 0; it <= iterations; it++) {
            // Destroy and clear cells array for the next iteration
            yield return new WaitForSeconds(0.2f);
            if (it > 0) {
                copyArray();
                if (cellsArray.Length != 0) {
                    foreach (GameObject cell in cellsArray) {
                        Destroy(cell);
                    }
                    Array.Clear(cellsArray, 0, cellsArray.Length);
                }
            }
            for (int i = 0; i < gridHeight; i++) {
                for (int j = 0; j < gridWidth; j++) {
                    // Check if the cell matrix is empty before creating a new one
                    if (canGenerateCell) {
                        if(it == 0) {
                            bool randomValue = UnityEngine.Random.Range(0, 100) < 50;
                            // Create cell instance with random color value.
                            GameObject temp = Instantiate(gridElement, new Vector3((j * prefabWidth) + (topLeftCorner.x + prefabWidth), (topLeftCorner.y - prefabHeight) - (i * prefabHeight), 0), Quaternion.identity);
                            temp.GetComponent<Cell>().setCellColor(randomValue);

                            // Asign cell to a position in the matrix
                            cellsArray[i, j] = temp;
                        } else {
                            // Check neigbors
                            GameObject temp = Instantiate(gridElement, new Vector3((j * prefabWidth) + (topLeftCorner.x + prefabWidth), (topLeftCorner.y - prefabHeight) - (i * prefabHeight), 0), Quaternion.identity);
                            int numberOfNeighbors = checkNeighbors(i, j);
                            // If the cell is alive the color returns true
                            if (cellsArrayOne[i,j].GetComponent<Cell>().getColor()) {
                                // Check if rule is met for alive cells and asign color
                                temp.GetComponent<Cell>().setCellColor((numberOfNeighbors >= numAlive)?true : false);
                            }
                            else
                            {
                                // Check if rule is met for dead cells and asign color
                                temp.GetComponent<Cell>().setCellColor((numberOfNeighbors >= numDead) ? false : true);
                            }
                            // To keep cells in a closed grid
                            if(i == 0 || j == 0 || i == gridHeight -1|| j == gridWidth -1) {
                                temp.GetComponent<Cell>().setCellColor(false);
                            }
                            // Add temp cell to the original cells array
                            cellsArray[i,j] = temp;
                        }
                        // Create a little pause before drawing each individual cell on the matrix
                        if (m_isStepped) {
                            yield return new WaitForSeconds(0.02f);
                        }
                    }
                }
            }
            // Clear the cells from the secondary matrix array
            if (cellsArrayOne.Length != 0) {
                foreach (GameObject cell in cellsArrayOne) {
                    Destroy(cell);
                }
                Array.Clear(cellsArrayOne, 0, cellsArrayOne.Length);
            }
        }
    }

    int checkNeighbors(int x, int y) {
        int num = 0;
        // Exclude the edges of the grid from the math
        if(x== 0 || y == 0 || x == gridWidth - 1 || y == gridHeight - 1) {
            return 0;
        }
        // Go through the neighbors of the cell and check if they are the same color as the center cell
        for(int i = -1; i <= 1; i++) {
            for(int j = -1; j <= 1; j++) {
                if (cellsArrayOne[x + i, y + j].GetComponent<Cell>().getCellColor() == cellsArrayOne[x, y].GetComponent<Cell>().getCellColor()) {
                    num++;
                }
            }
        }
        // Minus one to ignore the center cell comparing with itself
        return num -1;
    }

    // Copy matrix array to check the condition of the next generation of cells without affecting the original matrix of cells
    void copyArray() {
        cellsArrayOne = new GameObject[cellsArray.GetLength(0), cellsArray.GetLength(1)];
        for(int i = 0; i < cellsArray.GetLength(0); i++) {
            for(int j = 0; j < cellsArray.GetLength(1); j++) {
                cellsArrayOne[i, j] = Instantiate(gridElement);
                // Set active to false to hide the temp gameObject from the game scene
                cellsArrayOne[i, j].SetActive(false);
                cellsArrayOne[i, j].GetComponent<Cell>().setCellColor(cellsArray[i,j].GetComponent<Cell>().getColor());
            }
        }
    }
}
