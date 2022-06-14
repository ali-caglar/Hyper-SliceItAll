using UnityEngine;

public class SharpEdge : MonoBehaviour
{
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IKnifeHit hitInfo))
        {
            hitInfo.OnSharpEdgeHit(_playerController);
        }
    }
}