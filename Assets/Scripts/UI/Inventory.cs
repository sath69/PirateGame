using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    [SerializeField] private TMP_Text portName;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button downgradeButton;
    [SerializeField] private Image portColor;
    [SerializeField] private TMP_Text portLevelText;
    private int portId;
    private void Start()
    {
        portLevelText.text = "Lvl 0";
    }
    private void Awake()
    {
        Instance = this;
    }
    public void SetPortName(string name)
    {
        portName.text = name;
    }

    public void HideButtons()
    {
        upgradeButton.gameObject.SetActive(false);
        downgradeButton.gameObject.SetActive(false);
    }

    public void SetPortId(int id)
    {
        portId = id;
    }
    
    public void UpdateUpgradeButtons(int level)
    {
        switch (level)
        {
            case 0:
               upgradeButton.gameObject.SetActive(true);
               downgradeButton.gameObject.SetActive(false);
               portLevelText.text = "Lvl 0";
               break;
            case 1:
               upgradeButton.gameObject.SetActive(true);
               downgradeButton.gameObject.SetActive(true);
               portLevelText.text = "Lvl 1";
               break;
            case 2:
               upgradeButton.gameObject.SetActive(true);
               downgradeButton.gameObject.SetActive(true);
               portLevelText.text = "Lvl 2";
               break;
            case 3:
               upgradeButton.gameObject.SetActive(false);
               downgradeButton.gameObject.SetActive(true);
               portLevelText.text = "Lvl 3";
               break;
        }
    }
    public void OnUpgradeButtonClicked()
    {
        GameManager.Instance.Upgrade(portId);
    }

    public void OnDowngradeButtonClicked()
    {
        GameManager.Instance.Downgrade(portId);
    }


    public void SetPortColour(PortType type)
    {
        switch (type)
        {
            case PortType.IndianOcean:
                portColor.color = Color.red;
                break;
            case PortType.SouthEastAsia:
                portColor.color = new Color(0f,0.5372549f, 1f);
                break;
            case PortType.EastAsia:
                portColor.color = new Color(0.9921569f,0.5529412f,0.7411765f);
                break;
            case PortType.Africa:
                portColor.color = new Color(0.02352941f,0.6392157f,0.2588235f);
                break;
            case PortType.SouthernEurope:
                portColor.color = new Color(0.6980392f, 0.3098039f, 0);
                break;
            case PortType.SouthernAmerica:
                portColor.color = new Color(0, 0.05098039f, 0.4901961f);
                break;
            case PortType.NorthernEurope:
                portColor.color =  new Color(0.509804f, 0.3960784f, 0.5529412f);
                break;
            case PortType.NorthAmerica:
                portColor.color = new Color(0.145098f, 0.1764706f, 0.3137255f);
                break;
        }
    }
}
