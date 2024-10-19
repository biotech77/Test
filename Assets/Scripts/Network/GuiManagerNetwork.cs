using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GuiManagerNetwork : MonoBehaviour
{
    public Button ButtonHost;
    public Button ButtonClient;
    public Button ButtonServer;
    public GuiPlayerInfo PlayerInfo;

    public GameObject ServerUI;
    public GameObject ServerSettingsUI;
    public GameObject ClientUI;
    public GameObject GamePlayUI;
    
    public static GuiManagerNetwork Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ButtonHost.onClick.AddListener(OnHostButtonClick);
        ButtonClient.onClick.AddListener(OnClientButtonClick);
        ButtonServer.onClick.AddListener(OnServerButtonClick);
    }

    private void OnServerButtonClick()
    {
        NetworkManager.Singleton.StartServer();
    }

    private void OnClientButtonClick()
    {
        NetworkManager.Singleton.StartClient();
        ShowGamePlayUI();
    }

    private void OnHostButtonClick()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void OnDestroy()
    {
        ButtonHost.onClick.RemoveListener(OnHostButtonClick);
        ButtonHost.onClick.RemoveListener(OnClientButtonClick);
        ButtonHost.onClick.RemoveListener(OnServerButtonClick);
    }

    public void ShowServerUI()
    {
        ServerSettingsUI.gameObject.SetActive(false);
        ServerUI.gameObject.SetActive(true);
        ClientUI.gameObject.SetActive(false);
        GamePlayUI.gameObject.SetActive(false);
    }
    
    public void ShowClientUI()
    {
        ServerSettingsUI.gameObject.SetActive(false);
        ServerUI.gameObject.SetActive(false);
        ClientUI.gameObject.SetActive(true);
        GamePlayUI.gameObject.SetActive(false);
    }
    
    public void ShowGamePlayUI()
    {
        ServerSettingsUI.gameObject.SetActive(false);
        ServerUI.gameObject.SetActive(false);
        ClientUI.gameObject.SetActive(false);
        GamePlayUI.gameObject.SetActive(true);
    }
    
    public void ShowServerSettingsUI()
    {
        ServerSettingsUI.gameObject.SetActive(true);
        ServerUI.gameObject.SetActive(false);
        ClientUI.gameObject.SetActive(false);
        GamePlayUI.gameObject.SetActive(false);
    }
}
