using System;

namespace UnityDI
{
    public static class DIEventDispatcher
    {
        public delegate void InitHandler(object sender, Type type);
        public static event InitHandler OnInit;
        public static void InvokeOnInit(object sender, Type type)
        {
            OnInit?.Invoke(sender, type);
        }

        public delegate void DisposeHandler(object sender, Type type);
        public static event DisposeHandler OnDispose;
        public static void InvokeOnDispose(object sender, Type type)
        {
            OnDispose?.Invoke(sender, type);
        }
    }
}
