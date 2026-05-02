using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using UnityEngine;

public class CustomVector3ContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        
        if(property.Readable && !property.Writable)
        property.ShouldSerialize = instance => false;
        
        if(property.DeclaringType == typeof(Vector3) &&
        (property.PropertyName == "normalized" || property.PropertyName == "magnitude" || property.PropertyName == "sqrMagnitude"))
        property.ShouldSerialize = instance => false;
        
        return property;
    }
}