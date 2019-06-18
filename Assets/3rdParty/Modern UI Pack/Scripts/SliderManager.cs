using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class SliderManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("TEXTS")]
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI popupValueText;

        [Header("SETTINGS")]
        public bool usePercent = false;
        public bool showValue = true;
        public bool showPopupValue = true;
        public bool useRoundValue = false;
        public string connectomeNumber;
        private bool isDynamic = false;
        public bool isFirstUpdate = true;
        private Slider mainSlider;
        private Animator sliderAnimator;
        private string connectomeName;
        private string sliderName;
        private int isDuplicate;
        private GameObject connectomeParent;
        //public GameObject SpeedText;
        private DataLoader dataLoader;

        void Start()
        {
            dataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
            connectomeName = PlayerPrefs.GetString("SelectedConnectome_" + connectomeNumber);
            isDuplicate = PlayerPrefs.GetInt("isDuplicate");
            mainSlider = this.GetComponent<Slider>();
            sliderAnimator = this.GetComponent<Animator>();
            sliderName = this.name;
            if (showValue == false)
            {
                valueText.enabled = false;
            }

            if (showPopupValue == false)
            {
                popupValueText.enabled = false;
            }
        }

        void Update()
        {
            if (isFirstUpdate)
            {
                isFirstUpdate = false;
                if (isDuplicate == 0)
                    connectomeParent = GameObject.Find(connectomeName);
                else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 0)
                    connectomeParent = GameObject.FindGameObjectWithTag("SingleConnectome");
                else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 1)
                    connectomeParent = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
                isDynamic = dataLoader.isDynamicDictionaryGlobal[connectomeParent.name];
                if(!isDynamic && sliderName == "Speed")
                {
                    this.transform.parent.parent.Find("Title").GetComponent<Text>().color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                    this.transform.Find("Handle Slide Area").transform.Find("Handle").GetComponent<Image>().color = new Color(0.54f, 0.56f, 0.6f, 1.0f);
                    ColorBlock cb = this.transform.GetComponent<Slider>().colors ;
                    cb.normalColor = new Color(0.52f, 0.528f, 0.54f, 1.0f);
                    this.transform.GetComponent<Slider>().colors = cb;
                }
                    

            }
            if (useRoundValue == true)
            {
                if (usePercent == true)
                {
                    valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString() + "%";
                    popupValueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString() + "%";
                }

                else
                {
                    valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
                    popupValueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
                }
            }
            else
            {
                if (usePercent == true)
                {
                    valueText.text = mainSlider.value.ToString("F1") + "%";
                    popupValueText.text = mainSlider.value.ToString("F1") + "%";
                }

                else
                {
                    valueText.text = mainSlider.value.ToString("F1");
                    popupValueText.text = mainSlider.value.ToString("F1");
                }
            }
            if(sliderName == "Opacity")
                connectomeParent.GetComponent<SingleConnectome>().Opacity = float.Parse(valueText.text);
            else if(sliderName == "Glyph")
                connectomeParent.GetComponent<SingleConnectome>().Glyph = float.Parse(valueText.text);
            else if(sliderName == "Speed")
                connectomeParent.GetComponent<SingleConnectome>().Speed = float.Parse(valueText.text);
            else if (sliderName == "Gradient")
                connectomeParent.GetComponent<SingleConnectome>().Gradient = float.Parse(valueText.text);

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (showPopupValue == true)
            {
                sliderAnimator.Play("Value In");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (showPopupValue == true)
            {
                sliderAnimator.Play("Value Out");
            }
        }
    }
}