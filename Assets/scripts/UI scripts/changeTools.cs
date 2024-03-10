using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class changeTools : MonoBehaviour
{
    //references to the buttons
    public GameObject eraseButton;
    public GameObject pencilButton;
    public GameObject submitButton;
    public GameObject remakeButton;
    public GameObject startGameButton;
    //reference to the analyze image script
    private analyzeImage controllerScript;
    void Start()
    {
        controllerScript = GameObject.Find("controller").GetComponent<analyzeImage>();
        
        Button erase = eraseButton.GetComponent<Button>();
        erase.onClick.AddListener(changeToEraser);

        Button pencil = pencilButton.GetComponent<Button>();
        pencil.onClick.AddListener(changeToPencil);

        Button submit = submitButton.GetComponent<Button>();
        submit.onClick.AddListener(moveToNext);
       
        Button remake = remakeButton.GetComponent<Button>();
        remake.onClick.AddListener(Remake);

        Button startGame = startGameButton.GetComponent<Button>();
        startGame.onClick.AddListener(startGameScript);
    }

    // Update is called once per frame
    void Update()
    {
        AdjustToolCursor();
    }

    void changeToPencil()
    {
        controllerScript.currentTool = "Pencil";
    }

    void changeToEraser()
    {
        controllerScript.currentTool = "Eraser";
    }

    void moveToNext()
    {
        controllerScript.moveToTheNext();
    }

    void Remake()
    {
        controllerScript.remake();
    }

    public void changeFunctions()
    {
        //we have displayed the assets and now change the functions of these buttons to modify them
        submitButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Save");
        Button submit = submitButton.GetComponent<Button>();
        submit.onClick.RemoveAllListeners();
        submit.onClick.AddListener(saveChange);

        Button remake = remakeButton.GetComponent<Button>();
        remake.onClick.RemoveAllListeners();
        remake.onClick.AddListener(remakeClicked);
    }

    public void deactivateButtons()
    {
        submitButton.SetActive(false);
        remakeButton.SetActive(false);
        pencilButton.SetActive(false);
        eraseButton.SetActive(false);
    }

    public void activateButtons()
    {
        submitButton.SetActive(true);
        remakeButton.SetActive(true);
        pencilButton.SetActive(true);
        eraseButton.SetActive(true);
    }
    void saveChange()
    {
        controllerScript.submitChange();
    }
    void remakeClicked()
    {
        controllerScript.remakeClicked();
    }
    void startGameScript()
    {
        controllerScript.startGame();
    }
    public void enableStartGame()
    {
        startGameButton.SetActive(true);
    }
    public void disableStartGame()
    {
        startGameButton.SetActive(false);
    }
    //adjust tool cursor is called in update and checks for user input to change the radius of the tool based on the input
    public void AdjustToolCursor()
    {
        float mouseInput = Input.GetAxis("Mouse ScrollWheel");
        float toolRadius = controllerScript.toolRadius;
        toolRadius += mouseInput;
        if (toolRadius < 0)
        {
            toolRadius = 0;
        }
        //the max value is set to 2
        if (toolRadius > 2)
        {
            toolRadius = 2;
        }
        // reassign the value to the controller
        controllerScript.toolRadius = toolRadius;
    }
}
