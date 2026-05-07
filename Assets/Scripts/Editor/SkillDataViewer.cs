using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkillDataViewer : EditorWindow
{
    private Dictionary<string, SkillData> _globalSkillTable = new Dictionary<string, SkillData>();

    private Dictionary<string, List<SkillData>> _skillsByFile = new Dictionary<string, List<SkillData>>();
    private List<string> _validationLogs = new List<string>();

    private Vector2 _csvScrollPos;
    private Vector2 _listScrollPos;
    private Vector2 _detailScrollPos;
    private Vector2 _logScrollPos;

    private string _selectedCsvName = "";
    private SkillData _selectedSkill;

    private SkillData _testSkillA;
    private SkillData _testSkillB;
    private int _testSpA = 0;
    private int _testSpB = 0;
    private string _simLog = "스킬을 할당하고 시뮬레이션을 실행하세요.";
    private Vector2 _simScrollPos;

    [MenuItem("Tools/스킬 데이터 뷰어 및 검증기")]
    public static void ShowWindow()
    {
        var window = GetWindow<SkillDataViewer>("스킬 뷰어");
        window.minSize = new Vector2(1000, 600);
    }

    private void OnGUI()
    {
        DrawToolbar();
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        DrawCsvListPanel();
        DrawSkillListPanel();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DrawSkillDetailPanel();

        GUILayout.Space(5);
        DrawSimulatorPanel();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        DrawValidationLogPanel();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        bool isLoadClicked = GUILayout.Button("데이터 로드 및 검증", EditorStyles.toolbarButton, GUILayout.Width(250));
        GUILayout.FlexibleSpace();
        bool isClearClicked = GUILayout.Button("초기화", EditorStyles.toolbarButton, GUILayout.Width(100));

        GUILayout.EndHorizontal();

        if (isLoadClicked)
        {
            LoadAndValidateData();
            GUIUtility.ExitGUI();
        }

        if (isClearClicked)
        {
            _globalSkillTable.Clear();
            _skillsByFile.Clear();
            _validationLogs.Clear();
            _selectedCsvName = "";
            _selectedSkill = null;
            GUIUtility.ExitGUI();
        }
    }

    private void DrawCsvListPanel()
    {
        GUILayout.BeginVertical("box", GUILayout.Width(180));
        GUILayout.Label("로드된 파일", EditorStyles.boldLabel);

        _csvScrollPos = GUILayout.BeginScrollView(_csvScrollPos);

        if (_skillsByFile.Count == 0)
        {
            GUILayout.Label("데이터 없음", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            foreach (var kvp in _skillsByFile)
            {
                string fileName = kvp.Key;
                GUIStyle btnStyle = (_selectedCsvName == fileName) ? CustomStyles.SelectedButton : GUI.skin.button;

                if (GUILayout.Button(fileName, btnStyle))
                {
                    _selectedCsvName = fileName;
                    _selectedSkill = null;
                    GUI.FocusControl(null);
                }
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawSkillListPanel()
    {
        GUILayout.BeginVertical("box", GUILayout.Width(220));
        GUILayout.Label("스킬 목록", EditorStyles.boldLabel);

        _listScrollPos = GUILayout.BeginScrollView(_listScrollPos);

        if (string.IsNullOrEmpty(_selectedCsvName) || !_skillsByFile.ContainsKey(_selectedCsvName))
        {
            GUILayout.Label("파일을 선택하세요.", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            List<SkillData> currentFileSkills = _skillsByFile[_selectedCsvName];

            foreach (var skill in currentFileSkills)
            {
                GUIStyle btnStyle = (_selectedSkill != null && _selectedSkill.SkillID == skill.SkillID)
                                    ? CustomStyles.SelectedButton : GUI.skin.button;

                if (GUILayout.Button($"[{skill.SkillID}] {skill.SkillName}", btnStyle))
                {
                    _selectedSkill = skill;
                    GUI.FocusControl(null);
                }
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawSkillDetailPanel()
    {
        GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
        GUILayout.Label("스킬 상세 정보", EditorStyles.boldLabel);

        _detailScrollPos = GUILayout.BeginScrollView(_detailScrollPos);

        if (_selectedSkill != null)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"이름: {_selectedSkill.SkillName} (ID: {_selectedSkill.SkillID})", EditorStyles.largeLabel);
            GUILayout.Label($"타입: {_selectedSkill.SkillType} | 속성: {_selectedSkill.SinAttribute}");
            GUILayout.Label($"기본 위력: {_selectedSkill.BasePower} | 코인 위력: {_selectedSkill.CoinPower}");
            GUILayout.Label($"공격 가중치: {_selectedSkill.AttackWeight} | 코인 갯수: {_selectedSkill.CoinCount}");
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label("코인 및 스킬 효과", EditorStyles.boldLabel);

            if (_selectedSkill.Effects != null && _selectedSkill.Effects.Count > 0)
            {
                for (int i = 0; i < _selectedSkill.Effects.Count; i++)
                {
                    var eff = _selectedSkill.Effects[i];
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    string targetStr = eff.Target == TargetType.Self ? "자신에게" : $"[{eff.Target}]에게";
                    string timingStr = $"[{eff.Timing}]";
                    string actionStr = $"{eff.Type} (수치: {eff.Value})";
                    string buffStr = eff.TargetBuff != BuffType.None ? $" <color=#ffaaaa>[{eff.TargetBuff}]</color>" : "";

                    GUIStyle richTextStyle = new GUIStyle(GUI.skin.label) { richText = true };

                    GUILayout.Label($"<b>코인 {eff.CoinIndex}</b> | {timingStr} | {targetStr}", richTextStyle);
                    GUILayout.Label($"{actionStr}{buffStr}", richTextStyle);

                    GUILayout.EndVertical();
                }
            }
            else
            {
                GUILayout.Label("등록된 스킬 효과가 없습니다.", EditorStyles.miniLabel);
            }
        }
        else
        {
            GUILayout.Label("왼쪽 목록에서 스킬을 선택하세요.", EditorStyles.centeredGreyMiniLabel);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawValidationLogPanel()
    {
        GUILayout.BeginVertical("box", GUILayout.Height(150));
        GUILayout.Label("데이터 검증 결과 (Validator)", EditorStyles.boldLabel);

        _logScrollPos = GUILayout.BeginScrollView(_logScrollPos);

        if (_validationLogs.Count == 0)
        {
            GUILayout.Label("검증 대기 중... 또는 오류 없음", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            GUIStyle logStyle = new GUIStyle(GUI.skin.label) { richText = true };
            foreach (var log in _validationLogs)
            {
                GUILayout.Label(log, logStyle);
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void LoadAndValidateData()
    {
        _globalSkillTable.Clear();
        _skillsByFile.Clear();
        _validationLogs.Clear();
        _selectedCsvName = "";
        _selectedSkill = null;

        TextAsset[] csvFiles = Resources.LoadAll<TextAsset>("Data/Skills");

        if (csvFiles == null || csvFiles.Length == 0)
        {
            _validationLogs.Add("<color=red>[Error] 'Resources/Data/Skills' 폴더에서 스킬 데이터를 찾을 수 없습니다.</color>");
            return;
        }

        int totalSuccessCount = 0;

        foreach (TextAsset csv in csvFiles)
        {
            string[] lines = csv.text.Replace("\r", "").Split('\n');
            int fileSuccessCount = 0;

            _skillsByFile[csv.name] = new List<SkillData>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] row = System.Text.RegularExpressions.Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                if (row.Length < 9)
                {
                    _validationLogs.Add($"<color=orange>[Warn] {csv.name}.csv {i + 1}번째 줄: 데이터 칸이 부족합니다.</color>");
                    continue;
                }

                try
                {
                    SkillData skill = new SkillData
                    {
                        SkillID = int.Parse(row[0]),
                        SkillName = row[1].Replace("\"", ""),
                        SkillType = row[2],
                        SinAttribute = row[3],
                        BasePower = int.Parse(row[4]),
                        CoinPower = int.Parse(row[5]),
                        AttackWeight = int.Parse(row[6]),
                        CoinCount = int.Parse(row[7])
                    };

                    skill.Effects = SkillParser.ParseFullEffectString(row[8]);

                    if (!string.IsNullOrWhiteSpace(row[8]) && skill.Effects.Count == 0)
                    {
                        _validationLogs.Add($"<color=red>[Error] [{csv.name}] '{skill.SkillName}' 스킬 효과 파싱 실패 (문법 오류)</color>");
                    }

                    string stringID = row[0];
                    if (!_globalSkillTable.ContainsKey(stringID))
                    {
                        _globalSkillTable.Add(stringID, skill);
                        _skillsByFile[csv.name].Add(skill);
                        fileSuccessCount++;
                        totalSuccessCount++;
                    }
                    else
                    {
                        _validationLogs.Add($"<color=red>[Error] 스킬 ID 중복 발생! [{csv.name}]의 ID {stringID}가 이미 다른 파일에서 사용되었습니다.</color>");
                    }
                }
                catch (Exception e)
                {
                    _validationLogs.Add($"<color=red>[Error] {csv.name}.csv {i + 1}번째 줄 파싱 중 치명적 오류: {e.Message}</color>");
                }
            }

            _validationLogs.Add($"<color=#00C853>[Info] {csv.name} 로드 완료 ({fileSuccessCount}개)</color>");
        }

        if (csvFiles.Length > 0)
        {
            _selectedCsvName = csvFiles[0].name;
        }

        _validationLogs.Add($"<color=#00C853><b>[Success] 총 {csvFiles.Length}개 파일에서 {totalSuccessCount}개의 스킬 로드 및 파싱 완료!</b></color>");
    }

    private void DrawSimulatorPanel()
    {
        GUILayout.BeginVertical("box", GUILayout.Height(220));
        GUILayout.Label("⚔️ 합(Clash) 시뮬레이터", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button($"A에 할당\n{(_testSkillA != null ? _testSkillA.SkillName : "미할당")}", GUILayout.Height(40)))
        {
            if (_selectedSkill != null) _testSkillA = _selectedSkill;
        }
        if (GUILayout.Button($"B에 할당\n{(_testSkillB != null ? _testSkillB.SkillName : "미할당")}", GUILayout.Height(40)))
        {
            if (_selectedSkill != null) _testSkillB = _selectedSkill;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        _testSpA = EditorGUILayout.IntSlider("A 정신력(SP)", _testSpA, -45, 45);
        _testSpB = EditorGUILayout.IntSlider("B 정신력(SP)", _testSpB, -45, 45);
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUI.BeginDisabledGroup(_testSkillA == null || _testSkillB == null);
        if (GUILayout.Button("🔥 합(Clash) 결과 계산하기", GUILayout.Height(30)))
        {
            RunSimulation();
        }
        EditorGUI.EndDisabledGroup();

        _simScrollPos = GUILayout.BeginScrollView(_simScrollPos, EditorStyles.helpBox);
        GUILayout.Label(_simLog, new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true });
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void RunSimulation()
    {
        int finalPowerA = _testSkillA.BasePower;
        int finalPowerB = _testSkillB.BasePower;

        List<string> coinResultsA = new List<string>();
        List<string> coinResultsB = new List<string>();

        float headsProbA = Mathf.Clamp(50f + _testSpA, 5f, 95f);
        for (int i = 0; i < _testSkillA.CoinCount; i++)
        {
            bool isHeads = UnityEngine.Random.Range(0f, 100f) <= headsProbA;
            if (isHeads) finalPowerA += _testSkillA.CoinPower;
            coinResultsA.Add(isHeads ? "<b><color=#00C853>[앞]</color></b>" : "<color=gray>[뒤]</color>");
        }

        float headsProbB = Mathf.Clamp(50f + _testSpB, 5f, 95f);
        for (int i = 0; i < _testSkillB.CoinCount; i++)
        {
            bool isHeads = UnityEngine.Random.Range(0f, 100f) <= headsProbB;
            if (isHeads) finalPowerB += _testSkillB.CoinPower;
            coinResultsB.Add(isHeads ? "<b><color=#00C853>[앞]</color></b>" : "<color=gray>[뒤]</color>");
        }

        string winner = finalPowerA > finalPowerB ? "<color=cyan>A 승리!</color>" : (finalPowerB > finalPowerA ? "<color=red>B 승리!</color>" : "무승부");

        _simLog = $"<b>[A 결과]</b> 위력: {finalPowerA} | 동전: {string.Join(" ", coinResultsA)}\n";
        _simLog += $"<b>[B 결과]</b> 위력: {finalPowerB} | 동전: {string.Join(" ", coinResultsB)}\n\n";
        _simLog += $"<b>최종 결과: {winner}</b>";
    }

    private static class CustomStyles
    {
        private static GUIStyle _selectedButton;
        public static GUIStyle SelectedButton
        {
            get
            {
                if (_selectedButton == null)
                {
                    _selectedButton = new GUIStyle(GUI.skin.button);
                    _selectedButton.normal.textColor = Color.cyan;
                    _selectedButton.fontStyle = FontStyle.Bold;
                }
                return _selectedButton;
            }
        }
    }
}
