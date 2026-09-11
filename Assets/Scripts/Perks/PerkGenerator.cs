using System.Collections.Generic;
using UnityEngine;

public class PerkGenerator
{
    public enum TokenProfile { Positive, Negative, Balanced }
    
    private readonly PerkEffectPool effectPool;
    private readonly PerkConfiguration perkConfig;

    public PerkGenerator(PerkEffectPool effectPool, PerkConfiguration perkConfig)
    {
        this.effectPool = effectPool;
        this.perkConfig = perkConfig;
    }

    public List<Perk> GeneratePerks(int count, TokenProfile profile)
    {
        List<Perk> perkList = new();

        if (Random.value <= perkConfig.activePerkTypeChance)
        {  
            for (int i = 0; i < Mathf.Min(count, effectPool.GetAllActiveEffects().Count); i++)
            {
                perkList.Add(GenerateSinglePerk(profile, Perk.PerkType.Active));
            }
        }
        else
        {
            for (int i = 0; i < Mathf.Min(count, effectPool.GetAllPassiveEffects().Count); i++)
            {
                perkList.Add(GenerateSinglePerk(profile, Perk.PerkType.Passive));
            }           
        }

        return perkList;
    }

    private Perk GenerateSinglePerk(TokenProfile profile, Perk.PerkType type)
    {
        Perk generatedPerk = ScriptableObject.CreateInstance<Perk>();
        
        List<PerkEffectEntry> generatedPerkEffectEntries = new();

        List<PerkEffect> perkEffects = new();

        if (type == Perk.PerkType.Active)
        {
            perkEffects = effectPool.GetAllActiveEffects();
        }
        else if (type == Perk.PerkType.Passive)
        {
            perkEffects = effectPool.GetAllPassiveEffects();
        }
        else
        {
            Debug.LogError($"Unknown perk type: {type}");
            
            return null;
        }
        
        List<PerkEffectEntry> effectEntryList = new();

        foreach (PerkEffect effect in perkEffects)
        {
            PerkEffectEntry entry = new()
            {
                effect = effect
            };

            effectEntryList.Add(entry);
        }

        switch(profile)
        {
            case TokenProfile.Balanced:
            {
                List<PerkEffectEntry> positiveEffects = new();
                List<PerkEffectEntry> negativeEffects = new();

                for (int i = 0; i < effectEntryList.Count; i++)
                {
                    if (effectEntryList[i].effect.TokenValue < 0)
                    {
                        negativeEffects.Add(effectEntryList[i]);
                    }
                    else if (effectEntryList[i].effect.TokenValue > 0)
                    {
                        positiveEffects.Add(effectEntryList[i]);
                    }
                    else
                    {
                        Debug.LogError($"Unidentified token value: {effectEntryList[i].effect.TokenValue}");

                        continue;
                    }
                }

                int targetToken = Random.Range(1, perkConfig.maxPositiveToken + 1);
                int currentToken = 0;

                do
                {
                    int remainingTarget = targetToken - currentToken;
                    
                    // ters işaret göndererek dengeleme dalını tetikle.
                    List<PerkEffectEntry> added = GetRandomEffect(positiveEffects, generatedPerkEffectEntries, totalToken: -remainingTarget);
                    
                    if (added.Count == 0) break;
                    
                    generatedPerkEffectEntries.AddRange(added);
                    
                    // toplamı yeniden hesapla.
                    currentToken = CalculateTotalToken(generatedPerkEffectEntries);
                }
                while (currentToken < targetToken && generatedPerkEffectEntries.Count < perkConfig.effectLimit / 2);

                int totalToken;
                do
                {                    
                    totalToken = CalculateTotalToken(generatedPerkEffectEntries);

                    if (totalToken < - perkConfig.tokenTolerance)
                    {
                        List<PerkEffectEntry> balanceEffects = GetRandomEffect(positiveEffects, generatedPerkEffectEntries, totalToken: totalToken);

                        if (balanceEffects == null || balanceEffects.Count <= 0) break;
                        
                        generatedPerkEffectEntries.AddRange(balanceEffects);
                    }
                    else if (totalToken > perkConfig.tokenTolerance)
                    {
                        List<PerkEffectEntry> balanceEffects = GetRandomEffect(negativeEffects, generatedPerkEffectEntries, totalToken: totalToken);

                        if (balanceEffects == null || balanceEffects.Count <= 0) break;

                        generatedPerkEffectEntries.AddRange(balanceEffects);
                    }
                    else
                    {
                        break;
                    }
                }
                while (Mathf.Abs(totalToken) > perkConfig.tokenTolerance && generatedPerkEffectEntries.Count < perkConfig.effectLimit);                
            }
            break;

            case TokenProfile.Positive:
            {
                List<PerkEffectEntry> positiveEffects = new();

                for (int i = 0; i < effectEntryList.Count; i++)
                {
                    if (effectEntryList[i].effect.TokenValue > 0)
                    {
                        positiveEffects.Add(effectEntryList[i]);
                    }
                    else
                    {
                        continue;
                    }
                }

                int targetToken = Random.Range(1, perkConfig.maxPositiveToken + 1);
                int currentToken = 0;

                do
                {
                    int remainingTarget = targetToken - currentToken;
                    
                    // ters işaret göndererek dengeleme dalını tetikle.
                    List<PerkEffectEntry> added = GetRandomEffect(positiveEffects, generatedPerkEffectEntries, totalToken: -remainingTarget);
                    
                    if (added.Count == 0) break;
                    
                    generatedPerkEffectEntries.AddRange(added);
                    
                    // toplamı yeniden hesapla.
                    currentToken = CalculateTotalToken(generatedPerkEffectEntries);
                }
                while (currentToken < targetToken && generatedPerkEffectEntries.Count < perkConfig.effectLimit);
            }
            break;
            
            case TokenProfile.Negative:
            {
                List<PerkEffectEntry> NegativeEffects = new();

                for (int i = 0; i < effectEntryList.Count; i++)
                {
                    if (effectEntryList[i].effect.TokenValue < 0)
                    {
                        NegativeEffects.Add(effectEntryList[i]);
                    }
                    else
                    {
                        continue;
                    }
                }

                int targetToken = Random.Range(perkConfig.maxNegativeToken, 0);
                int currentToken = 0;

                do
                {
                    int remainingTarget = targetToken - currentToken;
                    
                    // Ters işaret göndererek dengeleme dalını tetikle.
                    List<PerkEffectEntry> added = GetRandomEffect(NegativeEffects, generatedPerkEffectEntries, totalToken: -remainingTarget);
                    
                    if (added.Count == 0) break;
                    
                    generatedPerkEffectEntries.AddRange(added);
                    
                    // Toplamı yeniden hesapla.
                    currentToken = CalculateTotalToken(generatedPerkEffectEntries);
                }
                while (currentToken > targetToken && generatedPerkEffectEntries.Count < perkConfig.effectLimit);
            }
            break;
        }

        generatedPerk.Initialize(GetRandomPerkData(), type, generatedPerkEffectEntries);

        return generatedPerk;
    }

    private int CalculateTotalToken(List<PerkEffectEntry> entries)
    {
        int totalToken = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            totalToken += entries[i].effect.TokenValue * entries[i].multiplier;
        }

        return totalToken;
    }

    private List<PerkEffectEntry> GetRandomEffect(List<PerkEffectEntry> effects, List<PerkEffectEntry> perkEffects, int totalToken = 0, int count = 1)
    {
        List<PerkEffectEntry> effectList = new();
        List<PerkEffectEntry> currentPerkEffects = new(perkEffects);

        for (int i = 0; i < count; i++)
        {
            List<PerkEffectEntry> suitableEffectsList = new();

            foreach (PerkEffectEntry candidate in effects)
            {
                bool isSuitable = true;

                foreach (PerkEffectEntry effect in currentPerkEffects)
                {
                    if (effect.effect.BlackList != null && effect.effect.BlackList.Contains(candidate.effect)) isSuitable = false;

                    if (candidate.effect.BlackList != null && candidate.effect.BlackList.Contains(effect.effect)) isSuitable = false;
                }

                if (isSuitable) suitableEffectsList.Add(candidate);
            }

            if (suitableEffectsList.Count <= 0) break;

            List<PerkEffectEntry> preferedEffectsList = new();

            foreach (PerkEffectEntry candidate in suitableEffectsList)
            {
                foreach (PerkEffectEntry effect in currentPerkEffects)
                {
                    if (effect.effect.WhiteList == null) continue;

                    if (effect.effect.WhiteList.Contains(candidate.effect) && !preferedEffectsList.Contains(candidate)) preferedEffectsList.Add(candidate);
                }
            }

            if (preferedEffectsList.Count > 0 && Random.value < perkConfig.whiteListChance)
            {
                int rng = Random.Range(0, preferedEffectsList.Count);

                effects.Remove(preferedEffectsList[rng]);

                currentPerkEffects.Add(preferedEffectsList[rng]);

                effectList.Add(preferedEffectsList[rng]);
            }
            else
            {
                int rng = Random.Range(0, suitableEffectsList.Count);                

                effects.Remove(suitableEffectsList[rng]);

                currentPerkEffects.Add(suitableEffectsList[rng]);

                effectList.Add(suitableEffectsList[rng]);
            }
        }

        effectList = SetRandomMultipliers(effectList, totalToken);

        return effectList;
    }

    private PerkData GetRandomPerkData()
    {
        int rng = Random.Range(0, perkConfig.perkDataList.Count);

        PerkData data = perkConfig.perkDataList[rng];

        return data;
    }

    private List<PerkEffectEntry> SetRandomMultipliers(List<PerkEffectEntry> list, int totalToken)
    {
        List<PerkEffectEntry> entries = new();

        foreach (PerkEffectEntry entry in list)
        {
            int minMultiplierLimit = entry.effect.MinLimit == -1 ? perkConfig.minMultiplier : entry.effect.MinLimit;
            int maxMultiplierLimit = entry.effect.MaxLimit == -1 ? perkConfig.maxMultiplier : entry.effect.MaxLimit;

            for (int j = 0; j < Mathf.Abs(entry.effect.TokenValue) / perkConfig.tokenMultiplierRatio; j++)
            {
                maxMultiplierLimit = Random.Range(minMultiplierLimit, maxMultiplierLimit + 1);

                if (entry.effect.TokenValue * totalToken < 0)
                {
                    minMultiplierLimit = Random.Range(minMultiplierLimit, maxMultiplierLimit + 1);
                }
            }

            if (entry.effect.TokenValue * totalToken < 0)
            {
                int idealBalance = Mathf.RoundToInt(-totalToken / (float)entry.effect.TokenValue);

                minMultiplierLimit = Mathf.Clamp(idealBalance - perkConfig.flexibility, minMultiplierLimit, maxMultiplierLimit);
                maxMultiplierLimit = Mathf.Clamp(idealBalance + perkConfig.flexibility, minMultiplierLimit, maxMultiplierLimit);
            }

            int multiplier = Random.Range(minMultiplierLimit, maxMultiplierLimit + 1);

            entry.multiplier = multiplier;

            entries.Add(entry);

            totalToken += entry.effect.TokenValue * multiplier;
        }

        return entries;
    }
}
