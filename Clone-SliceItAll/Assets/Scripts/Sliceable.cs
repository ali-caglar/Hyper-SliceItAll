using System;
using UnityEngine;
using TMPro;

public class Sliceable : MonoBehaviour, IKnifeHit
{
    public static event Action<int> OnObjectSliced;
    
    [SerializeField] private int _score;
    [SerializeField] private ParticleSystem _particle;
    
    private bool _isSliced;
    private Rigidbody[] _rigidbodies;
    private TextMeshProUGUI _scoreTMP;

    private void Awake()
    {
        _rigidbodies = GetComponentsInChildren<Rigidbody>();
        _scoreTMP = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        _scoreTMP.text = $"+{_score}";
        _scoreTMP.gameObject.SetActive(false);
    }

    public void OnSharpEdgeHit(PlayerController playerController)
    {
        if (_isSliced) return;
        _isSliced = true;
        
        Slice();
        _scoreTMP.gameObject.SetActive(true);
        OnObjectSliced?.Invoke(_score);
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
            part.AddForce(part.transform.localPosition.y * 5000f * Vector3.up);
        }
        
        _particle.Play();
    }
}