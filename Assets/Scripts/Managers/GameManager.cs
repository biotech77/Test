using RSG;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GuiManager GuiManager;
    public LevelManager LevelManager;
    public PlayerManager PlayerManager;
    public NpcManager NpcManager;

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Initialize()
            .Then(() => GuiManager.Initialize())
            .Then(() => LevelManager.Initialize())
            .Then(() => PlayerManager.Initialize())
            .Then(() => NpcManager.Initialize())
            .Done(() =>
            {
                //Reset();
            });
    }

    private IPromise Initialize()
    {
        return Promise.Resolved();
    }

    public void Run()
    {
        
    }

    public void Reset()
    {
        GuiManager.Reset()
            .Then(() => LevelManager.Reset())
            .Then(() => PlayerManager.Reset())
            .Then(() => NpcManager.Reset())
            .Done(() =>
            {

            });
    }
}
