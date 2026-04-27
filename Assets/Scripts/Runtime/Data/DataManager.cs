using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public Dictionary<int, SinnerData> SinnerTable = new();
    public Dictionary<int, IdentityData> IdentityTable = new();
    public Dictionary<int, SkillData> SkillTable = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadAllData();
    }

    private void LoadAllData()
    {
        LoadSinner();
        LoadIdentities();
        LoadAllSkills();

        CheckLoadedData();
    }

    private void CheckLoadedData()
    {
        if (SinnerTable.Count > 0)
        {
            var firstSinner = SinnerTable.Values.First();
            Debug.Log($"<color=cyan>[데이터 확인]</color> 첫 번째 수감자: {firstSinner.Name} (ID: {firstSinner.SinnerID})");

            var firstIdentity = IdentityTable.Values.FirstOrDefault(id => id.SinnerID == firstSinner.SinnerID);

            if (firstIdentity != null)
            {
                Debug.Log($"<color=cyan>[데이터 확인]</color> {firstSinner.Name}의 첫 번째 인격: {firstIdentity.Name}");

                // 3. 해당 인격의 첫 번째 스킬 확인 (Skill1_ID 사용)
                int skillIndex = 2;
                int firstSkillID = firstIdentity.SkillIDs[skillIndex];
                if (SkillTable.TryGetValue(firstSkillID, out SkillData skill))
                {
                    Debug.Log($"<color=cyan>[데이터 확인]</color> {skillIndex + 1}번 스킬 이름: {skill.SkillName}");

                    // 4. 스킬 효과 리스트 확인
                    if (skill.Effects != null && skill.Effects.Count > 0)
                    {
                        Debug.Log($"<color=cyan>[데이터 확인]</color> --- '{skill.SkillName}'의 스킬 효과 목록 (총 {skill.Effects.Count}개) ---");

                        for (int i = 0; i < skill.Effects.Count; i++)
                        {
                            var eff = skill.Effects[i];

                            Debug.Log($"<color=cyan>[효과 {i + 1}]</color> " +
                                      $"코인: {eff.CoinIndex} | " +
                                      $"대상: {eff.Target} | " +
                                      $"타이밍: {eff.Timing} | " +
                                      $"행동: {eff.Type} | " +
                                      $"버프: {eff.TargetBuff} | " +
                                      $"수치: {eff.Value}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[데이터 확인] '{skill.SkillName}'의 스킬 효과 리스트가 비어있습니다. 파서(Parser)를 확인하세요.");
                    }
                }
                else
                {
                    Debug.LogError($"[데이터 확인] SkillID {firstSkillID}를 SkillTable에서 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"[데이터 확인] {firstSinner.Name}에 할당된 인격 데이터를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("[데이터 확인] 수감자 테이블이 비어있습니다.");
        }
    }

    private void LoadSinner()
    {
        TextAsset csv = Resources.Load<TextAsset>("Data/Sinners");

        if (csv == null)
        {
            return;
        }

        string[] lines = csv.text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] row = lines[i].Split(',');
            SinnerData data = new SinnerData { SinnerID = int.Parse(row[0]), Name = row[1] };
            
            if (!SinnerTable.ContainsKey(data.SinnerID))
            {
                SinnerTable.Add(data.SinnerID, data);
            }
        }
    }

    private void LoadIdentities()
    {
        TextAsset csv = Resources.Load<TextAsset>("Data/Identities");

        if (csv == null)
        {
            return;
        }

        string[] lines = csv.text.Replace("\r", "").Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] row = lines[i].Split(',');
            IdentityData data = new IdentityData
            { 
                IdentityID = int.Parse(row[0]),
                SinnerID = int.Parse(row[1]),
                Name = row[2],
                MaxHP = int.Parse(row[3]),
                SpeedRange = row[4]
            };

            for (int j = 0; j < data.SkillIDs.Length; j++)
            {
                data.SkillIDs[j] = int.Parse(row[j + 5]);
            }

            if (!IdentityTable.ContainsKey(data.IdentityID))
            {
                IdentityTable.Add(data.IdentityID, data);
            }
        }
    }

    private void LoadAllSkills()
    {
        TextAsset[] skillFiles = Resources.LoadAll<TextAsset>("Data/Skills");
        foreach (var file in skillFiles)
        {
            ParseSkillCSV(file.text);
        }
    }

    private void ParseSkillCSV(string csvText)
    {
        string[] lines = csvText.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] row = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

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

            if (!SkillTable.ContainsKey(skill.SkillID))
            {
                SkillTable.Add(skill.SkillID, skill);
            }
        }
    }
}
