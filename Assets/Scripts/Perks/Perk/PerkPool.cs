using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PerkPool", menuName = "Perks/PerkPool", order = 1)]
public class PerkPool : ScriptableObject
{
    [SerializeField] private List<Perk> perkPool = new();

    public List<Perk> GetAllPerks()
    {
        return new List<Perk>(perkPool);
    }

    public void SetPerkPool(List<Perk> list)
    {
        perkPool = list;
    }
}