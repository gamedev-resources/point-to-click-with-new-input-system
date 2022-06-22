using Assets.Scripts;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

using static UnityEngine.InputSystem.InputAction;

public class PlayerMovementController : MonoBehaviour
{
    private InputControls _inputMapping;

    private Camera _camera;
    private NavMeshAgent _agent;

    private Vector3 _moveTarget = Vector3.zero;
    private Quaternion _lookRotation = Quaternion.identity;
    private Vector3 _direction = Vector3.zero;
    private bool _readyToMove = false;

    public float RotateSpeed = 10f;
    public float WalkSpeed = 2.5f;
    public float RunSpeed = 4f;

    private MovementStates _currentMovement;
    public MovementStates CurrentMovement
    {
        get => _currentMovement;
        set
        {
            switch (value)
            {
                case MovementStates.Walk:
                    _agent.speed = 2.5f;
                    AnimationController.Instance.CurrentState = MovementStates.Walk;
                    break;
                case MovementStates.Run:
                    _agent.speed = 4f;
                    AnimationController.Instance.CurrentState = MovementStates.Run;
                    break;

                case MovementStates.None:
                    AnimationController.Instance.CurrentState = MovementStates.None;
                    break;
            }

            _currentMovement = value;
        }
    }

    /// <summary>
    /// Returns true if the agent is in the middle of pathfinding or the agent has a remaining distance greater than 1/2 a meter (0.5f)
    /// </summary>
    public bool IsNavigating => _agent.pathPending || _agent.remainingDistance > .25f;

    private void Awake() => _inputMapping = new InputControls();

    void Start()
    {
        //Register listener events for inputs
        _inputMapping.Default.Walk.performed += Walk;
        _inputMapping.Default.Run.performed += Run;

        _camera = Camera.main; 
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!_readyToMove && !IsNavigating && _currentMovement != MovementStates.None)
        {
            Debug.Log("Stop Navigating");
           StopNavigation();
        }
        else if (_readyToMove)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotateSpeed);

            if (Vector3.Dot(_direction, transform.forward) >= 0.97)
            {
                Debug.Log(Vector3.Dot(_direction, transform.forward));

                StartNavigation(_moveTarget);

                _readyToMove = false;
            }
        }
    }

    private void OnEnable() => _inputMapping.Enable();

    private void OnDisable() => _inputMapping.Disable();

    private void Run(CallbackContext context)
    {
        //AnimationController.Instance.CurrentState = MovementStates.Run;
    }

    private void Walk(CallbackContext context)
    {

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navPos, .25f, 1 << 0))
            {
                _moveTarget = navPos.position;
                _direction = (_moveTarget.WithNewY(0) - transform.position).normalized;

                StopNavigation();
                _lookRotation = Quaternion.LookRotation(_direction);
                _readyToMove = true;

                CurrentMovement = MovementStates.Walk;

                //if (IsNavigating && Vector3.Dot(_direction, transform.forward) >= 0.25f)
                //{
                //    StartNavigation(_moveTarget);
                //}
                //else
                //{

                //}
            }
        }
    }

    private void StartNavigation(Vector3 destination)
    {
        //Debug.DrawRay(destination, Vector3.up, Color.blue, 5f);

        _agent.SetDestination(destination);
    }

    /// <summary>
    /// Stops the player from moving.
    /// </summary>
    private void StopNavigation()
    {
        _agent.SetDestination(transform.position);
        CurrentMovement = MovementStates.None;

    }
}
