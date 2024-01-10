using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChoice : MonoBehaviour
{
    [SerializeField] PlayerData[] choices;
    [SerializeField] int idx = 0;
    [SerializeField] TMP_Text playerName;
    [SerializeField] Button left, right;

    void Awake()
    {
        UpdatePlayerName();
        InitLeftRightButtons();
    }

    void InitLeftRightButtons()
    {
        left .onClick.AddListener(() => SetIdx(idx - 1));
        right.onClick.AddListener(() => SetIdx(idx + 1));
    }

    void SetIdx(int idx)
    {
        while (idx < 0)
            idx += choices.Length;

        idx %= choices.Length;

        this.idx = idx;

        UpdatePlayerName();
    }

    void UpdatePlayerName()
    {
        playerName.text = Player.name;
    }

    public PlayerData Player
    {
        get => choices[idx];
    }
}

