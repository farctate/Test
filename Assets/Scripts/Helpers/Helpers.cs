using UnityEngine;

namespace UnityDI
{
    public static class Helpers
    {
        public static TComp CreateSceneObject<TComp>(Transform parent)
                where TComp : Component
        {
            var obj = new GameObject();
            obj.transform.parent = parent.transform;
            obj.name = typeof(TComp).Name;
            var comp = obj.AddComponent<TComp>();
            return comp;
        }
    }

}