using UnityEngine;

public class FailZone : MonoBehaviour, IKnifeHit
{
    public void OnSharpEdgeHit(PlayerController playerController)
    {
        Lose(playerController);
    }

    public void OnKnifesBackHit(PlayerController playerController)
    {
        Lose(playerController);
    }

    private void Lose(PlayerController playerController)
    {
        playerController.Stuck();
        GameManager.Instance.SetGameState(GameState.Lose);
    }
}