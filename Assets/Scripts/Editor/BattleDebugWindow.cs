using UnityEditor;
using UnityEngine;

public class BattleDebugWindow : EditorWindow
{
    [MenuItem("Tools/전투 디버거")]
    public static void ShowWindow()
    {
        var window = GetWindow<BattleDebugWindow>("전투 디버거");
        window.minSize = new Vector2(300, 150);
    }

    private void OnGUI()
    {
        DrawGameControlPanel();
    }

    private void DrawGameControlPanel()
    {
        GUILayout.Space(10);
        GUILayout.Label("게임 진행 제어 (Play Mode 전용)", EditorStyles.boldLabel);
        GUILayout.Space(5);

        bool isPlayModeValid = Application.isPlaying && BattleManager.Instance != null;
        EditorGUI.BeginDisabledGroup(!isPlayModeValid);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("다음 페이즈로 스킵", GUILayout.Height(40)))
        {
            BattleManager.Instance.SkipToNextPhase();
        }
        if (GUILayout.Button("전투 전체 초기화", GUILayout.Height(40)))
        {
            BattleManager.Instance.ResetBattle();
        }
        GUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("게임 진행 제어는 유니티 Play 중에만 활성화됩니다.", MessageType.Info);
        }
    }
}