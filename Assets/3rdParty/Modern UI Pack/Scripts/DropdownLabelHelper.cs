using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class DropdownLabelHelper : MonoBehaviour
    {
        [Header("RESOURCES")]
        public TextMeshProUGUI mainText;
        public TextMeshProUGUI highlightedText;

        void Update()
        {
            highlightedText.text = mainText.text;
        }
    }
}
