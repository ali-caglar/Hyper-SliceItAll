using System.Collections;
using UnityEngine;

public class Ground : MonoBehaviour, IKnifeHit
{
    private bool _isKnifeStuck;

    private void OnEnable()
    {
        InputController.OnTap += UnStuck;
    }

    private void OnDisable()
    {
        InputController.OnTap -= UnStuck;
    }

    public void OnSharpEdgeHit(PlayerController playerController)
    {
        if (_isKnifeStuck) return;
        _isKnifeStuck = true;

        playerController.Stuck();
    }

    public void OnKnifesBackHit(PlayerController playerController)
    {
        if (_isKnifeStuck) return;
        _isKnifeStuck = true;
        
        playerController.JumpBack();
        StartCoroutine(UnStuckCoroutine());
    }

    private void UnStuck()
    {
        if (!_isKnifeStuck) return;
        StartCoroutine(UnStuckCoroutine());
    }

    private IEnumerator UnStuckCoroutine()
    {
        yield return new WaitForSeconds(1f);
        _isKnifeStuck = false;
    }
}