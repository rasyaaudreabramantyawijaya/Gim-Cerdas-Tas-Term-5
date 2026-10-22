using UnityEngine;
public sealed class WindMotion : MonoBehaviour
{
    public int kind;
    Quaternion rest;
    Vector3 scale;
    float phase;
    void Start(){rest=transform.localRotation;scale=transform.localScale;phase=transform.position.x*.7f+transform.position.z*.3f;}
    void Update()
    {
        float strength=GameFlow.I!=null?GameFlow.I.Wind:1;
        float gust=(Mathf.Sin(Time.unscaledTime*.7f)+Mathf.Sin(Time.unscaledTime*.23f+2))*.5f;
        float wave=Mathf.Sin(Time.unscaledTime*(kind==2?2.4f:1.2f)+phase)*(1+gust*.25f)*strength;
        transform.localRotation=rest*Quaternion.Euler(wave*(kind==2?8:kind==1?2:5),0,wave*(kind==2?3:kind==1?1:4));
    }
}
