using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ControlOptionSingleButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI controlOptionText;

    public void SetControlOptionText(string controlOptionText)
    {
        this.controlOptionText.text = controlOptionText;
    }

    public string GetControlOption()
    {
        return this.controlOptionText.text;
    }

}
