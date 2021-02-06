using System;
using System.Collections.Generic;
using System.Linq;

// I refered http://marupeke296.com/COL_3D_No15_Octree.html

public interface IOctreeRegistable
{
    ((float, float, float), (float, float, float)) MinMaxXYZ { get; }
    IOctreeCell AffiliationCell { get; set; }
}

public partial class Octree
{
    public Octree(int inMaxLevel, float inDimensions)
    {
        if ((inMaxLevel <= 0) || (8 < inMaxLevel)) {
            throw new ArgumentOutOfRangeException($"Octree.constructor: Argument \"{nameof(inMaxLevel)}\" must be in range of 1 to 8.");
        }

        MaxLevel = inMaxLevel;
        Dimensions = inDimensions;
        CellDimensions = new float[MaxLevel + 1];
        for (int i = 0; i <= MaxLevel; ++i) {
            var f = (float)Math.Pow(2.0f, i);
            CellDimensions[i] = Dimensions / f;
        }

        Root = new RootCell(MaxLevel);
        LinearCells = Root.Linearize();

        AxisCellsAtEndLayer = (int)(Math.Pow(2, MaxLevel) + 0.5);
        return;
    }

    public IOctreeCell Register(IOctreeRegistable inValue)
    {
        var minmax = inValue.MinMaxXYZ;
        var min = GetMortonOrder(minmax.Item1);
        var max = GetMortonOrder(minmax.Item2);
        var order = min ^ max;

        const uint MASK = 0xFFFFFFFF;
        int layer = MaxLevel;
        while ((order & MASK) != 0) {
            order = order >> 3;
            --layer;
        }
        order = min >> ((MaxLevel - layer) * 3);

        var index = GetLinearIndex(layer, order);
        var result = LinearCells[index];
        result.Objects.AddLast(inValue);
        return result;
    }

    public void Unregister(IOctreeRegistable inValue)
    {
        if (inValue.AffiliationCell != null) {
            if (!(inValue.AffiliationCell is Cell)) {
                throw new InvalidCastException($"Object belongs difference trees.");
            }

            (inValue.AffiliationCell as Cell).Objects.Remove(inValue);
        }
        return;
    }

    public void Clear()
    {
        foreach (var cell in LinearCells) {
            foreach (var obj in cell.Objects) {
                obj.AffiliationCell = null;
            }
            cell.Objects.Clear();
        }
        return;
    }

    public IOctreeCell GetCellAt(int inLevel, (float, float, float) inPosition)
    {
        uint morton = GetMortonOrder(inPosition);
        int index = GetLinearIndex(MaxLevel, morton);
        var cell = LinearCells[index];

        while ((cell != null) && (cell.Level != inLevel)) {
            cell = cell.Parent;
        }
        return cell;
    }

    public IOctreeCell GetCellAt(int inLevel, (int, int, int) inAxisIndicesInEndLayer)
    {
        uint morton = GetMortonOrder(inAxisIndicesInEndLayer);
        int index = GetLinearIndex(MaxLevel, morton);
        var cell = LinearCells[index];

        while ((cell != null) && (cell.Level != inLevel)) {
            cell = cell.Parent;
        }
        return cell;
    }

    public IEnumerable<IOctreeCell> TraverseOnRay((float x, float y, float z) inRayStart, (float x, float y, float z) inRayEnd)
    {
        // I refered
        // https://flipcode.com/archives/Raytracing_Topics_Techniques-Part_4_Spatial_Subdivisions.shtml
        // https://github.com/francisengelmann/fast_voxel_traversal/blob/master/main.cpp

        float dimension = CellDimensions.Last();

        (float x, float y, float z) ray = (
            inRayEnd.x - inRayStart.x,
            inRayEnd.y - inRayStart.y,
            inRayEnd.z - inRayStart.z
        );
        float rayLength = (float)Math.Sqrt((ray.x * ray.x) + (ray.y * ray.y) + (ray.z * ray.z));

        (int x, int y, int z) currentCell = (
            (int)(inRayStart.x / dimension),
            (int)(inRayStart.y / dimension),
            (int)(inRayStart.z / dimension)
        );
        //(int x, int y, int z) lastCell = (
        //    (int)(inRayEnd.x / dimension),
        //    (int)(inRayEnd.y / dimension),
        //    (int)(inRayEnd.z / dimension)
        //);

        if (rayLength <= 1.0e-5) {
            if (IndexInRangeOfEndLayer(currentCell.x, currentCell.y, currentCell.z)) {
                yield return GetCellAt(MaxLevel, currentCell);
            }
            yield break;
        }

        // In which direction the voxel ids are incremented.
        (int x, int y, int z) step = (
            Math.Sign(ray.x),
            Math.Sign(ray.y),
            Math.Sign(ray.z)
        );

        // Distance along the ray to the next voxel border from the current position tMax.
        //(float x, float y, float z)  nextBoundary = (
        //    (currentCell.x + step.x) * dimension,
        //    (currentCell.y + step.y) * dimension,
        //    (currentCell.z + step.z) * dimension
        //);

        // tDelta -- 
        // how far along the ray we must move for the horizontal component to equal the width of a voxel
        // the direction in which we traverse the grid
        // can only be FLT_MAX if we never go in that direction
        (float x, float y, float z)  tDelta = (
            (step.x != 0) ? dimension / ray.x * step.x : float.MaxValue,
            (step.y != 0) ? dimension / ray.y * step.y : float.MaxValue,
            (step.z != 0) ? dimension / ray.z * step.z : float.MaxValue
        );

        // tMax -- distance until next intersection with voxel-border
        // the value of t at which the ray crosses the first vertical voxel boundary
        var tMax = tDelta;

        Func<bool> IsStepOver = () => ((step.x != 0) && (tMax.x > rayLength)) || ((step.y != 0) && (tMax.y > rayLength)) || ((step.z != 0) && (tMax.z > rayLength));
        while (!IsStepOver()) {
            if (IndexInRangeOfEndLayer(currentCell.x, currentCell.y, currentCell.z)) {
                yield return GetCellAt(MaxLevel, currentCell);
            }

            switch (MinIndex(tMax)) {
                case 0:
                    currentCell.x += step.x;
                    tMax.x += tDelta.x;
                    break;
                case 1:
                    currentCell.y += step.y;
                    tMax.y += tDelta.y;
                    break;
                case 2:
                    currentCell.z += step.z;
                    tMax.z += tDelta.z;
                    break;
            }
        }
        yield break;
    }

    private (byte, byte, byte) GetMortonOrders((float, float, float) inPosition)
    {
        var cellDim = CellDimensions.Last();
        return GetMortonOrders((
            Clamp((int)(inPosition.Item1 / cellDim), 0, AxisCellsAtEndLayer - 1),
            Clamp((int)(inPosition.Item2 / cellDim), 0, AxisCellsAtEndLayer - 1),
            Clamp((int)(inPosition.Item3 / cellDim), 0, AxisCellsAtEndLayer - 1)
        ));
    }

    private (byte, byte, byte) GetMortonOrders((int, int, int) inAxisIndices)
    {
        return (
            (byte)inAxisIndices.Item1,
            (byte)inAxisIndices.Item2,
            (byte)inAxisIndices.Item3
        );
    }

    private uint GetMortonOrder((float, float, float) inPosition)
    {
        var (x, y, z) = GetMortonOrders(inPosition);
        return SeparateBit(x)
             | SeparateBit(y) << 1
             | SeparateBit(z) << 2;
    }

    private uint GetMortonOrder((int, int, int) inAxisIndices)
    {
        var (x, y, z) = GetMortonOrders(inAxisIndices);
        return SeparateBit(x)
             | SeparateBit(y) << 1
             | SeparateBit(z) << 2;
    }

    private uint SeparateBit(byte inValue)
    {
        const uint n8 = 0x0000f00f;
        const uint n4 = 0x000c30c3;
        const uint n2 = 0x00249249;
        uint v = inValue;
        v = (v | v << 8) & n8;
        v = (v | v << 4) & n4;
        v = (v | v << 2) & n2;
        return v;
    }

    private static int GetLinearIndex(int inLevel, uint inIndexInLayer)
    {
        if (Exp8[inLevel] <= inIndexInLayer) {
            throw new ArgumentOutOfRangeException($"{nameof(inIndexInLayer)}({inIndexInLayer}) is bigger than cell count in the layer {nameof(inLevel)}({inLevel}). ");
        }
        if (inLevel == 0) {
            return 0;
        }
        return (int)(((Exp8[inLevel] - 1) / 7) + inIndexInLayer); ;
    }

    private int Clamp(int inValue, int inMin, int inMax) => Math.Max(inMin, Math.Min(inMax, inValue));

    private int MinIndex((float x, float y, float z) inValue)
    {
        if (inValue.x < inValue.y) {
            if (inValue.x < inValue.z) {
                return 0;
            } else {
                return 2;
            }
        } else {
            if (inValue.y < inValue.z) {
                return 1;
            } else {
                return 2;
            }
        }
        throw new InvalidProgramException();
    }

    private bool IndexInRangeOfEndLayer(int inIndexX, int inIndexY, int inIndexZ)
        => (0 <= inIndexX) && (inIndexX < AxisCellsAtEndLayer)
        && (0 <= inIndexY) && (inIndexY < AxisCellsAtEndLayer)
        && (0 <= inIndexZ) && (inIndexZ < AxisCellsAtEndLayer);

    public int MaxLevel
    { get; }
    public float Dimensions
    { get; }
    private RootCell Root
    { get; }
    private IReadOnlyList<Cell> LinearCells
    { get; }
    private int AxisCellsAtEndLayer
    { get; }

    public readonly float[] CellDimensions;
    private static readonly uint[] Exp8 = {
        1, 8, 64, 512, 4096, 32768, 262144, 2097152, 16777216
    };
    public static readonly uint[] AxisCellsInLayer = {
        1, 2, 4, 8, 16, 32, 64, 128, 256
    };
}