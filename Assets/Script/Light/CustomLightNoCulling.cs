using UnityEngine;
using System.Reflection;

#if UNITY_2021_2_OR_NEWER
using UnityEngine.Rendering.Universal;
#else
using UnityEngine.Experimental.Rendering.Universal;
#endif

[RequireComponent(typeof(Light2D))]
public class CustomLightNoCulling : MonoBehaviour
{
    private Light2D m_Light2D;
    private FieldInfo m_BoundingSphereField;
    private Camera m_MainCamera;

    void Awake()
    {
        m_Light2D = GetComponent<Light2D>();
        m_MainCamera = Camera.main;

        // 获取底层真正存储 boundingSphere 的属性后备字段（Backing Field）
        m_BoundingSphereField = typeof(Light2D).GetField("<boundingSphere>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                                ?? typeof(Light2D).GetField("m_BoundingSphere", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void LateUpdate()
    {
        if (m_Light2D == null || m_Light2D.lightType == Light2D.LightType.Global)
            return;

        if (m_MainCamera == null)
        {
            m_MainCamera = Camera.main;
            if (m_MainCamera == null) return;
        }

        if (m_BoundingSphereField != null)
        {
            // 【核心欺骗逻辑】
            // 将灯光的包围球中心直接设为相机的位置，半径给 1000f（只要大于相机裁剪远平面即可）
            // 这样无论你怎么移动相机，这个灯光的包围球永远包裹着相机，底层 Test Planes 绝对判定为“可见”
            Vector3 camPos = m_MainCamera.transform.position;
            BoundingSphere fakeSphere = new BoundingSphere(camPos, 10000f);

            m_BoundingSphereField.SetValue(m_Light2D, fakeSphere);
        }
    }
}