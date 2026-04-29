using UnityEngine;
using UnityEditor;

public class GameDashboard : EditorWindow
{
    [MenuItem("Tools/통합 대시보드")]
    public static void ShowWindow()
    {
        var window = GetWindow<GameDashboard>("대시보드");
        window.minSize = new Vector2(300, 200);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("통합 컨트롤 패널", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.BeginVertical("box");
        GUILayout.Label("세이브 & 메모리", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (GUILayout.Button("PlayerPrefs 초기화 (세이브 삭제)", GUILayout.Height(30)))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            CustomLogger.Warn("[대시보드] PlayerPrefs 초기화 완료");
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Once 로그 캐시 비우기", GUILayout.Height(30)))
        {
            CustomLogger.OnceClear();
            CustomLogger.LogSystem("[대시보드] Once 로그 캐시 클리어");
        }
        GUILayout.EndVertical();
    }
}