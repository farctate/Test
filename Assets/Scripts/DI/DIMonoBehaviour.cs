using UnityEngine;

namespace UnityDI
{
    public class DIMonoBehaviour : MonoBehaviour
    {
        protected void OnEnable()
        {
            DIEventDispatcher.InvokeOnInit(this, GetType());
            OnEnabled();
        }

        protected void OnDisable()
        {
            DIEventDispatcher.InvokeOnDispose(this, GetType());
            OnDisabled();
        }

        protected virtual void OnEnabled() {}

        protected virtual void OnDisabled() {}
    }
}
