using UnityEngine;
using UnityEditor;

// Unity编辑器工具类，用于生成外围音频可视化组（在圆形路径上生成立方体数组）
public class CircleArrayGenerator : EditorWindow
{
    // 圆心物体，用于确定生成位置的中心点
    public GameObject centerObject;

    [SerializeField]
    [Tooltip("圆形数组的半径，决定立方体距离中心的距离")]
    public float radius = 500f;

    [SerializeField]
    [Tooltip("要生成的立方体数量，均匀分布在圆周上")]
    public int count = 2048;

    // 用于生成立方体的预制体
    public GameObject cubePrefab;

    // 在Unity编辑器菜单中添加工具入口
    [MenuItem("Tools/Circle Array Generator")]
    public static void ShowWindow()
    {
        // 打开编辑器窗口并设置标题
        GetWindow<CircleArrayGenerator>("Circle Generator");
    }

    // 编辑器窗口的GUI绘制逻辑
    void OnGUI()
    {
        // 显示标题标签
        GUILayout.Label("Array Settings", EditorStyles.boldLabel);

        // 参数输入区域
        centerObject = (GameObject)EditorGUILayout.ObjectField("Center Object", centerObject, typeof(GameObject), true);
        radius = EditorGUILayout.FloatField("Radius", radius);
        count = EditorGUILayout.IntField("Count", count);
        cubePrefab = (GameObject)EditorGUILayout.ObjectField("Cube Prefab", cubePrefab, typeof(GameObject), false);

        // 生成按钮，点击时触发生成逻辑
        if (GUILayout.Button("Generate Array"))
        {
            GenerateCubes();
        }
    }

    // 生成圆形数组的立方体
    void GenerateCubes()
    {
        if (!centerObject)
        {
            Debug.LogError("No center object assigned!"); // 检查中心物体是否为空
            return;
        }

        // 清空旧的子物体
        ClearChildren();

        Vector3 center = centerObject.transform.position; // 获取中心点位置

        // 计算每个立方体之间的角度间隔（度）
        float angleStep = 360f / count;

        // 循环生成指定数量的立方体
        for (int i = 0; i < count; i++)
        {
            // 计算当前立方体的角度（弧度）
            float angle = i * angleStep * Mathf.Deg2Rad;

            // 根据半径和角度计算立方体的位置
            Vector3 pos = center + new Vector3(
                Mathf.Cos(angle) * radius, // X轴位置
                0,                         // Y轴保持不变
                Mathf.Sin(angle) * radius  // Z轴位置
            );

            // 从预制体实例化新的立方体
            GameObject newCube = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab);
            newCube.transform.position = pos;              // 设置位置
            newCube.transform.SetParent(centerObject.transform); // 设置为中心的子物体

            // 记录Undo操作，支持撤销
            Undo.RegisterCreatedObjectUndo(newCube, "Create Cube");
        }
    }

    // 清空中心物体下的所有子物体
    void ClearChildren()
    {
        // 循环销毁所有子物体
        while (centerObject.transform.childCount > 0)
        {
            DestroyImmediate(centerObject.transform.GetChild(0).gameObject);
        }
    }
}