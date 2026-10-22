using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EldervaleVerification
{
    static int pass;
    static void Check(bool value, string name) { if (!value) throw new Exception("FAIL: " + name); pass++; Debug.Log("PASS: " + name); }
    static object Call(string method, params object[] args)
    {
        var type = Type.GetType("RunRules, Assembly-CSharp");
        Check(type != null, "RunRules available for actual travel and objective decisions");
        var info=type.GetMethod(method,BindingFlags.Public|BindingFlags.Static);Check(info!=null,method+" implementation exists");return info.Invoke(null,args);
    }
    public static void Rules()
    {
        pass=0;
        try
        {
            Check((bool)Call("CanTravel", true, false, true, false, true), "known waypoint available in safe overworld");
            Check(!(bool)Call("CanTravel", false, false, true, false, true), "unknown waypoint rejected");
            Check(!(bool)Call("CanTravel", true, true, true, false, true), "combat rejects teleport");
            Check(!(bool)Call("CanTravel", true, false, false, false, true), "dead player rejects teleport");
            Check(!(bool)Call("CanTravel", true, false, true, true, true), "busy transition rejects duplicate");
            Check(!(bool)Call("CanTravel", true, false, true, false, false), "dungeon map travel rejected");
            Check(!(bool)Call("CanOpenSanctum", true, false), "one sigil cannot open Sanctum");
            Check((bool)Call("CanOpenSanctum", true, true), "two sigils open Sanctum");
            Check(!(bool)Call("CanWin", true, true, false, true, true), "artifact alone cannot skip Guardian");
            Check(!(bool)Call("CanWin", true, true, true, true, false), "extraction required");
            Check((bool)Call("CanWin", true, true, true, true, true), "complete expedition wins at camp");
            Check((string)Call("SelectMusic", 0, 0, false) == "Explore", "safe uses Explore");
            Check((string)Call("SelectMusic", 2, 0, false) == "Alert", "suspicion uses Alert");
            Check((string)Call("SelectMusic", 1, 1, false) == "Combat", "engaged enemy takes priority over alert");
            Check((string)Call("SelectMusic", 0, 1, true) == "Boss", "Guardian priority");
            Check((bool)Call("CanGlide",4f,true,true),"gliding permitted from height");
            Check(!(bool)Call("CanGlide",1f,true,true),"double jump near ground cannot fly");
            Check(!(bool)Call("CanGlide",5f,false,true),"standing high is not airborne");
            Check(!(bool)Call("CanGlide",5f,true,false),"dead character cannot glide");
            Check(new RunState().Valid,"fresh progress is valid");
            Check(!new RunState{party=new[]{0,0,2}}.Valid,"duplicate party members rejected");
            Check(!new RunState{health=new float[]{100,-1,100}}.Valid,"negative health save rejected");
            Check(!new RunState{checkpoint=6}.Valid,"invalid checkpoint rejected");
            Check((float)Call("CameraPitch",10f,0f,true)==10f,"invert Y does not flip stationary camera every frame");
            Check((float)Call("CameraPitch",10f,2f,false)==12f,"normal camera delta");
            Check((float)Call("CameraPitch",10f,2f,true)==8f,"invert applies only to delta");
            Check((float)Call("CameraPitch",64f,8f,false)==65f,"camera pitch clamped");
            var evidence = Path.Combine(Application.dataPath, "../docs/evidence");
            Directory.CreateDirectory(evidence);
            File.WriteAllText(Path.Combine(evidence, "rules-checks.txt"), pass + " PASS; 0 FAIL");
            Debug.Log("ELDERVALE_RULES_SUCCESS " + pass);
        }
        catch (Exception ex) { Debug.LogError(ex); throw; }
    }
}

