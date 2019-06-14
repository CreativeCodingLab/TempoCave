using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Linq;

namespace Michsky.UI.ModernUIPack
{
    public class CustomDropdown : MonoBehaviour
    {
        [Header("OBJECTS")]
        public GameObject triggerObject;
        public TextMeshProUGUI selectedText;
        public Image selectedImage;
        public Transform itemParent;
        public GameObject itemObject;
        public GameObject scrollbar;
        public GameObject canvasForMenu;
        private VerticalLayoutGroup itemList;

        [Header("SETTINGS")]
        public bool enableIcon = true;
        public bool enableTrigger = true;
        public bool enableScrollbar = true;
        public bool invokeAtStart = false;
        public bool isFirstUpdate = true;
        public AnimationType animationType;
        public string connectomeNumber;

        [SerializeField]
        private List<DropdownOptions> dropdownItems = new List<DropdownOptions>();
        //private List<Weapon> imageList = new List<Weapon>();
        public int selectedItemIndex = 0;
   

        private Animator dropdownAnimator;
        private TextMeshProUGUI setItemText;
        private Image setItemImage;

        Sprite imageHelper;
        string textHelper;
        bool isOn;

        string dropdownMenuName;

        private DataLoader connectomeDataLoader;
        private DesktopController DC;
        private GameObject connectomeParent;
        private string[] _preLegendList;
        private int isDuplicate;

        string connectomeName;

        public enum AnimationType
        {
            FADING,
            SLIDING,
            STYLISH
        }

        [System.Serializable]
        public  class DropdownOptions
        {
            public string itemName;
            public UnityEvent OnItemSelection;
        }
        void test()
        {
            Debug.Log("dff");
        }
        void Start()
        {
    
            dropdownAnimator = this.GetComponent<Animator>();
            itemList = itemParent.GetComponent<VerticalLayoutGroup>();
            connectomeName = PlayerPrefs.GetString("SelectedConnectome_" + connectomeNumber);
            isDuplicate = PlayerPrefs.GetInt("isDuplicate");
            connectomeDataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
            DC = GameObject.Find("Canvas").GetComponent<DesktopController>();
            string[] representations = connectomeDataLoader.connectomeRepresentationGlobalDictionary[connectomeName].ToArray();
            string[] classifications = connectomeDataLoader.connectomeClassificationGlobalDictionary[connectomeName].ToArray();
           
            dropdownMenuName = this.transform.name;

            if (dropdownMenuName == "Representation")
                for (int i = 0; i < representations.Length; i++)
                {
                    int x = i;
                    dropdownItems[i].itemName = representations[i];
                    dropdownItems[i].OnItemSelection = new UnityEvent();
                    dropdownItems[i].OnItemSelection.AddListener(() => ChangeDropdownInfo(x));
                }

            else if (dropdownMenuName == "Classification")
                for (int i = 0; i < classifications.Length; i++)
                {
                    int x = i;
                    dropdownItems[i].itemName = classifications[i];
                    dropdownItems[i].OnItemSelection = new UnityEvent();
                    dropdownItems[i].OnItemSelection.AddListener(() => ChangeDropdownInfo(x));
                }

            for (int i = 0; i < dropdownItems.Count; i++)
            {
                GameObject go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(itemParent, false);

                setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                textHelper = dropdownItems[i].itemName;
                setItemText.text = textHelper;

                //Transform goImage;
                //goImage = go.gameObject.transform.Find("Icon");
                //setItemImage = goImage.GetComponent<Image>();
                ////imageHelper = dropdownItems[i].itemIcon;
                //setItemImage.sprite = imageHelper;

                Button itemButton;
                itemButton = go.GetComponent<Button>();
                itemButton.onClick.AddListener(dropdownItems[i].OnItemSelection.Invoke);
                itemButton.onClick.AddListener(Animate);

                //if (invokeAtStart == true)
                //{
                //    dropdownItems[i].OnItemSelection.Invoke();
                //}
            }

            selectedText.text = dropdownItems[selectedItemIndex].itemName;
            //selectedImage.sprite = dropdownItems[selectedItemIndex].itemIcon;

            if (enableScrollbar == true)
            {
                itemList.padding.right = 25;
                scrollbar.SetActive(true);
            }

            else
            {
                itemList.padding.right = 8;
                Destroy(scrollbar);
            }

            if (enableIcon == false)
            {
                selectedImage.enabled = false;
            }
        }

        void Update()
        {

            if(isFirstUpdate)
            {
                if (isDuplicate == 0)
                    connectomeParent = GameObject.Find(connectomeName);
                else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 0)
                    connectomeParent = GameObject.FindGameObjectWithTag("SingleConnectome");
                else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 1)
                    connectomeParent = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");

                _preLegendList = connectomeParent.GetComponent<SingleConnectome>().ModuleList.ToArray();
                isFirstUpdate = !isFirstUpdate;
            }

          
            string[] LegendList = connectomeParent.GetComponent<SingleConnectome>().ModuleList.ToArray();
            if ( !_preLegendList.SequenceEqual(LegendList))
            {
                if (dropdownMenuName == "Legend")
                {
                    _preLegendList = LegendList;
                    //dropdownItems.Clear();
                    for (int i = 0; i < LegendList.Length; i++)
                    {
                        int x = i;
                        dropdownItems[i].itemName = LegendList[i];
                        dropdownItems[i].OnItemSelection = new UnityEvent();
                        dropdownItems[i].OnItemSelection.AddListener(() => ChangeDropdownInfo(x));
                    }

                    for (int i = 0; i < dropdownItems.Count; i++)
                    {
                        GameObject go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                        go.transform.SetParent(itemParent, false);

                        setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                        textHelper = dropdownItems[i].itemName;
                        setItemText.text = textHelper;

                        Button itemButton;
                        itemButton = go.GetComponent<Button>();
                        itemButton.onClick.AddListener(dropdownItems[i].OnItemSelection.Invoke);
                        itemButton.onClick.AddListener(Animate);
                    }
                }
            }
            

        }
        public void ChangeDropdownInfo(int itemIndex)
        {
          
            selectedText.text = dropdownItems[itemIndex].itemName;
            selectedItemIndex = itemIndex;
            
            if (dropdownMenuName == "Representation")
                DC.ChangeRepresentation(connectomeParent, selectedText.text);
            else if (dropdownMenuName == "Classification")
                DC.ChangeClassification(connectomeParent, selectedText.text);
            
        }

       

        public void Animate()
        {
            if (isOn == false && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading In");
                isOn = true;
            }

            else if (isOn == true && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading Out");
                isOn = false;
            }

            else if (isOn == false && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding In");
                isOn = true;
            }

            else if (isOn == true && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding Out");
                isOn = false;
            }

            else if (isOn == false && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish In");
                isOn = true;
            }

            else if (isOn == true && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish Out");
                isOn = false;
            }

            if (enableTrigger == true && isOn == false)
            {
                triggerObject.SetActive(false);

            }

            else if (enableTrigger == true && isOn == true)
            {
                triggerObject.SetActive(true);

            }
        }
    }
}