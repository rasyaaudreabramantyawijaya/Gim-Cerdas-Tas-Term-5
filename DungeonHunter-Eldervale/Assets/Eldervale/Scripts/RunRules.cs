using System;
public static class RunRules
{
    public static bool CanTravel(bool discovered, bool engaged, bool alive, bool busy, bool inWorld)
        => discovered && !engaged && alive && !busy && inWorld;
    public static bool CanOpenSanctum(bool a, bool b) => a && b;
    public static bool CanWin(bool a, bool b, bool guardian, bool artifact, bool atCamp)
        => a && b && guardian && artifact && atCamp;
    public static string SelectMusic(int alertCount, int combatCount, bool boss)
        => boss ? "Boss" : combatCount > 0 ? "Combat" : alertCount > 0 ? "Alert" : "Explore";
    public static bool CanGlide(float groundDistance, bool airborne, bool alive) => groundDistance >= 3f && airborne && alive;
    public static float CameraPitch(float current,float delta,bool inverted)=>UnityEngine.Mathf.Clamp(current+delta*(inverted?-1:1),-12,65);
}
[Serializable] public sealed class RunState
{
    public int version = 1;
    public int character, weapon, upgrade, coins = 100, potions = 2, checkpoint;
    public int xp,level=1,claimedLevel=1;
    public int[] party={0,1,2};
    public float[] health={100,100,100};
    public bool sigilA, sigilB, guardian, artifact;
    public bool[] waypoints = { true, false, false, false };
    public bool Valid => version == 1 && character >= 0 && character < 3 && weapon >= 0 && weapon < 3
        && waypoints != null && waypoints.Length == 4 && upgrade >= 0 && upgrade <= 5 && coins >= 0 && potions >= 0
        && checkpoint>=0&&checkpoint<4&&xp>=0&&level>=1&&claimedLevel>=1&&claimedLevel<=level
        && party!=null&&party.Length==3&&party[0]>=0&&party[0]<3&&party[1]>=0&&party[1]<3&&party[2]>=0&&party[2]<3
        && party[0]!=party[1]&&party[1]!=party[2]&&party[0]!=party[2]
        && health!=null&&health.Length==3&&GoodHealth(health[0])&&GoodHealth(health[1])&&GoodHealth(health[2]);
    static bool GoodHealth(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value)&&value>=0&&value<=100;
}
