using UnityEngine;
using System.Collections;

// 音频可视化颜色渐变：控制材质颜色随时间平滑变化，生成明亮的HDR颜色
public class OuterRing_SpotLamp_Color : MonoBehaviour
{
    [Header("颜色参数")]
    [SerializeField]
    [Range(0.1f, 10f)]
    [Tooltip("颜色变化的间隔时间（秒），包括渐变和等待时间")]
    private float changeInterval = 3f; // 颜色变化间隔

    [SerializeField]
    [Range(0.1f, 5f)]
    [Tooltip("颜色从当前值渐变到目标值所需的持续时间（秒）")]
    private float transitionDuration = 1f; // 渐变持续时间

    [Header("材质控制")]
    [SerializeField]
    [Tooltip("需要控制的目标材质，需支持HDR颜色属性")]
    private Material targetMaterial; // 需要外部拖入的材质

    [SerializeField]
    [Tooltip("目标材质的HDR颜色属性名称")]
    public string colorProperty = "_"; // HDR颜色属性名

    private Color currentColor;        // 当前颜色
    private Color targetColor;         // 目标颜色
    private Coroutine colorTransition; // 控制颜色渐变的协程

    // 启用时开始颜色循环
    void OnEnable()
    {
        StartColorCycle();
    }

    // 禁用时停止颜色循环
    void OnDisable()
    {
        StopColorCycle();
    }

    // 开始颜色循环逻辑
    public void StartColorCycle()
    {
        if (colorTransition == null)
        {
            currentColor = targetMaterial.GetColor(colorProperty); // 初始化当前颜色
            colorTransition = StartCoroutine(ColorTransitionLoop()); // 启动协程
        }
    }

    // 停止颜色循环逻辑
    public void StopColorCycle()
    {
        if (colorTransition != null)
        {
            StopCoroutine(colorTransition); // 停止协程
            colorTransition = null;         // 清空协程引用
        }
    }

    // 颜色渐变循环协程
    IEnumerator ColorTransitionLoop()
    {
        while (true)
        {
            // 生成新的明亮HDR颜色作为目标
            targetColor = GenerateBrightHDRColor();

            float elapsed = 0f;
            Color startColor = currentColor; // 记录起始颜色

            // 颜色渐变阶段
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                currentColor = Color.Lerp(startColor, targetColor, elapsed / transitionDuration); // 平滑插值
                UpdateMaterialColor(currentColor); // 更新材质颜色
                yield return null; // 等待下一帧
            }

            // 等待剩余的间隔时间
            yield return new WaitForSeconds(changeInterval - transitionDuration);
        }
    }

    // 生成明亮的HDR颜色
    Color GenerateBrightHDRColor()
    {
        // 使用HSV模式生成颜色，确保明亮且适合HDR
        Color baseColor = Random.ColorHSV(
            0f, 1f,    // 色相范围：全范围随机
            0.5f, 1f,  // 饱和度范围：中等至高饱和度
            1f, 1f,  // 亮度范围：较高亮度
            2f, 3f     // HDR强度范围：Alpha通道存储强度，适合发光效果
        );

        // 返回HDR颜色，强度存储在Alpha通道
        return new Color(
            baseColor.r,
            baseColor.g,
            baseColor.b,
            baseColor.a // HDR强度
        );
    }

    // 更新材质的颜色属性
    void UpdateMaterialColor(Color newColor)
    {
        if (targetMaterial != null)
        {
            // 直接设置材质颜色（注：此处未使用MaterialPropertyBlock以保持原逻辑）
            targetMaterial.SetColor(colorProperty, newColor);
        }
    }

    // 外部方法：设置目标材质并初始化当前颜色
    public void SetTargetMaterial(Material mat)
    {
        targetMaterial = mat;
        if (mat != null)
        {
            currentColor = mat.GetColor(colorProperty); // 更新当前颜色
        }
    }
}