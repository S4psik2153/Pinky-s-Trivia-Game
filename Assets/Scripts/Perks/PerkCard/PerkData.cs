using UnityEngine;

[CreateAssetMenu(fileName = "PerkData", menuName = "Perks/PerkData", order = 0)]
public class PerkData : ScriptableObject
{
    [SerializeField] private string perkName;
    public string Name => perkName;
    [SerializeField] private Sprite perkIcon;
    public Sprite Icon => perkIcon;
    [SerializeField] private Sprite miniPerkIcon;
    public Sprite MiniIcon => miniPerkIcon;
}
