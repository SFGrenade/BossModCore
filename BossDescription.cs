using UGameObject = UnityEngine.GameObject;

namespace BossModCore
{
    public class BossDescription
    {
        public string statueName;
        public string statueDescription;
        public bool customScene; // either a prefab, or custom supplied one (name below)
        public string scenePrefabName; // either this or custom supplied one
        public UGameObject statueGo;
    }
}
