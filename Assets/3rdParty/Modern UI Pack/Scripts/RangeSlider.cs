using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class RangeSlider : MonoBehaviour
    {
        [Header("SETTINGS")]
        [Range(0, 3)] public int DecimalPlaces = 0;
        //public float minValue = 0;
        //public float maxValue = 1;
        public bool useWholeNumbers = false;
        public bool showLabels = true;

        [Header("MIN SLIDER")]
        public RangeMinSlider minSlider;
        public TextMeshProUGUI minSliderLabel;

        [Header("MAX SLIDER")]
        public RangeMaxSlider maxSlider;
        public TextMeshProUGUI maxSliderLabel;
        public string connectomeNumber;

        private bool isFirstUpdate = true;
        private string connectomeName;
        private int isDuplicate;
        private GameObject connectomeParent;
        private float minValue = -1;
        private float maxValue = 1;
        // Properties
        public float CurrentLowerValue
        {
            get { return minSlider.value; }
        }
        public float CurrentUpperValue
        {
            get { return maxSlider.realValue; }
        }

        void Awake()
        {
            // Define if we use indicators:
            connectomeName = PlayerPrefs.GetString("SelectedConnectome_" + connectomeNumber);
            isDuplicate = PlayerPrefs.GetInt("isDuplicate");
            if (showLabels)
            {
                minSlider.label = minSliderLabel;
                minSlider.numberFormat = "n" + DecimalPlaces;
                maxSlider.label = maxSliderLabel;
                maxSlider.numberFormat = "n" + DecimalPlaces;
            }
            else
            {
                minSliderLabel.gameObject.SetActive(false);
                maxSliderLabel.gameObject.SetActive(false);
            }

            // Adjust Max/Min values for both sliders
            minSlider.minValue = minValue;
            minSlider.maxValue = maxValue;
            minSlider.wholeNumbers = useWholeNumbers;

            maxSlider.minValue = minValue;
            maxSlider.maxValue = maxValue;
            maxSlider.wholeNumbers = useWholeNumbers;
            minSlider.value = -1.0f;
            maxSlider.value = 1.0f;
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

                connectomeParent.GetComponent<SingleConnectome>().ThresholdMaxSlider = this.gameObject.transform.Find("Max Slider").gameObject;
                connectomeParent.GetComponent<SingleConnectome>().ThresholdMinSlider = this.gameObject.transform.Find("Min Slider").gameObject;
            }
            else
            {
                connectomeParent.GetComponent<SingleConnectome>().thresholdMax = CurrentUpperValue;
                connectomeParent.GetComponent<SingleConnectome>().thresholdMin = CurrentLowerValue;
            }
        }
    }
}