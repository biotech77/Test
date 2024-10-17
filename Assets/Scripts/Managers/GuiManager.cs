using RSG;
using TMPro;
using UnityEngine;

public class GuiManager : MonoBehaviour
{
    public TextMeshProUGUI TextTotalHits;
    public TextMeshProUGUI TextTotalShots;
    public TextMeshProUGUI TextSuccessRate;

    private PlayerManager _player;
    public IPromise Initialize()
    {
        _player = GameManager.Instance.PlayerManager;
        _player.HitRegistered += OnHitRegistered;
        _player.ShotFired += OnHitRegistered;
        return Promise.Resolved();
    }

    private void OnHitRegistered()
    {
        TextTotalHits.text = $"Total Hits: {_player.TotalSuccessHits.ToString()}";
        TextTotalShots.text = $"Total Shots Fired: {_player.TotalShotsFired.ToString()}";

        if (_player.TotalSuccessHits != 0)
        {
            var successRate = (float)_player.TotalSuccessHits / _player.TotalShotsFired;
            TextSuccessRate.text = $"Success Rate: {successRate.ToString("P2")}";
        }
    }

    public IPromise Reset()
    {
        _player.HitRegistered -= OnHitRegistered;
        _player.ShotFired -= OnHitRegistered;
        return Promise.Resolved();
    }
}
