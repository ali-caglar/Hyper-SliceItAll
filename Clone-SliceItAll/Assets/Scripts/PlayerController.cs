using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Vector3 _jumpForce;
    [SerializeField] private Vector3 _spinTorque;
    
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        InputController.OnTap += OnTapHandler;
    }

    private void OnDisable()
    {
        InputController.OnTap -= OnTapHandler;
    }

    public void Stuck()
    {
        _rigidbody.isKinematic = true;
    }

    private void OnTapHandler()
    {
        _rigidbody.isKinematic = false;
        Jump();
        Spin();
    }

    private void Jump()
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(_jumpForce, ForceMode.Impulse);
    }

    private void Spin()
    {
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.AddTorque(_spinTorque, ForceMode.Acceleration);
    }
}