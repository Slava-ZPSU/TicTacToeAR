using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private PlaceBoard placeBoard;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private GameObject deleteButton;
    [SerializeField] private TextMeshProUGUI modeLabel;

    private void Start()
    {
        UpdateResetButton();
    }

    private void Update()
    {
        UpdateResetButton();
        deleteButton.SetActive(placeBoard.IsBoardSelected);
    }

    public void OnAddModeButtonClicked()
    {
        placeBoard.SetAddMode();
        modeLabel.text = "Add";
        UpdateResetButton();
    }

    public void OnEditModeButtonClicked()
    {
        placeBoard.SetEditMode();
        modeLabel.text = "Edit";
        UpdateResetButton();
    }

    public void OnResetModeButtonClicked()
    {
        placeBoard.SetNoneMode();
        modeLabel.text = "Game";
        UpdateResetButton();
    }

    private void UpdateResetButton()
    {
        if (placeBoard.CurrentMode == BoardMode.None) {
            resetButton.SetActive(false);
        } else {
            resetButton.SetActive(true);
        }
    }
}
