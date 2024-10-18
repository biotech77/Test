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
}
