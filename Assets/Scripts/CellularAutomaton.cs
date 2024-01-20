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
    Vector3 topLeftCorner;

    bool canGenerateCell;

    GameObject[,] cellsArray = new GameObject[0,0];

    //int[] ruleBinaryArray = new int[8];
    bool[] boolBinaryArray = new bool[8];

    private void Awake() {
        UpdatePosition();
    }


    private void Start() {
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

    void UpdatePosition() {
        // Set the position to the top-left corner of the screen
        topLeftCorner = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
    }

    public void generateGrid() {
        if (canGenerateCell) {

            canGenerateCell = false;
            StopAllCoroutines();
        }

        //if(cellsArray != null) {
        //    Array.Clear(cellsArray, 0, cellsArray.Length);
        //}

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

    void ResizeMatrix(int newRows, int newColumns) {
        // Create a new matrix with the desired size
        GameObject[,] newMatrix = new GameObject[newRows, newColumns];
        cellsArray = newMatrix;
    }

    public void generateRule(string input) {
        rule = Convert.ToInt32(input);
        //Debug.Log(rule);
        convertNumToBinary(rule);
    }

    public void convertNumToBinary(int num) {
        bool[] binArray = new bool[8];
        
        // Convert num to binary in boolean representation.
        for(int i = 7; i >= 0; i--) {
            // bitwise left shift operator
            binArray[i] = (num &(1 << (7 - i))) != 0;
        }

        // Convert bool array to binary representation.
        //for(int i = 0; i <binArray.Length; i++) {
        //    if (binArray[i]) {
        //        ruleBinaryArray[i] = 1;
        //        continue;
        //    }
        //    ruleBinaryArray[i] = 0;
        //}

        // Caching bool array
        for (int i = 0; i < binArray.Length; i++) {
            boolBinaryArray[i] = binArray[i];
            Debug.Log(" " + boolBinaryArray[i] + " ");
        }
        //for (int i = 0; i < ruleBinaryArray.Length; i++) {
        //    Debug.Log(" " + ruleBinaryArray[i] + " ");
        //}
    }

    public void getWidthInput(string input) {
        gridWidth = Convert.ToInt32(input);
        // To create a center position in grid's width.
        //if(gridWidth % 2 == 0) {
        //    gridWidth += 1;
        //}
    }

    public void getHeightInput(string input) {
        gridHeight = Convert.ToInt32(input);
    }

    IEnumerator createGridBySteps() {
        int middlePos = (int)(gridWidth * 0.5f);
        //GameObject[] cellArray = new GameObject[ruleBinaryArray.Length];

        for (int i = 0; i < gridHeight; i++) {
            for (int j = 0; j < gridWidth; j++) {
                if(canGenerateCell) {
                    bool[] patternSpace = new bool[3];
                    
                    // Create cell instance.
                    GameObject temp = Instantiate(gridElement, new Vector3((j * prefabWidth) + (topLeftCorner.x + prefabWidth), (topLeftCorner.y - prefabHeight) - (i * prefabHeight), 0), Quaternion.identity);
                    // Change color of center cell in first row.
                    if(i == 0 && j == middlePos) {
                        //temp.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1);
                        if(temp.GetComponent<Cell>().getColor()) {
                            temp.GetComponent<Cell>().setCellColor(false);
                        }
                    }
                    cellsArray[i, j] = temp;  

                    if(i > 0 && rule != 0) {
                        for(int k = 0; k < 3; k++) {
                            if (j == 0 && k==0) {
                                patternSpace[k] = true;
                                //Debug.Log("cell_ x: " + i + " y: " + j + " bool[" + k + "] :" + patternSpace[k]);
                                continue;
                            }
                            if (j == 0) {
                                patternSpace[k] = cellsArray[i - 1, k - 1].GetComponent<Cell>().getColor();
                                //Debug.Log("cell_ x: " + i + " y: " + j + " bool[" + k + "] :" + patternSpace[k]);

                                //if(k == 2) {
                                //   cellsArray[i, j].GetComponent<Cell>().setCellColor(checkCellArrayColors(patternSpace));
                                //}
                                continue;
                            }

                            if(k == 0) {
                                patternSpace[k] = cellsArray[i - 1, j - 1].GetComponent<Cell>().getColor();
                            }

                            if(k == 1) {
                                patternSpace[k] = cellsArray[i - 1, j].GetComponent<Cell>().getColor();
                            }

                            if (k == 2) {
                                if(j == cellsArray.GetLength(1) - 1) {
                                    patternSpace[k] = true;

                                    //cellsArray[i, j].GetComponent<Cell>().setCellColor(checkCellArrayColors(patternSpace));
                                    //Debug.Log("cell_ x: " + i + " y: " + j + " bool[" + k + "] :" + patternSpace[k]);
                                    continue;
                                }
                                patternSpace[k] = cellsArray[i - 1, j + 1].GetComponent<Cell>().getColor();
                            }
                            //Debug.Log("cell_ x: " + i + " y: " +j + " bool[" + k + "] :" + patternSpace[k]);
                        }
                        // check pattern space for color and assign color to cell.
                        cellsArray[i,j].GetComponent<Cell>().setCellColor(!checkCellArrayColors(patternSpace));
                        Debug.Log("Cell x: " + i + " y: " + j);
                    }

                    Array.Clear(patternSpace,0,patternSpace.Length);

                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
    }

    bool checkCellArrayColors(bool[] boolSectionArray) {
        patternCases _case = new patternCases();
        bool asignedColor = false;

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

        Debug.Log("case: " + _case.ToString());
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
