using UnityEngine;

public class SunLight2D : MonoBehaviour
{
    [Header("太阳绕零点旋转速度（度/秒）")]
    public float rotationSpeed = 20f;

    [Header("太阳距离中心的半径")]
    public float radius = 5f;

    [Header("发光参数")]
    [Range(0f, 5f)] public float minIntensity = 0.1f;
    [Range(0f, 5f)] public float maxIntensity = 1f;
    [Range(0.1f, 5f)] public float power = 1f;

    private static readonly int SunDirID = Shader.PropertyToID("_SunDir");
    private static readonly int MinID = Shader.PropertyToID("_MinIntensity");
    private static readonly int MaxID = Shader.PropertyToID("_MaxIntensity");
    private static readonly int PowerID = Shader.PropertyToID("_Power");

    void Start()
    {
        transform.position = new Vector3(radius, 0, 0);

        // 初始化全局参数
        Shader.SetGlobalFloat(MinID, minIntensity);
        Shader.SetGlobalFloat(MaxID, maxIntensity);
        Shader.SetGlobalFloat(PowerID, power);
    }

    void Update()
    {
        // 太阳绕零点旋转
        transform.RotateAround(Vector3.zero, Vector3.forward, rotationSpeed * Time.deltaTime);

        // 太阳方向
        Vector3 sunDir = (Vector3.zero - transform.position).normalized;
        Shader.SetGlobalVector(SunDirID, sunDir);
    }
}
