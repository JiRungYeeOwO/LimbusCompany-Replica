using UnityEngine;
using UnityEditor;

public class SkillStringBuilder : EditorWindow
{
    private int _coinIndex = 1;
    private TargetType _target = TargetType.Target;
    private EffectTiming _timing = EffectTiming.OnHit;
    private string _condition = "";
    private EffectType _effect = EffectType.AddBuffPotency;
    private BuffType _buff = BuffType.None;
    private int _value = 1;
    private string _generatedString = "";

    [MenuItem("Tools/스킬 스트링 빌더")]
    public static void ShowWindow()
    {
        var window = GetWindow<SkillStringBuilder>("스트링 빌더");
        window.minSize = new Vector2(400, 350);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("🪄 스킬 효과 문자열 생성기", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.BeginVertical("box");
        _coinIndex = EditorGUILayout.IntSlider("코인 번호 (0은 스킬)", _coinIndex, 0, 10);
        _target = (TargetType)EditorGUILayout.EnumPopup("대상 지정", _target);
        _timing = (EffectTiming)EditorGUILayout.EnumPopup("발동 타이밍", _timing);

        GUILayout.Space(5);
        _condition = EditorGUILayout.TextField("조건 (선택)", _condition);
        GUILayout.Space(5);

        _effect = (EffectType)EditorGUILayout.EnumPopup("행동 (Effect)", _effect);
        _buff = (BuffType)EditorGUILayout.EnumPopup("버프 (Buff)", _buff);
        _value = EditorGUILayout.IntField("수치", _value);
        GUILayout.EndVertical();

        GUILayout.Space(15);

        string conditionStr = string.IsNullOrEmpty(_condition) ? "" : $"?{_condition}";
        string buffStr = _buff == BuffType.None ? "" : $"_{_buff}";
        _generatedString = $"{_coinIndex}@{_target}@{_timing}{conditionStr}:{_effect}{buffStr}_{_value}";

        GUILayout.Label("생성된 문자열", EditorStyles.boldLabel);
        GUILayout.TextField(_generatedString, EditorStyles.textArea, GUILayout.Height(30));

        GUILayout.Space(10);
        GUIStyle copyBtn = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold };
        if (GUILayout.Button("📋 클립보드 복사", copyBtn, GUILayout.Height(40)))
        {
            GUIUtility.systemCopyBuffer = _generatedString;
            Debug.Log($"<color=#00FFFF>[스트링 빌더]</color> 복사 완료: {_generatedString}");
        }
    }
}