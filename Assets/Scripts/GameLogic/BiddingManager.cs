using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class BiddingManager : NetworkBehaviour
{
    //TO DO: Make sure that if player bids that they have enough money
    //Updates prices as a person makes a bid call
    //Develop the timer function
    //Update ownership of port and take their money off
    
    public static BiddingManager Instance {get; set;}
    private float timer;
    public TMP_Text portName;
    public GameObject biddingUI;
    [SerializeField] private Button price1Btn;
    [SerializeField] private Button price2Btn;
    [SerializeField] private Button price3Btn;
    [SerializeField] private TMP_Text price1Text;
    [SerializeField] private TMP_Text price2Text;
    [SerializeField] private TMP_Text price3Text;
    [SerializeField] private TMP_Text currentBidText;
    [SerializeField] private TMP_Text currentBidderText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text availablityText;
    [SerializeField] private TMP_Text isBiddingText;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private Slider timerSlider;
    private NetworkVariable<int> currentBid = new NetworkVariable<int>(0);
    private NetworkVariable<int> price1 = new NetworkVariable<int>(0);
    private NetworkVariable<int> price2 = new NetworkVariable<int>(0);
    private NetworkVariable<int> price3 = new NetworkVariable<int>(0);
    private NetworkVariable<ulong> currentBidderId = new NetworkVariable<ulong>(1000000);
    private NetworkVariable<bool> isBidding = new NetworkVariable<bool>(false);
    private int tilePosition;
    private ulong senderId;
    private void Update()
    {
        if (isBidding.Value)
        {
            GameTimer();
        }
    }

    public override void OnNetworkSpawn()
    {
        //Subscriber events to detect change in a value of the networked variables which automatically updates the text in the UI.
        currentBid.OnValueChanged += OnCurrentBidChanged;
        price1.OnValueChanged += OnPrice1Changed;
        price2.OnValueChanged += OnPrice2Changed;
        price3.OnValueChanged += OnPrice3Changed;
        currentBidderId.OnValueChanged += OnCurrentBidderChanged;
    }
    
    private void Awake()
    {
        Instance = this;
    }

    //Detects whether the user has clicked the 1st pricing option button
    public void OnPrice1ButtonClicked()
    {
        UpdatePricingOptionsServerRpc(1, new RpcParams());
    }
    //Detects whether the user has clicked the 2nd pricing option button
    public void OnPrice2ButtonClicked()
    {
        UpdatePricingOptionsServerRpc(2, new RpcParams());
    }
    //Detects whether the user has clicked the 3rd pricing option button
    public void OnPrice3ButtonClicked()
    {
        UpdatePricingOptionsServerRpc(3, new RpcParams());
    }
    //Functions to change the text to the value that has been updated.
    private void OnPrice1Changed<T>(T oldValue, T newValue)
    {
        price1Text.text = price1.Value.ToString();
    }
    private void OnPrice2Changed<T>(T oldValue, T newValue)
    {
        price2Text.text = price2.Value.ToString();
    }
    private void OnPrice3Changed<T>(T oldValue, T newValue)
    {
        price3Text.text = price3.Value.ToString();
    }
    private void OnCurrentBidChanged<T>(T oldValue, T newValue)
    {
        currentBidText.text = currentBid.Value.ToString();
    }
    private void OnCurrentBidderChanged<T>(T oldValue, T newValue)
    {
        if (currentBidderId.Value == 1000000)
        {
            return;
        }
        currentBidderText.text = GameManager.Instance.players[currentBidderId.Value].GetUsername();
    }

    private void GameTimer()
    {
        if (IsServer)
        {
            if(timer <= 0)
            {
                isBidding.Value = false;
                FinaliseBidding();
                return;
            }
            timer -= Time.deltaTime;
            UpdateTimerClientRpc(timer);
        }
    }
    private void FinaliseBidding()
    {
        if(currentBidderId.Value == 1000000)
        {
            Dice.Instance.EnableEndTurnClientRpc(senderId);
            return;
        }
        else
        {
            GameManager.Instance.PurchasePort(currentBidderId.Value, currentBid.Value, tilePosition);
            Dice.Instance.EnableEndTurnClientRpc(senderId);
            currentBidderId.Value = 1000000;
        }
    }
    private void UpdateTimerValue(float value)
    {
       timerText.text = $"{Mathf.FloorToInt(value)}";
    }

    #region RPCs
    
    [Rpc(SendTo.Server)]
    public void BiddingServerRpc(RpcParams rpcparams)
    {
        senderId = rpcparams.Receive.SenderClientId;
        tilePosition = GameManager.Instance.players[senderId].currentTilePosition;
        Port port = GameManager.Instance.tiles[tilePosition].GetComponent<Port>();
        BiddingClientRpc(port.GetTileName());
        price1.Value = 4;
        price2.Value = 25;
        price3.Value = 102;
        CheckPriceClientRpc();
        currentBid.Value = 2;
        timer = 10f;
        isBidding.Value = true;
    }

    [Rpc(SendTo.Server)]
    private void UpdatePricingOptionsServerRpc(int priceOption, RpcParams rpcParams)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        switch (priceOption)
        {
            case 1:
               currentBidderId.Value = senderClientId;
               currentBid.Value += 2;
               break;
            case 2:
               currentBidderId.Value = senderClientId;
               currentBid.Value += 23;
               break;
            case 3:
               currentBidderId.Value = senderClientId;
               currentBid.Value += 100;
               break;
        }
        price1.Value = currentBid.Value + 2;
        price2.Value = currentBid.Value + 23;
        price3.Value = currentBid.Value + 100;
        CheckPriceClientRpc();
        timer = 10f;
    }

    [ClientRpc]
    private void BiddingClientRpc(string tileName)
    {
        availablityText.text = "";
        isBiddingText.text = "i am bidding...";
        biddingUI.SetActive(true);
        price1Btn.gameObject.SetActive(true);
        price2Btn.gameObject.SetActive(true);
        price3Btn.gameObject.SetActive(true);
        portName.text = tileName;
        timer = 10f;
        timerSlider.maxValue = timer;
        timerSlider.value = timer;
        currentBidderText.text = "None";
        inventoryUI.SetActive(false);
    }
    [ClientRpc]
    private void UpdateTimerClientRpc(float newValue)
    {
        timer = newValue;
        timerSlider.value = newValue;
        UpdateTimerValue(timer);
        if(timer <= 0)
        {
            biddingUI.SetActive(false);
            inventoryUI.SetActive(true);
            return;
        }
    }
    [ClientRpc]
    private void CheckPriceClientRpc()
    {
           ulong id = NetworkManager.Singleton.LocalClientId;
           
           if(price1.Value > GameManager.Instance.players[id].GetGold())
           {
               price1Btn.gameObject.SetActive(false);
           }
           if(price2.Value > GameManager.Instance.players[id].GetGold())
           {
               price2Btn.gameObject.SetActive(false);
           }  
           if(price3.Value > GameManager.Instance.players[id].GetGold())
           {
               price3Btn.gameObject.SetActive(false);
           }

           if(!price1Btn.IsActive() && !price2Btn.IsActive() && !price3Btn.IsActive())
            {
                isBiddingText.text = "";
                availablityText.text = "No pricing options available, you have ran out of gold.";
            }
        }
    }
    #endregion

