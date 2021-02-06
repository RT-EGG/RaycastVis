using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnityOctreeTester : MonoBehaviour
{
    [SerializeField, Range(1, 100)] int GenerateObjectCount = 1;
    [SerializeField] UnityOctree Octree = null;
    [SerializeField] float ScaleMin = 0.1f;
    [SerializeField] float ScaleMax = 5.0f;
    [SerializeField] RayProjector RayProjector = null;

    public bool Escape = false;

    private void Update()
    {
        if ((Octree == null) || (RayProjector == null)) {
            return;
        }

        // convert to relative from self transform
        var world2local = Octree.transform.localToWorldMatrix.inverse;
        Vector3 rayStart = world2local.MultiplyPoint(RayProjector.RayStartPosition);
        Vector3 rayEnd = world2local.MultiplyPoint(RayProjector.RayEndPosition);

        Octree.HideAllCellVisual();
        foreach (var cell in Octree.TraverseCellsOnRay(rayStart, rayEnd)) {
            Octree.ShowCellVisual(cell);
        }

        if (Escape) {
            UnityEditor.EditorApplication.isPlaying = false;
        }

        return;
    }

    public void GenerateObjects()
    {
        Clear();
        if (Octree == null) {
            throw new NullReferenceException(nameof(Octree));
        }

        for (int i = 0; i < GenerateObjectCount; ++i) {
            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.name = $"object{i}";
            newObject.transform.parent = Octree?.transform;
            newObject.AddComponent<UnityOctreeRegistable>();
            RandomizeTransform(newObject.transform);

            m_Objects.Add(newObject);
        }

        Octree.Register(m_Objects.Select(o => o.GetComponent<UnityOctreeRegistable>()));
        return;
    }

    public void Clear()
    {
        Octree.Clear();

        foreach (var o in m_Objects) {
            Destroy(o);
        }
        m_Objects.Clear();
        return;
    }

    private void RandomizeTransform(Transform inTarget)
    {
        inTarget.localPosition = RandomRange(Octree.Dimensions);
        inTarget.localRotation = UnityEngine.Random.rotation;
        inTarget.localScale = RandomRange(ScaleMin, ScaleMax);
        return;
    }

    private Vector3 RandomRange(float inMax)
        => RandomRange(0.0f, inMax);

    private Vector3 RandomRange(float inMin, float inMax)
        => new Vector3(
            UnityEngine.Random.Range(inMin, inMax),
            UnityEngine.Random.Range(inMin, inMax),
            UnityEngine.Random.Range(inMin, inMax)
        );

    private List<GameObject> m_Objects = new List<GameObject>();
}
