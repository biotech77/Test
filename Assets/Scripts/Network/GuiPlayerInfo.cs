using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

public class GuiPlayerInfo : MonoBehaviour
{
    public GameObject Player1Container;
    public GameObject Player2Container;
    
    public TextMeshProUGUI Player1Name;
    public TextMeshProUGUI Player1Hits;
    public TextMeshProUGUI Player1Record;
    
    public TextMeshProUGUI Player2Name;
    public TextMeshProUGUI Player2Hits;
    public TextMeshProUGUI Player2Record;

    void Awake()
    {
        Player1Container.SetActive(false);
        Player2Container.SetActive(false);    
    }
    
    private StringBuilder _builder = new();
    
    public void UpdatePlayerInfo(ulong playerId, int hits,int totalShots, Queue<bool> record)
    {
        Debug.Log("UpdatePlayerInfo called! " + playerId);

        if (totalShots == 0) totalShots = 1;
        var successRate = (float)hits / totalShots;
        
        if (!Player1Container.gameObject.activeInHierarchy)
            Player1Container.SetActive(true);

        Player1Name.text = $"Player: {playerId}";
        Player1Hits.text = $"Hits: {hits.ToString()} Success Rate: {successRate:F1}%";
        Player1Record.text = $"Last 10: {FormatRecord(record)}";
    }

    string FormatRecord(Queue<bool> record)
    {
        _builder.Clear();

        foreach (var value in record)
        {
            _builder.Append(value ? "X" : "O");
        }

        return _builder.ToString();
    }
}
