using System;
using UnityDI;
using UnityDI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Demo0
{

    public class ContainerScreen : DIContainer
    {
        public ContainerScreen(DIContainer parent, Transform transform)
        {
            var container = this;

            UnityEngine.Random.InitState(DateTime.UtcNow.Millisecond);

            container.RegisterTransient<Image>((x) => GetComponent<Image>(x));
            //
            container.RegisterTransient<MarkerBtnChangeColor>((x) => GetComponentInChildren<MarkerBtnChangeColor>(x));
            container.RegisterTransient<MarkerImgTest>((x) => GetComponentInChildren<MarkerImgTest>(x));
            container.RegisterTransient<MarkerBtnAdd>((x) => GetComponentInChildren<MarkerBtnAdd>(x));
            container.RegisterTransient<MarkerBtnRemove>((x) => GetComponentInChildren<MarkerBtnRemove>(x));
            container.RegisterTransient<MarkerImgContainer>((x) => GetComponentInChildren<MarkerImgContainer>(x));

            container.RegisterFilter<ViewImg>();
        }

        private TComp GetComponent<TComp>(object obj, Func<Transform, bool> predicate = null)
                    where TComp : Component
        {
            if (obj is DIMonoBehaviour cast)
            {
                return cast.GetComponent<TComp>();
            }

            return null;
        }

        private TComp GetComponentInChildren<TComp>(object obj, Func<GameObject, bool> predicate = null)
               where TComp : Component
        {
            if (obj is DIMonoBehaviour cast)
            {
                var comp = cast.transform.SearchComponent<TComp>(predicate);
                return comp;
            }

            return null;
        }
    } 
}
