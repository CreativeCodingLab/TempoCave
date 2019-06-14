using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Michsky.UI.ModernUIPack
{
    public class SwitchAnim : MonoBehaviour
    {
        [Header("SWITCH")]
        public Animator switchAnimator;

        [Header("SETTINGS")]
        [Tooltip("IMPORTANT! EVERY SWITCH MUST HAVE A DIFFERENT ID")]
        public int switchID = 0;
        public bool isOn;
        public bool saveValue;
        [Tooltip("Use it if you're using this switch first time. 1 = ON, and 0 = OFF")]
        [Range(0, 1)] public int playerPrefsHelper;

        public UnityEvent OffEvents;
        public UnityEvent OnEvents;
        public string connectomeNumber;
        private Button offButton;
        private Button onButton;
        private string connectomeName;
        private string onTransition = "Switch On";
        private string offTransition = "Switch Off";
        private bool isFirstUpdate = true;
        private GameObject connectomeParent1;
        private GameObject connectomeParent2;
        private DesktopController DC;
        private string SwitchName;
        private DataLoader dataLoader;
        private bool isEdgeBundling;
        private bool isDynamic1;
        private bool isDynamic2;
        private int isDuplicate;
        void Start()
        {
            connectomeName = PlayerPrefs.GetString("SelectedConnectome_" + connectomeNumber);
            isDuplicate = PlayerPrefs.GetInt("isDuplicate");
            
            playerPrefsHelper = PlayerPrefs.GetInt(switchID + "Switch");
            SwitchName = this.transform.name;
            if (saveValue == true)
            {
                if (playerPrefsHelper == 1)
                {
                    OnEvents.Invoke();
                    switchAnimator.Play(onTransition);
                    isOn = true;
                }

                else
                {
                    OffEvents.Invoke();
                    switchAnimator.Play(offTransition);
                    isOn = false;
                }
            }

            else
            {
                if (isOn == true)
                {
                    switchAnimator.Play(onTransition);
                    OnEvents.Invoke();
                    isOn = true;
                }

                else
                {
                    switchAnimator.Play(offTransition);
                    OffEvents.Invoke();
                    isOn = false;
                }
            }
        }

        void Update()
        {
            if (isFirstUpdate)
            {
                dataLoader = GameObject.Find("DataLoader").GetComponent<DataLoader>();
                //isEdgeBundling = dataLoader.isEdgeBundlingDictionaryGlobal[connectomeName];
                
                isFirstUpdate = false;

                GameObject[] singleConnectomes = GameObject.FindGameObjectsWithTag("SingleConnectome");
                if (isDuplicate == 0 && int.Parse(connectomeNumber) == 0)
                {
                    connectomeParent1 = singleConnectomes[0];
                    connectomeParent2 = singleConnectomes[1];
                }   
                else if(isDuplicate == 0 && int.Parse(connectomeNumber) == 1)
                {
                    connectomeParent1 = singleConnectomes[1];
                    connectomeParent2 = singleConnectomes[0];
                }
                    
                else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 0)
                {
                    connectomeParent1 = GameObject.FindGameObjectWithTag("SingleConnectome");
                    connectomeParent2 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
                }
                    
                else if (isDuplicate == 1 && int.Parse(connectomeNumber) == 1)
                {
                    connectomeParent1 = GameObject.FindGameObjectWithTag("SingleConnectomeDuplicate");
                    connectomeParent2 = GameObject.FindGameObjectWithTag("SingleConnectome");
                }
                DC = GameObject.Find("Canvas").GetComponent<DesktopController>();

                isDynamic1 = dataLoader.isDynamicDictionaryGlobal[connectomeParent1.name];
                isDynamic2 = dataLoader.isDynamicDictionaryGlobal[connectomeParent2.name];
                
            }
        }
        public void AnimateSwitch()
        {
            if (isOn == true)
            {
                if (SwitchName == "Sync" && isDynamic1 && isDynamic2)
                {
                    connectomeParent1.GetComponent<SingleConnectome>().isSync = false;
                    DC.ChangeSyncOption(connectomeParent1, connectomeParent2, false);
                    OffEvents.Invoke();
                    switchAnimator.Play(offTransition);
                    isOn = false;
                    playerPrefsHelper = 0;
                }
                if (SwitchName == "EdgeBundling")
                {
                    connectomeParent1.GetComponent<SingleConnectome>().isEdgeBundling = false;
                    OffEvents.Invoke();
                    switchAnimator.Play(offTransition);
                    isOn = false;
                    playerPrefsHelper = 0;
                }
                if (SwitchName == "PosNeg")
                {
                    connectomeParent1.GetComponent<SingleConnectome>().PosNeg = false;
                    OffEvents.Invoke();
                    switchAnimator.Play(offTransition);
                    isOn = false;
                    playerPrefsHelper = 0;
                }
                if (SwitchName == "SelectingNode")
                {
                    connectomeParent1.GetComponent<SingleConnectome>().isSelectingNode = false;
                    connectomeParent1.GetComponent<SingleConnectome>().HighLightSelectedNode();
                    OffEvents.Invoke();
                    switchAnimator.Play(offTransition);
                    isOn = false;
                    playerPrefsHelper = 0;
                }
            }

            else
            {
                if (SwitchName == "Sync" && isDynamic1 && isDynamic2)
                {
                    connectomeParent1.GetComponent<SingleConnectome>().isSync = true;
                    connectomeParent2.GetComponent<SingleConnectome>().isSync = false;
                    connectomeParent1.GetComponent<SingleConnectome>()._classificationType = "Dynamic Modularity";
                    connectomeParent2.GetComponent<SingleConnectome>()._classificationType = "Dynamic Modularity";
                    DC.ChangeSyncOption(connectomeParent1, connectomeParent2, true);
                    OnEvents.Invoke();
                    switchAnimator.Play(onTransition);
                    isOn = true;
                    playerPrefsHelper = 1;
                }
                if (SwitchName == "EdgeBundling")
                {
                    connectomeParent1.GetComponent<SingleConnectome>().isEdgeBundling = true;
                    OnEvents.Invoke();
                    switchAnimator.Play(onTransition);
                    isOn = true;
                    playerPrefsHelper = 1;
                }
                if (SwitchName == "PosNeg")
                {
                    connectomeParent1.GetComponent<SingleConnectome>().PosNeg = true;
                    OnEvents.Invoke();
                    switchAnimator.Play(onTransition);
                    isOn = true;
                    playerPrefsHelper = 1;
                }
                if (SwitchName == "SelectingNode")
                {
                    connectomeParent1.GetComponent<SingleConnectome>().isSelectingNode = true;
                    connectomeParent1.GetComponent<SingleConnectome>().showAllNode();
                    OnEvents.Invoke();
                    switchAnimator.Play(onTransition);
                    isOn = true;
                    playerPrefsHelper = 1;
                }
            }
            if (saveValue == true)
            {
                PlayerPrefs.SetInt(switchID + "Switch", playerPrefsHelper);
            }
        }
           
    }
    
}