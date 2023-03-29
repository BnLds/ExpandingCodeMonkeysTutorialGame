using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ControlOptionSingleButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI controlOptionText;
    [SerializeField] private Button buttonTemplate;

    public void SetControlOptionText(string controlOptionText)
    {
        this.controlOptionText.text = controlOptionText;
    }

    public Button GetButtonTemplate()
    {
        return buttonTemplate;
    }

    public string GetControlOption()
    {
        return this.controlOptionText.text;
    }

}
