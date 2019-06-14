using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectomeFocus : MonoBehaviour
{
    // Start is called before the first frame update

    public TextMesh title;
    public TextMesh anatomy;
    public TextMesh isomap;
    public TextMesh tsne;
    public TextMesh mds;

    public List<string> selectedConnectome = new List<string>();
    private Dictionary<string, List<string>> _connectomeRepresentationDictionary;

   public void GetRepresentationDictionary(Dictionary<string, List<string>> connectomeRepresentationDictionary)
    {
        _connectomeRepresentationDictionary = connectomeRepresentationDictionary;
    }

    private void OnTriggerEnter(Collider collider)
    {
        GameObject selectedConnectome;

        if (collider.transform.tag == "SelectionConnectome")
            selectedConnectome = collider.transform.parent.gameObject;
        else
            selectedConnectome = collider.gameObject;

        selectedConnectome.transform.parent.transform.position = new Vector3(this.transform.position.x, 0f, this.transform.position.z);
    }

    private void OnTriggerStay(Collider collider)
    {
        GameObject selectedConnectome;
    
        if (collider.transform.tag == "SelectionConnectome")
            selectedConnectome = collider.transform.parent.gameObject;
        else
            selectedConnectome = collider.gameObject;

        selectedConnectome.transform.parent.transform.localScale = new Vector3 (2, 2, 2);

        title.text = selectedConnectome.transform.name;

        if(_connectomeRepresentationDictionary[title.text] != null && title.text!=null)
            foreach (string _representationName in _connectomeRepresentationDictionary[title.text])
            {
                if(_representationName == "anatomy")
                    anatomy.gameObject.SetActive(true);
                if (_representationName == "isomap")
                    isomap.gameObject.SetActive(true);
                if (_representationName == "tsne")
                    tsne.gameObject.SetActive(true);
                if (_representationName == "mds")
                    mds.gameObject.SetActive(true);
            }

    }
    private void OnTriggerExit(Collider collider)
    {
        GameObject selectedConnectome;
        if (collider.transform.tag == "SelectionConnectome")
            selectedConnectome = collider.transform.parent.gameObject;
        else
            selectedConnectome = collider.gameObject;

        anatomy.gameObject.SetActive(false);
        isomap.gameObject.SetActive(false);
        tsne.gameObject.SetActive(false);
        mds.gameObject.SetActive(false);

        selectedConnectome.transform.parent.transform.localScale = new Vector3(1, 1, 1);
    }
}
