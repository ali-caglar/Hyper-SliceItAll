using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Jump Forth")]
    [SerializeField] private Vector3 _jumpForthForce;
    [SerializeField] private Vector3 _spinForthTorque;
    [Header("Jump Back")]
    [SerializeField] private Vector3 _jumpBackForce;
    [SerializeField] private Vector3 _spinBackTorque;
    
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

    private void Start()
    {
        GameManager.Instance.SetGameState(GameState.Start);
    }

    public void Stuck()
    {
        _rigidbody.isKinematic = true;
    }

    public void JumpBack()
    {
        Jump(-1);
        Spin(-1);
    }

    private void OnTapHandler()
    {
        GameManager.Instance.SetGameState(GameState.InGame);
        
        _rigidbody.isKinematic = false;
        Jump();
        Spin();
    }

    private void Jump(int direction = 1)
    {
        Vector3 jumpForce = direction == 1 ? _jumpForthForce : _jumpBackForce;
        
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(jumpForce, ForceMode.Impulse);
    }

    private void Spin(int direction = 1)
    {
        Vector3 spinTorque = direction == 1 ? _spinForthTorque : _spinBackTorque;
        
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.AddTorque(spinTorque, ForceMode.Acceleration);
    }
}