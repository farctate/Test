using UnityEngine.UI;
using UnityEngine;
using UnityDI;

namespace Demo0
{
    public class ViewImg : DIMonoBehaviour
    {
        [Injected] public Image Img;

        private void Start()
        {
            Img.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }
}
