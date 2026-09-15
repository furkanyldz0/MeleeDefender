using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class DifficultyConfigSO : ScriptableObject
{
    public List<DifficultyTier> tiers;
}
