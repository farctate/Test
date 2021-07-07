using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace Demo0
{
    public class ScreenController : DIMonoBehaviour
    {
        public GameObject FiltarableImgPfab;
        //
        [Injected] [SerializeField] private MarkerImgTest _img;
        [Injected] [SerializeField] private MarkerBtnChangeColor _btnChangeColor;
        [Injected] [SerializeField] private MarkerBtnAdd _btnAdd;
        [Injected] [SerializeField] private MarkerBtnRemove _btnRemove;
        [Injected] [SerializeField] private MarkerImgContainer _imgContainer;

        [Injected] [SerializeField] private Filter<ViewImg> _filterViews;


        private void Awake()
        {
            var container = new ContainerScreen(null, transform);
        }

        private void Start()
        {
            _btnChangeColor.GetComponent<Button>().onClick.AddListener(() =>
            {
                _img.GetComponent<Image>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            });

            _btnAdd.GetComponent<Button>().onClick.AddListener(() =>
            {
                Instantiate(FiltarableImgPfab, _imgContainer.transform);
            });

            _btnRemove.GetComponent<Button>().onClick.AddListener(() =>
            {
                if(_filterViews.Count == 0)
                {
                    return;
                }
                Destroy(_filterViews[Random.Range(0, _filterViews.Count)].gameObject);
            });
        }
    } 
}
