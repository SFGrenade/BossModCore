using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UGameObject = UnityEngine.GameObject;

namespace BossModCore
{
    class BossDescription
    {
        public string statueName;
        public string statueDescription;
        public bool customScene; // either a prefab, or custom supplied one (name below)
        public string scenePrefabName; // either this or custom supplied one
        public UGameObject statueGO;
    }
}
