using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IOctreeCellExtensions
{
    public static Vector3Int LocalIndices(this IOctreeCell inCell)
        => new Vector3Int(
            ((inCell.LocalIndex % 2) == 1) ? 1 : 0,
            ((inCell.LocalIndex % 4) >= 2) ? 1 : 0,
            (inCell.LocalIndex > 3) ? 1 : 0
        );

    public static Vector3Int GetAxisIndicesInLayer(this IOctreeCell inCell)
    {
        if (inCell.Level == 0) {
            return Vector3Int.zero;
        }

        var result = inCell.Parent.GetAxisIndicesInLayer() * 2;        
        return result + inCell.LocalIndices();
    }
}

public class UnityOctree : MonoBehaviour
{
    [SerializeField, Range(1, 8)] int MaxLevel = 3;
    [SerializeField] public float Dimensions = 10.0f;

    public void Rebuild()
    {
        Clear();
        m_Octree = new Octree(MaxLevel, Dimensions);

        //for (int x = 0; x < Octree.AxisCellsInLayer[MaxLevel]; ++x) {
        //    for (int y = 0; y < Octree.AxisCellsInLayer[MaxLevel]; ++y) {
        //        for (int z = 0; z < Octree.AxisCellsInLayer[MaxLevel]; ++z) {
        //            var cell = m_Octree.GetCellAt(MaxLevel, (x, y, z));
        //            ShowCellVisual(cell);
        //        }
        //    }
        //}
        return;
    }

    private void Start()
    {
        Rebuild();
        return;
    }

    public void Register(IEnumerable<IOctreeRegistable> inObjects)
    {
        Clear();

        if (m_Octree == null) {
            Rebuild();
        }
        m_Octree.Clear();
        foreach (var obj in inObjects) {
            var cell = m_Octree.Register(obj);
            ShowCellVisual(cell);
        }
        return;
    }

    public void Clear()
    {
        foreach (var obj in m_CellVisuals.Values) {
            Destroy(obj);
        }
        m_CellVisuals.Clear();
        return;
    }

    public void ShowCellVisual(IOctreeCell inCell)
    {
        if (!m_CellVisuals.TryGetValue(inCell, out var visual)) {
            var newCellObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var newCell = newCellObject.AddComponent<UnityOctreeCell>();
            var indices = inCell.GetAxisIndicesInLayer();
            newCell.transform.parent = transform;
            newCell.name = $"{inCell.Level}-{indices.x}.{indices.y}.{indices.z}";
            newCell.Owner = this;
            newCell.Cell = inCell;
            newCell.Visible = true;

            m_CellVisuals.Add(inCell, newCellObject);
            visual = newCellObject;
        }

        visual.GetComponent<UnityOctreeCell>().Visible = true;
        visual.SetActive(true);
        return;
    }

    public void HideAllCellVisual()
    {
        foreach (var cell in m_CellVisuals.Values) {
            cell.GetComponent<UnityOctreeCell>().Visible = false;
        }
        return;
    }

    public IEnumerable<IOctreeCell> TraverseCellsOnRay(Vector3 inRayStart, Vector3 inRayEnd)
        => m_Octree.TraverseOnRay(inRayStart.Deconstruction(), inRayEnd.Deconstruction());

    public Vector3 GetCellCenterPosition(IOctreeCell inCell)
    {
        var indices = inCell.GetAxisIndicesInLayer();
        var dimensions = GetCellDimensionsForLevel(inCell.Level);

        return new Vector3(
                indices.x * dimensions,
                indices.y * dimensions,
                indices.z * dimensions
            ) + (Vector3.one * (dimensions * 0.5f));
    }
    public float GetCellDimensionsForLevel(int inLevel) => m_Octree.CellDimensions[inLevel];

    private Octree m_Octree = null;
    private Dictionary<IOctreeCell, GameObject> m_CellVisuals = new Dictionary<IOctreeCell, GameObject>();
}
