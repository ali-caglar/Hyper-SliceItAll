using UnityEngine;

public class Sliceable : MonoBehaviour, IKnifeHit
{
    private bool _isSliced;
    private Rigidbody[] _rigidbodies;

    private void Awake()
    {
        _rigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    public void OnSharpEdgeHit(PlayerController playerController)
    {
        if (_isSliced) return;
        _isSliced = true;
        
        Slice();
    }

    public void OnKnifesBackHit(PlayerController playerController)
    {
        if (_isSliced) return;
        
        playerController.JumpBack();
    }

    private void Slice()
    {
        foreach (var part in _rigidbodies)
        {
            part.isKinematic = false;
            part.AddForce(part.transform.localPosition.x * 250f * Vector3.right);
        }
    }
}