using UnityEngine;




public class StartGameButton : MonoBehaviour
{
    [SerializeField] PlayerChoice p1Choice, p2Choice;

    public void StartGame()
    {
        GameManager.Inst.StartGame(p1Choice.Player, p2Choice.Player);
    }
}
