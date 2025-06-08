using System.Collections.Generic;
using UnityEngine;

// 外圈音频可视化：根据音频频谱驱动一组立方体的高度变化，形成3D可视化效果
public class OuterRing : MonoBehaviour
{
    // 音频频谱数据源
    public GetSpectrum frequencySpectrum;

    private int _sampleLength; // 采样值，从FrequencySpectrum对象中自动获取

    [Header("外圈音频可视化设置")]
    [SerializeField]
    [Tooltip("包含所有音频条（立方体）的父物体")]
    public Transform cubeParent;

    // 存储所有音频条（立方体）的列表，从父物体中自动获取
    private List<GameObject> _cubeList = new List<GameObject>();

    [SerializeField]
    [Range(1, 30)]
    [Tooltip("音频条高度变化的平滑插值速度")]
    public float UpLerp = 15; // 音频条变化速度控制

    // 初始化时获取子物体并校验数量
    void Start()
    {
        if (cubeParent != null)
        {
            _sampleLength = frequencySpectrum._sampleLength; // 从频谱对象获取采样长度

            // 获取父物体下的所有子物体（立方体）并添加到列表
            foreach (Transform child in cubeParent)
            {
                _cubeList.Add(child.gameObject);
            }

            // 检查立方体数量是否与采样长度匹配
            if (_cubeList.Count != _sampleLength)
            {
                Debug.LogWarning($"Cube数量不匹配！父物体包含{_cubeList.Count}个子物体，但_sampleLenght={_sampleLength}");
                _sampleLength = Mathf.Min(_sampleLength, _cubeList.Count); // 取较小值以避免越界
            }
        }
        else
        {
            Debug.LogError("未指定Cube父物体！"); // 如果未指定父物体，输出错误日志
        }
    }

    // 每帧更新音频条的高度
    void Update()
    {
        for (int i = 0; i < _sampleLength; i++)
        {
            if (i >= _cubeList.Count) break; // 防止数组越界

            // 获取当前立方体的缩放
            Vector3 newScale = _cubeList[i].transform.localScale;

            // 计算目标高度：基于音频样本值，并随索引增加非线性放大
            float sampleValue = Mathf.Clamp(frequencySpectrum._samples[i] * (50 + i * i * 0.5f), 0, 50);

            // 平滑插值调整Y轴高度
            newScale.y = Mathf.Lerp(newScale.y, sampleValue, Time.deltaTime * UpLerp);

            // 应用新的缩放值
            _cubeList[i].transform.localScale = newScale;
        }
    }
}