using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectRow : MonoBehaviour
{
    [SerializeField] private Image effectTokenProfile_Icon;
    [SerializeField] private TextMeshProUGUI effectDescription_Text;
    [SerializeField] TextMeshProUGUI multiplier_Text;

    public void SetEffectElements(string description, Sprite icon, float multiplier)
    {
        effectDescription_Text.text = description;

        effectTokenProfile_Icon.sprite = icon;
        effectTokenProfile_Icon.type = Image.Type.Tiled;

        multiplier_Text.text = multiplier == 1f ? "" : $"x{multiplier:0.#}";
    }
}
