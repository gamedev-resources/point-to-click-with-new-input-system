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
    private bool _needToRotate = false;

    private float _rotateSpeed = 10f;
    public float _walkSpeed = 2.5f;
    public float _runSpeed = 4f;

    public ParticleSystem WalkDecal;

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
                    break;
                case MovementStates.Run:
                    _agent.speed = 4f;
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
        // Confirm that the player is done navigating
        if (!_needToRotate && !IsNavigating && _currentMovement != MovementStates.None)
        {
            StopNavigation();
        }
        //Navigation is likely starting
        else if (_needToRotate)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * _rotateSpeed);

            if (Vector3.Dot(_direction, transform.forward) >= .99f)
            {
                StartNavigation(_moveTarget);

                _needToRotate = false;
            }
        }
    }

    private void OnEnable() => _inputMapping.Enable();

    private void OnDisable() => _inputMapping.Disable();

    /// <summary>
    /// Called when the player does a double left mouse button click
    /// </summary>
    private void Run(CallbackContext context)
    {
        CurrentMovement = MovementStates.Run;
    }

    /// <summary>
    /// Called when the player does a single left mouse button click
    /// </summary>
    private void Walk(CallbackContext context)
    {

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navPos, .25f, 1 << 0))
            {
                //Stop navigating
                StopNavigation();

                _moveTarget = navPos.position;

                //Show the walk decal
                WalkDecal.transform.position = _moveTarget.WithNewY(0.1f);
                WalkDecal.Play();

                //Calculate rotation direction
                _direction = (_moveTarget.WithNewY(transform.position.y) - transform.position).normalized;
                _lookRotation = Quaternion.LookRotation(_direction, Vector3.up);
                _needToRotate = true;

                //Set the speed and ready the animation
                CurrentMovement = MovementStates.Walk;

                if (IsNavigating && Vector3.Dot(_direction, transform.forward) >= 0.25f)
                {
                    StartNavigation(_moveTarget);
                }
            }
        }
    }

    /// <summary>
    /// Start moving the player
    /// </summary>
    /// <param name="destination">Closest allowed position to where the player clicked</param>
    private void StartNavigation(Vector3 destination)
    {
        AnimationController.Instance.CurrentState = CurrentMovement;

        _agent.SetDestination(destination);

    }

    /// <summary>
    /// Stops the player from moving.
    /// </summary>
    private void StopNavigation()
    {
        _agent.SetDestination(transform.position);
        CurrentMovement = MovementStates.None;
        AnimationController.Instance.CurrentState = CurrentMovement;
    }
}
