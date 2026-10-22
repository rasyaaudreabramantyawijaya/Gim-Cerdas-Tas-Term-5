using System.Collections.Generic;
using UnityEngine;

// Own only newly-created native resources, never shared imported Resources assets.
public sealed class RuntimeResources:MonoBehaviour
{
    readonly List<Object> owned=new List<Object>();
    public static T Own<T>(GameObject owner,T resource) where T:Object
    {
        var component=owner.GetComponent<RuntimeResources>();if(component==null)component=owner.AddComponent<RuntimeResources>();
        if(resource!=null&&!component.owned.Contains(resource))component.owned.Add(resource);return resource;
    }
    void OnDestroy(){foreach(var resource in owned)if(resource!=null)Destroy(resource);}
}
