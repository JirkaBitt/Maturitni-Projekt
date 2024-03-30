using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTools : MonoBehaviour
{
    //references to the buttons
    public GameObject eraseButton;
    public GameObject pencilButton;
    public GameObject submitButton;
    public GameObject remakeButton;
    public GameObject startGameButton;
    //reference to the analyze image script
    private AnalyzeImage controllerScript;
    void Start()
    {
        controllerScript = GameObject.Find("controller").GetComponent<AnalyzeImage>();
        
        Button erase = eraseButton.GetComponent<Button>();
        erase.onClick.AddListener(ChangeToEraser);

        Button pencil = pencilButton.GetComponent<Button>();
        pencil.onClick.AddListener(ChangeToPencil);

        Button submit = submitButton.GetComponent<Button>();
        submit.onClick.AddListener(MoveToNext);
       
        Button remake = remakeButton.GetComponent<Button>();
        remake.onClick.AddListener(Remake);

        Button startGame = startGameButton.GetComponent<Button>();
        startGame.onClick.AddListener(StartGameScript);
    }

    // Update is called once per frame
    void Update()
    {
        AdjustToolCursor();
    }

    void ChangeToPencil()
    {
        controllerScript.currentTool = "Pencil";
    }

    void ChangeToEraser()
    {
        controllerScript.currentTool = "Eraser";
    }

    void MoveToNext()
    {
        controllerScript.MoveToTheNext();
    }

    void Remake()
    {
        controllerScript.Remake();
    }

    public void ChangeFunctions()
    {
        //we have displayed the assets and now change the functions of these buttons to modify them
        submitButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Save");
        Button submit = submitButton.GetComponent<Button>();
        submit.onClick.RemoveAllListeners();
        submit.onClick.AddListener(SaveChange);

        Button remake = remakeButton.GetComponent<Button>();
        remake.onClick.RemoveAllListeners();
        remake.onClick.AddListener(RemakeClicked);
    }

    public void DeactivateButtons()
    {
        submitButton.SetActive(false);
        remakeButton.SetActive(false);
        pencilButton.SetActive(false);
        eraseButton.SetActive(false);
    }

    public void ActivateButtons()
    {
        submitButton.SetActive(true);
        remakeButton.SetActive(true);
        pencilButton.SetActive(true);
        eraseButton.SetActive(true);
    }
    void SaveChange()
    {
        controllerScript.SubmitChange();
    }
    void RemakeClicked()
    {
        controllerScript.RemakeClicked();
    }
    void StartGameScript()
    {
        controllerScript.StartGame();
    }
    public void EnableStartGame()
    {
        startGameButton.SetActive(true);
    }
    public void DisableStartGame()
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
