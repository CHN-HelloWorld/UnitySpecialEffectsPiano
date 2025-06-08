using UnityEngine;
using System.Collections;

public class GetSpectrum : MonoBehaviour
{
    // 外部指定的父GameObject，仅其子物体包含AudioSource组件
    [SerializeField]
    [Tooltip("包含所有子物体AudioSource组件的父GameObject")]
    public GameObject audioSourceParent;

    [SerializeField]
    [Range(64, 8192)]
    [Tooltip("频谱采样的数量，必须是2的幂 (范围: 64 - 8192)，建议与可视化对象的数量匹配")]
    public int _sampleLength = 512; // 采样值，建议与可视化对象数量匹配，例如512

    // 缩放倍数基础值，用于调整频谱数据的视觉效果强度
    [SerializeField]
    [Tooltip("频谱数据的缩放倍数基础值，控制可视化强度")]
    public float baseScaleMultiplier = 1f;

    // 动态缩放因子范围，用于根据AudioSource数量增强效果
    [SerializeField]
    [Tooltip("动态缩放因子最小值，适用于少量音符")]
    public float minDynamicScale = 1f;

    [SerializeField]
    [Tooltip("动态缩放因子最大值，适用于多音符场景")]
    public float maxDynamicScale = 2f;

    // 存储频谱数据的数组，供其他脚本（如可视化脚本）使用
    public float[] _samples;

    // 存储所有子物体上的AudioSource组件
    private AudioSource[] audioSources;

    void Start()
    {
        // 初始化频谱数据数组，根据采样长度分配空间
        _samples = new float[_sampleLength];

        // 启动协程，延迟1秒获取子物体的AudioSource组件
        StartCoroutine(DelayedInitializeAudioSources(1f));
    }

    // 协程：延迟指定时间后获取子物体的AudioSource组件
    private IEnumerator DelayedInitializeAudioSources(float delay)
    {
        // 等待指定时间（1秒），确保子物体的AudioSource已挂载
        yield return new WaitForSeconds(delay);

        // 获取父物体下所有子物体的AudioSource组件
        if (audioSourceParent != null)
        {
            audioSources = audioSourceParent.GetComponentsInChildren<AudioSource>();
            if (audioSources.Length == 0)
            {
                Debug.LogWarning("未在父物体 '" + audioSourceParent.name + "' 的子物体中找到任何AudioSource组件！");
            }
            else
            {
                Debug.Log($"在父物体 '{audioSourceParent.name}' 的子物体中找到 {audioSources.Length} 个AudioSource组件");
            }
        }
        else
        {
            Debug.LogError("请在Inspector中为 audioSourceParent 指定一个父GameObject！");
        }
    }

    void Update()
    {
        // 清空频谱数据数组，防止旧数据干扰
        System.Array.Clear(_samples, 0, _sampleLength);

        // 如果没有AudioSource或父物体未赋值，直接返回
        if (audioSources == null || audioSources.Length == 0 || audioSourceParent == null)
        {
            return;
        }

        // 临时数组，存储单个AudioSource的频谱数据
        float[] tempSamples = new float[_sampleLength];

        // 记录当前正在播放的AudioSource数量
        int activeSources = 0;

        // 遍历所有子物体的AudioSource，累加频谱数据
        foreach (AudioSource audio in audioSources)
        {
            if (audio != null && audio.isPlaying)
            {
                // 获取当前AudioSource的频谱数据
                // 参数说明：
                // - tempSamples: 存储单个AudioSource的频谱数据
                // - 0: 声道索引（0表示左声道或单声道）
                // - FFTWindow.BlackmanHarris: 傅里叶变换窗口类型，提供较好的频率分辨率
                audio.GetSpectrumData(tempSamples, 0, FFTWindow.BlackmanHarris);

                // 累加到_samples数组
                for (int i = 0; i < _sampleLength; i++)
                {
                    _samples[i] += tempSamples[i];
                }
                activeSources++;
            }
        }

        // 如果有正在播放的AudioSource，计算均值并应用动态缩放
        if (activeSources > 0)
        {
            // 计算动态缩放因子，根据AudioSource数量在minDynamicScale和maxDynamicScale之间插值
            // 假设10个AudioSource为最大参考值，调整插值逻辑以适应钢琴程序
            float dynamicScale = Mathf.Lerp(minDynamicScale, maxDynamicScale, activeSources / 10f);

            // 计算均值并应用缩放
            for (int i = 0; i < _sampleLength; i++)
            {
                _samples[i] /= activeSources; // 取均值
                _samples[i] *= baseScaleMultiplier * dynamicScale; // 应用基础缩放和动态缩放
                _samples[i] = Mathf.Clamp(_samples[i], 0f, 1f); // 限制频谱值在0-1范围，避免过大
            }
        }
    }
}