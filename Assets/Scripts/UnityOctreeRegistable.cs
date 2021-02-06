using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class UnityOctreeRegistable : MonoBehaviour, IOctreeRegistable
{
    private void Awake()
    {
        m_AABBVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_AABBVisual.name = "AABB";
        //m_AABBVisual.transform.parent = gameObject.transform;
        if (VisualizeOptions.Instance.ObjectAABBMaterial != null) {
            m_AABBVisual.GetComponent<Renderer>().material = VisualizeOptions.Instance.ObjectAABBMaterial;
        }
        return;
    }

    private void OnDestroy()
    {
        Destroy(m_AABBVisual);
        return;
    }

    private void Update()
    {
        if (!VisualizeOptions.Instance.ShowObjectAABB.Value) {
            m_AABBVisual.SetActive(false);
            return;
        }
        Bounds bounds = AABB;
        if (bounds.size.sqrMagnitude <= 1.0e-5f) {
            m_AABBVisual.SetActive(false);
            return;
        }

        m_AABBVisual.SetActive(true);
        m_AABBVisual.transform.position = transform.position;
        m_AABBVisual.transform.rotation = Quaternion.identity;
        m_AABBVisual.transform.localScale = bounds.size;
        return;
    }

    public Bounds AABB => GetComponent<Renderer>().bounds;

    ((float, float, float), (float, float, float)) IOctreeRegistable.MinMaxXYZ
    {
        get {
            Bounds aabb = AABB;
            return (
                    (aabb.min.x, aabb.min.y, aabb.min.z),
                    (aabb.max.x, aabb.max.y, aabb.max.z)
                );
        }
    }
    IOctreeCell IOctreeRegistable.AffiliationCell { get; set; } = null;

    private GameObject m_AABBVisual = null;
}
