using UnityEngine;

[RequireComponent(typeof(Renderer))]
class UnityOctreeCell : MonoBehaviour
{
    public UnityOctree Owner = null;
    public IOctreeCell Cell = null;

    private Renderer m_Renderer = null;

    private void Start()
    {
        m_Renderer = GetComponent<Renderer>(); // localize

        Debug.Assert((Cell != null) && (Owner != null), $"Set property \"{nameof(Owner)}\" and \"{nameof(Cell)}\".");

        transform.parent = Owner.transform;
        transform.localPosition = Owner.GetCellCenterPosition(Cell);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * Owner.GetCellDimensionsForLevel(Cell.Level);
        return;
    }

    private void Update()
    {
        if (!VisualizeOptions.Instance.GetOctreeCellOptionsForLevel(Cell.Level, out var cellVisualOptions)) {
            Visible = false;
            return;
        }
        if ((!cellVisualOptions.Show) || (cellVisualOptions.Material == null)) {
            Visible = false;
            return;
        }

        GetComponent<Renderer>().material = cellVisualOptions.Material;
        return;
    }

    public bool Visible
    {
        get => Renderer.enabled;
        set => Renderer.enabled = value;
    }

    private Renderer Renderer
    {
        get {
            if (m_Renderer == null) {
                m_Renderer = GetComponent<Renderer>();
            }
            return m_Renderer;
        }
    }
}
