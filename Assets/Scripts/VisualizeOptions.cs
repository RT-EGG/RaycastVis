using UnityEngine;
using UniRx;

[System.Serializable]
public struct OctreeCellVisualOptions
{
    public bool Show;
    public Material Material;
}

public class VisualizeOptions : SingletonMonoBehaviour<VisualizeOptions>
{
    [SerializeField] public BoolReactiveProperty ShowObjectAABB = new BoolReactiveProperty(true);
    [SerializeField] public Material ObjectAABBMaterial = null;
    [SerializeField] public BoolReactiveProperty ShowCellAABB = new BoolReactiveProperty(true);
    [SerializeField] public OctreeCellVisualOptions[] OctreeCellOptions = new OctreeCellVisualOptions[0];

    public bool GetOctreeCellOptionsForLevel(int inLevel, out OctreeCellVisualOptions outValue)
    {
        if (inLevel >= OctreeCellOptions.Length) {
            outValue = default;
            return false;
        }

        outValue = OctreeCellOptions[inLevel];
        return true;
    }
}
