using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PerkConfig", menuName = "Trivia/Perk Config")]
public class PerkConfiguration : ScriptableObject
{
    public List<PerkData> perkDataList;

    public int effectLimit = 5;

    public int tokenTolerance = 2;

    public int minMultiplier = 1;
    public int maxMultiplier = 10;

    public int maxPositiveToken = 32;
    public int maxNegativeToken = -32;

    public int flexibility = 2;

    public int tokenMultiplierRatio = 5;

    public float whiteListChance = 0.3f;

    public float activePerkTypeChance = 0.2f;

    public float positivePerkChance = 0.2f;
    public float negativePerkChance = 0.13f;

    public Sprite perkPositiveProfileIcon;
    public Sprite perkNegativeProfileIcon;
    public Sprite perkNeutralProfileIcon;
}
