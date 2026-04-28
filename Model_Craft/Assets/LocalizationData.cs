using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationData", menuName = "Lego/Localization Data")]
public class LocalizationData : ScriptableObject
{
    [System.Serializable]
    public class LocalizedText
    {
        public string key;
        [TextArea] public string english;
        [TextArea] public string russian;
    }

    public List<LocalizedText> texts = new List<LocalizedText>();
}