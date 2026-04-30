using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DataManager : MonoSingleton<DataManager>
{
    public Dictionary<int, SinnerData> SinnerTable = new();
    public Dictionary<int, IdentityData> IdentityTable = new();
    public Dictionary<int, SkillData> SkillTable = new();

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        LoadAllData();
    }

    private void LoadAllData()
    {
        LoadSinner();
        LoadIdentities();
        LoadAllSkills();
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

            for (int j = 0; j < data.SkillIDs.Count; j++)
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
namespace Limbus_Replica.Runtime.Managers
{

}

