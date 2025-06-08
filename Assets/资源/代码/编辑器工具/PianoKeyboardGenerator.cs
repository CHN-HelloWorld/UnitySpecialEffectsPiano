using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PianoKeyGenerator : EditorWindow
{
    GameObject whiteKeyPrefab;
    GameObject blackKeyPrefab;
    float spacing = 2.4f;
    float heightOffset = 1.5f;
    float zOffset = 3.5f;
    Transform parentTransform;

    // PyGame 键表转换为 Unity KeyCode
    private readonly Dictionary<string, KeyCode> pygameToUnityKeyMap = new Dictionary<string, KeyCode>
    {
        {"F1", KeyCode.F1}, {"F2", KeyCode.F2}, {"F3", KeyCode.F3}, {"F4", KeyCode.F4},
        {"F5", KeyCode.F5}, {"F6", KeyCode.F6}, {"F7", KeyCode.F7}, {"F9", KeyCode.F9},
        {"F10", KeyCode.F10}, {"F11", KeyCode.F11}, {"F12", KeyCode.F12},
        {"BACKQUOTE", KeyCode.BackQuote}, {"1", KeyCode.Alpha1}, {"2", KeyCode.Alpha2},
        {"3", KeyCode.Alpha3}, {"4", KeyCode.Alpha4}, {"5", KeyCode.Alpha5},
        {"6", KeyCode.Alpha6}, {"7", KeyCode.Alpha7}, {"8", KeyCode.Alpha8},
        {"9", KeyCode.Alpha9}, {"0", KeyCode.Alpha0}, {"MINUS", KeyCode.Minus},
        {"EQUALS", KeyCode.Equals}, {"BACKSPACE", KeyCode.Backspace},
        {"KP_DIVIDE", KeyCode.KeypadDivide}, {"KP_MULTIPLY", KeyCode.KeypadMultiply},
        {"KP_MINUS", KeyCode.KeypadMinus}, {"TAB", KeyCode.Tab}, {"q", KeyCode.Q},
        {"w", KeyCode.W}, {"e", KeyCode.E}, {"r", KeyCode.R}, {"t", KeyCode.T},
        {"y", KeyCode.Y}, {"u", KeyCode.U}, {"i", KeyCode.I}, {"o", KeyCode.O},
        {"p", KeyCode.P}, {"LEFTBRACKET", KeyCode.LeftBracket}, {"RIGHTBRACKET", KeyCode.RightBracket},
        {"BACKSLASH", KeyCode.Backslash}, {"KP7", KeyCode.Keypad7}, {"KP8", KeyCode.Keypad8},
        {"KP9", KeyCode.Keypad9}, {"KP_PLUS", KeyCode.KeypadPlus}, {"a", KeyCode.A},
        {"s", KeyCode.S}, {"d", KeyCode.D}, {"f", KeyCode.F}, {"g", KeyCode.G},
        {"h", KeyCode.H}, {"j", KeyCode.J}, {"k", KeyCode.K}, {"l", KeyCode.L},
        {"SEMICOLON", KeyCode.Semicolon}, {"QUOTE", KeyCode.Quote}, {"RETURN", KeyCode.Return},
        {"KP4", KeyCode.Keypad4}, {"KP5", KeyCode.Keypad5}, {"KP6", KeyCode.Keypad6},
        {"LSHIFT", KeyCode.LeftShift}, {"z", KeyCode.Z}, {"x", KeyCode.X}, {"c", KeyCode.C},
        {"v", KeyCode.V}, {"b", KeyCode.B}, {"n", KeyCode.N}, {"m", KeyCode.M},
        {"COMMA", KeyCode.Comma}, {"PERIOD", KeyCode.Period}, {"SLASH", KeyCode.Slash},
        {"RSHIFT", KeyCode.RightShift}, {"KP1", KeyCode.Keypad1}, {"KP2", KeyCode.Keypad2},
        {"KP3", KeyCode.Keypad3}, {"KP_ENTER", KeyCode.KeypadEnter}, {"LCTRL", KeyCode.LeftControl},
        {"LALT", KeyCode.LeftAlt}, {"SPACE", KeyCode.Space}, {"RALT", KeyCode.RightAlt},
        {"RCTRL", KeyCode.RightControl}, {"LEFT", KeyCode.LeftArrow}, {"UP", KeyCode.UpArrow},
        {"DOWN", KeyCode.DownArrow}, {"RIGHT", KeyCode.RightArrow}, {"KP0", KeyCode.Keypad0},
        {"KP_PERIOD", KeyCode.KeypadPeriod}
    };

    // PyGame 键表（顺序与提供的一致）
    private readonly string[] keyList = new string[]
    {
        "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F9", "F10", "F11", "F12",
        "BACKQUOTE", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "MINUS", "EQUALS",
        "BACKSPACE", "KP_DIVIDE", "KP_MULTIPLY", "KP_MINUS", "TAB", "q", "w", "e", "r",
        "t", "y", "u", "i", "o", "p", "LEFTBRACKET", "RIGHTBRACKET", "BACKSLASH",
        "KP7", "KP8", "KP9", "KP_PLUS", "a", "s", "d", "f", "g", "h", "j", "k", "l",
        "SEMICOLON", "QUOTE", "RETURN", "KP4", "KP5", "KP6", "LSHIFT", "z", "x", "c",
        "v", "b", "n", "m", "COMMA", "PERIOD", "SLASH", "RSHIFT", "KP1", "KP2", "KP3",
        "KP_ENTER", "LCTRL", "LALT", "SPACE", "RALT", "RCTRL", "LEFT", "UP", "DOWN",
        "RIGHT", "KP0", "KP_PERIOD"
    };

    [MenuItem("Tools/Piano Key Generator")]
    public static void ShowWindow()
    {
        GetWindow<PianoKeyGenerator>("Piano Key Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("钢琴琴键生成器", EditorStyles.boldLabel);

        whiteKeyPrefab = (GameObject)EditorGUILayout.ObjectField("白键预制体", whiteKeyPrefab, typeof(GameObject), false);
        blackKeyPrefab = (GameObject)EditorGUILayout.ObjectField("黑键预制体", blackKeyPrefab, typeof(GameObject), false);
        spacing = EditorGUILayout.FloatField("白键间距", spacing);
        heightOffset = EditorGUILayout.FloatField("黑键高度偏移", heightOffset);
        zOffset = EditorGUILayout.FloatField("黑键Z轴偏移", zOffset);
        parentTransform = (Transform)EditorGUILayout.ObjectField("父变换", parentTransform, typeof(Transform), true);

        if (GUILayout.Button("生成琴键"))
        {
            GenerateKeys();
        }
    }

    void GenerateKeys()
    {
        if (whiteKeyPrefab == null || blackKeyPrefab == null)
        {
            Debug.LogError("请指定白键和黑键预制体！");
            return;
        }

        if (whiteKeyPrefab.GetComponent<KeyMaterialSound>() == null)
        {
            Debug.LogError("白键预制体缺少 KeyMaterialSound 组件！");
            return;
        }
        if (blackKeyPrefab.GetComponent<KeyMaterialSound>() == null)
        {
            Debug.LogError("黑键预制体缺少 KeyMaterialSound 组件！");
            return;
        }

        if (parentTransform == null)
        {
            GameObject parentObj = new GameObject("PianoKeys");
            parentTransform = parentObj.transform;
            Undo.RegisterCreatedObjectUndo(parentObj, "Create PianoKeys Parent");
        }

        // 验证键表长度
        if (keyList.Length != 88)
        {
            Debug.LogError($"键表长度错误！预期 88，实际 {keyList.Length}");
            return;
        }

        // 收集白键和黑键的索引
        List<int> whiteKeyIndices = new List<int>();
        List<int> blackKeyIndices = new List<int>();
        for (int i = 0; i < 88; i++)
        {
            if (IsWhiteKey(i))
                whiteKeyIndices.Add(i);
            else
                blackKeyIndices.Add(i);
        }

        // 验证白键和黑键总数
        if (whiteKeyIndices.Count + blackKeyIndices.Count != 88)
        {
            Debug.LogError("白键和黑键总数不匹配 88！");
            return;
        }

        // 键位分配：优先白键
        List<KeyCode> availableKeys = new List<KeyCode>();
        foreach (string key in keyList)
        {
            if (pygameToUnityKeyMap.TryGetValue(key, out KeyCode keyCode))
            {
                availableKeys.Add(keyCode);
            }
            else
            {
                Debug.LogWarning($"无法映射 PyGame 键: {key}");
            }
        }

        if (availableKeys.Count < 88)
        {
            Debug.LogError($"可用键位不足！需要 88 个，实际 {availableKeys.Count}");
            return;
        }

        // 分配键位：先白键，后黑键
        Dictionary<int, KeyCode> keyAssignments = new Dictionary<int, KeyCode>();
        int keyIndex = 0;

        // 为白键分配键位
        foreach (int index in whiteKeyIndices)
        {
            if (keyIndex < availableKeys.Count)
            {
                keyAssignments[index] = availableKeys[keyIndex];
                keyIndex++;
            }
        }

        // 为黑键分配剩余键位
        foreach (int index in blackKeyIndices)
        {
            if (keyIndex < availableKeys.Count)
            {
                keyAssignments[index] = availableKeys[keyIndex];
                keyIndex++;
            }
        }

        int whiteKeyCount = 0; // 跟踪白键数量
        for (int i = 0; i < 88; i++)
        {
            bool isWhite = IsWhiteKey(i);
            GameObject prefab = isWhite ? whiteKeyPrefab : blackKeyPrefab;
            Vector3 position;

            if (isWhite)
            {
                position = new Vector3(whiteKeyCount * spacing, 0, 0);
                whiteKeyCount++;
            }
            else
            {
                float blackKeyX = (whiteKeyCount - 1) * spacing + spacing * 0.5f;
                position = new Vector3(blackKeyX, heightOffset, zOffset);
            }

            // 实例化预制体并记录 Undo
            GameObject key = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parentTransform);
            Undo.RegisterCreatedObjectUndo(key, $"Create Key_{i:D2}");
            key.transform.position = position;
            key.transform.rotation = Quaternion.Euler(0, 270, 0);
            key.name = $"Key_{i:D2}_{(isWhite ? "White" : "Black")}";

            // 为 KeyMaterialSound 组件设置属性
            KeyMaterialSound keySound = key.GetComponent<KeyMaterialSound>();
            if (keySound != null)
            {
                // 设置音效
                int noteNumber = i + 21;
                string audioPath = noteNumber <= 99
                    ? $"资源/音效/German Concert D 0{noteNumber:D2} 083"
                    : $"资源/音效/German Concert D {noteNumber:D3} 083";
                string fullPath = $"Assets/{audioPath}.wav";
                Debug.Log($"尝试加载音效文件: {fullPath} for Key_{i:D2}");

                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(fullPath);
                if (clip != null)
                {
                    Undo.RecordObject(keySound, $"Set soundClip for Key_{i:D2}");
                    keySound.soundClip = clip;
                }
                else
                {
                    Debug.LogWarning($"未找到音效文件: {fullPath} for Key_{i:D2}");
                }

                // 设置键位
                if (keyAssignments.TryGetValue(i, out KeyCode assignedKey))
                {
                    Undo.RecordObject(keySound, $"Set keyToPress for Key_{i:D2}");
                    keySound.keyToPress = assignedKey;
                    Debug.Log($"为 Key_{i:D2} 设置键位: {assignedKey}");
                }
                else
                {
                    Debug.LogWarning($"无法为 Key_{i:D2} 分配键位！");
                }

                EditorUtility.SetDirty(keySound); // 标记组件为已修改
            }
            else
            {
                Debug.LogWarning($"琴键 {key.name} 缺少 KeyMaterialSound 组件！");
            }
        }

        Debug.Log("琴键生成成功！");
    }

    bool IsWhiteKey(int i)
    {
        if (i < 3)
        {
            return i == 0 || i == 2; // A0 (白), A#0 (黑), B0 (白)
        }
        else
        {
            int j = (i - 3) % 12;
            return j == 0 || j == 2 || j == 4 || j == 5 || j == 7 || j == 9 || j == 11;
            // C (白), C# (黑), D (白), D# (黑), E (白), F (白), F# (黑), G (白), G# (黑), A (白), A# (黑), B (白)
        }
    }
}