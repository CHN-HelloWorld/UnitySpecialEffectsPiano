using UnityEngine;
using System.Collections;

public class KeyMaterialSound : MonoBehaviour
{
    [SerializeField] public KeyCode keyToPress = KeyCode.None; // 按下的键
    [SerializeField] private Material activeMaterial; // 按下时的材质
    [SerializeField] private Material defaultMaterial; // 默认材质
    [SerializeField] public AudioClip soundClip; // 音效剪辑
    [SerializeField] private float fadeOutTime = 0.5f; // 淡出时间

    private AudioSource audioSource;
    private Renderer objectRenderer;
    private bool isKeyPressed;
    private float fadeOutTimer;
    private bool isFadingOut;
    private ParticleSystem particleCoarse; // 粒子粗
    private ParticleSystem particleFine; // 粒子细
    private GameObject particleCoarseObject; // 粒子粗的GameObject
    private GameObject particleFineObject; // 粒子细的GameObject

    void Start()
    {
        // 获取或添加 AudioSource 组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = soundClip;
        audioSource.playOnAwake = false;

        // 获取 Renderer 组件
        objectRenderer = GetComponent<Renderer>();

        // 确保默认材质已设置
        if (defaultMaterial != null && objectRenderer != null)
        {
            objectRenderer.material = defaultMaterial;
        }

        // 初始化粒子系统
        InitializeParticleSystems();
    }

    void Update()
    {
        // 检查粒子系统是否需要重新初始化
        if ((particleCoarse == null || particleCoarseObject == null || !particleCoarseObject.activeInHierarchy) ||
            (particleFine == null || particleFineObject == null || !particleFineObject.activeInHierarchy))
        {
            InitializeParticleSystems();
        }

        // 检测按键按下
        if (Input.GetKeyDown(keyToPress) && !isKeyPressed)
        {
            isKeyPressed = true;

            // 切换到激活材质
            if (objectRenderer != null && activeMaterial != null)
            {
                objectRenderer.material = activeMaterial;
            }

            // 播放音效
            if (audioSource != null && soundClip != null)
            {
                audioSource.volume = 0.5f; // 重置音量
                audioSource.Play();
                isFadingOut = false; // 取消淡出状态
            }

            // 播放粒子特效
            if (particleCoarse != null && particleCoarseObject.activeInHierarchy) particleCoarse.Play();
            if (particleFine != null && particleFineObject.activeInHierarchy) particleFine.Play();
        }

        // 检测按键抬起
        if (Input.GetKeyUp(keyToPress) && isKeyPressed)
        {
            ReleaseKey();
        }

        // 处理淡出逻辑
        if (isFadingOut && audioSource != null)
        {
            fadeOutTimer -= Time.deltaTime;
            if (fadeOutTimer > 0)
            {
                // 线性淡出音量
                audioSource.volume = fadeOutTimer / fadeOutTime-0.5f;
            }
            else
            {
                // 淡出完成，停止音效
                audioSource.Stop();
                audioSource.volume = 0.5f; // 重置音量以备下次使用
                isFadingOut = false;
            }
        }
    }

    // 模拟按键按下指定时间
    public void SimulateKeyPress(float duration)
    {
        if (isKeyPressed) return; // 防止重复按下

        isKeyPressed = true;

        // 切换到激活材质
        if (objectRenderer != null && activeMaterial != null)
        {
            objectRenderer.material = activeMaterial;
        }

        // 播放音效
        if (audioSource != null && soundClip != null)
        {
            audioSource.volume = 0.5f; // 重置音量
            audioSource.Play();
            isFadingOut = false; // 取消淡出状态
        }

        // 播放粒子特效
        if (particleCoarse != null && particleCoarseObject.activeInHierarchy) particleCoarse.Play();
        if (particleFine != null && particleFineObject.activeInHierarchy) particleFine.Play();

        // 启动协程以模拟按键释放
        StartCoroutine(ReleaseKeyAfterDuration(duration));
    }

    // 协程：等待指定时间后释放按键
    private IEnumerator ReleaseKeyAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        ReleaseKey();
    }

    // 按键释放逻辑
    private void ReleaseKey()
    {
        isKeyPressed = false;

        // 恢复默认材质
        if (objectRenderer != null && defaultMaterial != null)
        {
            objectRenderer.material = defaultMaterial;
        }

        // 如果音效在播放，开始淡出
        if (audioSource != null && audioSource.isPlaying)
        {
            isFadingOut = true;
            fadeOutTimer = fadeOutTime;
        }

        // 停止粒子特效
        if (particleCoarse != null && particleCoarseObject.activeInHierarchy) particleCoarse.Stop();
        if (particleFine != null && particleFineObject.activeInHierarchy) particleFine.Stop();
    }

    // 初始化粒子系统
    private void InitializeParticleSystems()
    {
        // 查找粒子系统
        foreach (Transform child in transform)
        {
            if (child.name == "粒子粗")
            {
                particleCoarseObject = child.gameObject;
                particleCoarse = child.GetComponent<ParticleSystem>();
                if (particleCoarse != null)
                {
                    var main = particleCoarse.main;
                    main.stopAction = ParticleSystemStopAction.None; // 防止粒子系统自我销毁
                    particleCoarse.Stop(); // 确保初始停止
                }
            }
            else if (child.name == "粒子细")
            {
                particleFineObject = child.gameObject;
                particleFine = child.GetComponent<ParticleSystem>();
                if (particleFine != null)
                {
                    var main = particleFine.main;
                    main.stopAction = ParticleSystemStopAction.None; // 防止粒子系统自我销毁
                    particleFine.Stop(); // 确保初始停止
                }
            }
        }
    }
}