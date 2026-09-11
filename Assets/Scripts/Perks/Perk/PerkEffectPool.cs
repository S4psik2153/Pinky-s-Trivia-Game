using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PerkEffectPool", menuName = "Perks/Perk Effect Pool")]
public class PerkEffectPool : ScriptableObject
{
    [SerializeField] private List<PerkEffect> ActiveEffects;

    public List<PerkEffect> GetAllActiveEffects()
    {
        if (ActiveEffects == null)
        {
            Debug.LogError("Perk effect list is null.");

            return new List<PerkEffect>();
        }

        return new List<PerkEffect>(ActiveEffects);
    }

    [SerializeField] private List<PerkEffect> PassiveEffects;

    public List<PerkEffect> GetAllPassiveEffects()
    {
        if (PassiveEffects == null)
        {
            Debug.LogError("Perk effect list is null.");

            return new List<PerkEffect>();
        }

        return new List<PerkEffect>(PassiveEffects);
    }
}
