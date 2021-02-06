using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RayProjector : MonoBehaviour
{
    [SerializeField] public float StartOffset = 0.0f;
    [SerializeField] public float RayLength = 100.0f;

    public Vector3 RayStartPosition => RayPositions[0];
    public Vector3 RayEndPosition => RayPositions[1];

    // Update is called once per frame
    void Update()
    {
        var renderer = GetComponent<LineRenderer>();

        var forward = transform.forward;
        var position = transform.position;
        RayPositions[0] = position + (forward * StartOffset);
        RayPositions[1] = position + (forward * (StartOffset + RayLength));

        renderer.SetPositions(RayPositions);
        return;
    }

    private Vector3[] RayPositions = new Vector3[2];
}
