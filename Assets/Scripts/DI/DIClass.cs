namespace UnityDI
{
    public class DIClass
    {
        protected DIClass()
        {
            DIEventDispatcher.InvokeOnInit(this, GetType());
        }

        ~DIClass()
        {
            DIEventDispatcher.InvokeOnDispose(this, GetType());
        }
    }
}
