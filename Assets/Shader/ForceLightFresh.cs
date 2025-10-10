using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class ForceLightFresh : MonoBehaviour
{
    private void Awake()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Material mat = GetComponent<Renderer>().material;
        Graphics.DrawMesh(mesh, transform.localToWorldMatrix, mat, 0);

    }
}
