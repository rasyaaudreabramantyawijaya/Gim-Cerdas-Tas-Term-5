using UnityEngine;
public enum UseKind { Gate, Tower, Exit, Sigil, Artifact, Shop, Talk, Altar, Duel, Reward, Drink, Rest }
public sealed class Interactable : MonoBehaviour
{
    public UseKind Kind;
    public int Id;
    public string Label="Interaksi";
    public string DrinkName="Kopi";
    public bool Available => Kind!=UseKind.Sigil || (Id==1?!GameFlow.I.State.sigilA:!GameFlow.I.State.sigilB);
    public void Use(HunterPlayer player)
    {
        var f=GameFlow.I;GameAudio.I?.Cue("interact",transform.position);
        switch(Kind)
        {
            case UseKind.Gate:f.EnterDungeon(Id);break;
            case UseKind.Tower:f.Activate(Id);break;
            case UseKind.Exit:f.ExitDungeon(Id);break;
            case UseKind.Sigil:f.Collect(Id);if(!Available)gameObject.SetActive(false);break;
            case UseKind.Artifact:f.Collect(3);if(f.State.artifact)gameObject.SetActive(false);break;
            case UseKind.Shop:if(!f.Engaged)f.SetScreen(GameScreen.Shop);else f.Note("Pedagang menunggu combat berakhir.");break;
            case UseKind.Talk:f.Talk("Warga Eldervale","Dulu gerbang di pegunungan itu disegel oleh dua sigil. Hati-hati pada para penjaganya.");break;
            case UseKind.Altar:f.Extract();break;
            case UseKind.Reward:f.ClaimRewards();break;
            case UseKind.Duel:
                var e=GetComponent<EnemyAgent>();if(e!=null){e.Dueling=!e.Dueling;e.HP=80;f.Note(e.Dueling?"Latihan dimulai. Bertarung tanpa risiko kematian.":"Latihan dihentikan.");}break;
            case UseKind.Drink:
                player.Heal(15);GameAudio.I?.Cue("heal");
                f.Note(DrinkName=="Air"?"Minum air segar. HP +15.":"Menikmati "+DrinkName+" hangat. HP +15.");
                if(player.Visual!=null)player.Visual.Action("Cast");
                break;
            case UseKind.Rest:
                player.Heal(30);GameAudio.I?.Cue("heal");
                f.Note("Beristirahat sejenak di kasur. HP +30.");
                break;
        }
    }
}
