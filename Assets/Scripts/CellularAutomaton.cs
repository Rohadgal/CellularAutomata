using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;



public class CellularAutomaton : MonoBehaviour
{
    public enum patternCases { one, two, three, four, five, six, seven, eight }

    [SerializeField]
    GameObject gridElement;

    int gridWidth = 0;
    int gridHeight = 0;
    int rule = 0;

    float prefabWidth;
    float prefabHeight;
    bool canGenerateCell;
    bool isRandomStart;
    Vector3 topLeftCorner;


    GameObject[,] cellsArray = new GameObject[0,0];
    bool[] boolBinaryArray = new bool[8];

    private void Awake() {
        UpdatePosition();
    }


    private void Start() {
        // Check if cell has been defined and check and assign it's dimension
        if(gridElement != null) {
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

    // Set the position to the top-left corner of the screen
    void UpdatePosition() {
        topLeftCorner = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
    }

    // Function to prevent grids from generating on top of another when generate button is clicked many times.
    public void generateGrid() {
        if (canGenerateCell) {
            canGenerateCell = false;
            StopAllCoroutines();
        }

        // Check if matrix array size has not been defined and set a default size if it hasn´t.
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
            Array.Clear(cellsArray, 0, cellsArray.Length);
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

    // Create a new matrix with the desired size
    void ResizeMatrix(int newRows, int newColumns) {
        GameObject[,] newMatrix = new GameObject[newRows, newColumns];
        cellsArray = newMatrix;
    }


    public void generateRule(string input) {
        rule = Convert.ToInt32(input);
        convertNumToBinary(rule);
    }

    // This functions creates an initial position in the first row of the matrix at random
    public void randomizeStart(bool input) {
        isRandomStart = input;
    }

    void convertNumToBinary(int num) {
        bool[] binArray = new bool[8];
        
        // Convert num to binary in boolean representation.
        for(int i = 7; i >= 0; i--) {
            // bitwise left shift operator done with the help of Chatgpt open.ai
            binArray[i] = (num &(1 << (7 - i))) != 0;
        }

        // Caching bool array
        for (int i = 0; i < binArray.Length; i++) {
            boolBinaryArray[i] = binArray[i];
        }
    }

    public void getWidthInput(string input) {
        gridWidth = Convert.ToInt32(input);
    }

    public void getHeightInput(string input) {
        gridHeight = Convert.ToInt32(input);
    }

    IEnumerator createGridBySteps() {
        int middlePos = (int)(gridWidth * 0.5f);
        int randomPos = UnityEngine.Random.Range(0, gridWidth);
        for (int i = 0; i < gridHeight; i++) {
            for (int j = 0; j < gridWidth; j++) {
                // Check if the cell matrix is empty before creating a new one
                if(canGenerateCell) {
                    bool[] patternSpace = new bool[3];
                    
                    // Create cell instance.
                    GameObject temp = Instantiate(gridElement, new Vector3((j * prefabWidth) + (topLeftCorner.x + prefabWidth), (topLeftCorner.y - prefabHeight) - (i * prefabHeight), 0), Quaternion.identity);
                    // Change color of center cell in first row if random position is not selected
                    if (!isRandomStart) {
                        if(i == 0 && j == middlePos) {
                            if(temp.GetComponent<Cell>().getColor()) {
                                temp.GetComponent<Cell>().setCellColor(false);
                            }
                        }
                    } else { 
                        if (i == 0 && j == randomPos) {
                            if (temp.GetComponent<Cell>().getColor()) {
                                temp.GetComponent<Cell>().setCellColor(false);
                            }
                        }
                    }
                    // Asign cell to a position in the matrix
                    cellsArray[i, j] = temp;  
                    // Begin assigning color to cells based on the cells on the row above and the rule selected only after the first row has been created
                    if(i > 0 && rule != 0) {
                        // check the condition of the three cells on the row above the cell being revised
                        for(int k = 0; k < 3; k++) {
                            // Always set to white the first cell of the pattern space when checking the first column
                            if (j == 0 && k==0) {
                                patternSpace[k] = true;
                                continue;
                            }
                            if (j == 0) {
                                // Assign color to the second or third cell of the pattern space when working with the first column
                                patternSpace[k] = cellsArray[i - 1, k - 1].GetComponent<Cell>().getColor();
                                continue;
                            }

                            if(k == 0) {
                                // Assign color to the first cell of the pattern space when working with the rest of cells of the matrix
                                patternSpace[k] = cellsArray[i - 1, j - 1].GetComponent<Cell>().getColor();
                            }

                            if(k == 1) {
                                // Assign color to the second cell of the pattern space when working with the rest of cells of the matrix
                                patternSpace[k] = cellsArray[i - 1, j].GetComponent<Cell>().getColor();
                            }

                            if (k == 2) {
                                // Always set to white the last cell of the pattern space when checking the last column
                                if (j == cellsArray.GetLength(1) - 1) {
                                    patternSpace[k] = true;
                                    continue;
                                }
                                // Assign color to the last cell of the pattern space
                                patternSpace[k] = cellsArray[i - 1, j + 1].GetComponent<Cell>().getColor();
                            }
                        }
                        // check pattern space for color and assign color to cell.
                        cellsArray[i,j].GetComponent<Cell>().setCellColor(!checkCellArrayColors(patternSpace));
                    }
                    // Clear the array that checks the condition of the three positions on top of the cell in turn.
                    Array.Clear(patternSpace,0,patternSpace.Length);
                    // Create a little pause before drawing each individual cell on the matrix
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
    }

    // Asign color to cell on every row depending on rule condition
    bool checkCellArrayColors(bool[] boolSectionArray) {
        patternCases _case = new patternCases();
        bool asignedColor = false;

        // Check the bools in the array of the pattern space to determine one of the eight enum cases
        // Condition that checks the cases that start with false bool
        if (boolSectionArray[0] == false) {
            if (boolSectionArray[1] == false) {
                if (boolSectionArray[2] == false) {
                    _case = patternCases.one;
                    goto caseSelection;
                }
                _case = patternCases.two;
                goto caseSelection;
            }
            if (boolSectionArray[2] ==false) {
                _case = patternCases.three;
                goto caseSelection;
            }
            _case = patternCases.four;
        }
        // Condition that checks the cases that start with true bool
        if (boolSectionArray[0] == true) {
            if (boolSectionArray[1] == true) {
                if (boolSectionArray[2] == true) {
                    _case = patternCases.eight;
                    goto caseSelection;
                }
                _case = patternCases.seven;
                goto caseSelection;
            }
            if (boolSectionArray[2] == true) {
                _case = patternCases.six;
                goto caseSelection;
            }
            _case= patternCases.five;
        }

        caseSelection:

        // Asign color to cell depending con the condition of the pattern space above the cell
        switch (_case) {
            case patternCases.one: 
                asignedColor = boolBinaryArray[0];
            break;

            case patternCases.two:
                asignedColor = boolBinaryArray[1];
            break;
        
            case patternCases.three:
                asignedColor = boolBinaryArray[2];
            break;
        
            case patternCases.four:
                asignedColor = boolBinaryArray[3];
            break;
        
            case patternCases.five:
                asignedColor = boolBinaryArray[4];
            break;
        
            case patternCases.six:
                asignedColor = boolBinaryArray[5];
            break;
        
            case patternCases.seven:
                asignedColor = boolBinaryArray[6];
            break;
        
            case patternCases.eight:
                asignedColor = boolBinaryArray[7];
            break;

            default: break;
        }
        return asignedColor;
    }
}
