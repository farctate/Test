using System.Collections.Generic;

namespace UnityDI
{
    public interface IFilter
    {
        void Add(object obj);
        void Remove(object obj);
    }

    public class Filter<T> : List<T>, IFilter
        where T : class
    {
        public Filter() {}

        public void Add(object obj)
        {
            if(obj is T cast)
            {
                base.Add(cast);
                return;
            }

            throw new System.Exception($"{obj} is not of type {typeof(T)}");
        }

        public void Remove(object obj)
        {
            if (obj is T cast)
            {
                base.Remove(cast);
                return;
            }

            throw new System.Exception($"{obj} is not of type {typeof(T)}"); ;
        }
    }
}
