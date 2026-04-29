[System.Serializable]
public class IdentityData
{
    public int IdentityID;
    public int SinnerID;
    public string Name;
    public int MaxHP;
    public string SpeedRange;

    public int[] SkillIDs = new int[4];

    public (int min, int max) GetSpeed()
    {
        var split = SpeedRange.Split('-');
        return (int.Parse(split[0]), int.Parse(split[1]));
    }
}
