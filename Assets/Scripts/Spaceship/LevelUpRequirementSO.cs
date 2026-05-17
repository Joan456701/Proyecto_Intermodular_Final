using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "LevelUpRequirement", menuName = "Base/Level Up Requirement")]
public class LevelUpRequirementSO : ScriptableObject
{
    public BuildRequirement[] requirements;

    public Sprite materialOne;
    public Sprite materialTwo;
}
