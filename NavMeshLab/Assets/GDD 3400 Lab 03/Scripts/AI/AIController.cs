using UnityEngine;

public class AIController : MonoBehaviour
{
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Search,
    }

    [Header("Health")]
    [SerializeField] int _Health = 100;

    [Header("Navigation")]
    [SerializeField] float _ReNavigateInterval = 1f;

    [Header("Patrol Settings")]
    [SerializeField] private float _PatrolSpeed = 2f;
    [SerializeField] private Transform[] _PatrolPoints;     // Possible patrol/search locations
    [SerializeField] private float _IdleAtPatrolPointTime = 2f;

    [Header("Chase Settings")]
    [SerializeField] private float _ChaseSpeed = 5f;
    [SerializeField] private float _PreferredDistance = 8f;
    [SerializeField] private float _DistanceTolerance = 2f;

    [Header("Search Settings")]
    [SerializeField] private float _TimeToRememberPlayer = 3f;

    [Header("Attack Settings")]
    [SerializeField] private bool _AttackOnSight = true;

    // Internal references
    PlayerController _player;
    AINavigation _navigation;
    AIPerception _perception;
    ShootMechanic _shootMechanic;

    // Internal state
    AIState _currentState = AIState.Idle;

    private Transform currentPatrolPoint;
    private float idleTimer = 0f;

    private float lostSightTimer = 0f;
    private Vector3 lastKnownPlayerPosition;


    #region Unity Lifecycle
    
    private void Awake()
    {
        // Find the player, navigation, shoot mechanic, and perception components
        _player = FindFirstObjectByType<PlayerController>();
        _navigation = this.GetComponent<AINavigation>();
        _shootMechanic = this.GetComponent<ShootMechanic>();
        _perception = this.GetComponent<AIPerception>();

        // Start the agent in the idle state
        SwitchState(AIState.Idle);
    }


    private void Update()
    {
        // Set the name of the agent to the current state for debugging purposes
        this.name = "AI Agent: " + _currentState;

        // Update the state of the agent
        switch (_currentState)
        {
            case AIState.Idle:
                UpdateIdle();
                break;

            case AIState.Patrol:
                UpdatePatrol();
                break;

            case AIState.Chase:
                UpdateChase();
                break;

            case AIState.Search:
                UpdateSearch();
                break;
        }
    }

    #endregion

    #region State Updates

    private void SwitchState(AIState newState)
    {
        // First call the exit state method to clean up the current state
        OnExitState(_currentState);

        // Then set the new state
        _currentState = newState;

        // Finally call the enter state method to initialize the new state
        OnEnterState(newState);
    }

    // Called once when the state is entered
    private void OnEnterState(AIState state)
    {
        switch (state)
        {
            case AIState.Idle:
                idleTimer = 0f;
                break;

            case AIState.Patrol:
                _navigation.SetSpeed(_PatrolSpeed);
                PickNewPatrolPoint();
                break;

            case AIState.Chase:
                _navigation.SetSpeed(_ChaseSpeed);
                break;

            case AIState.Search:
                lostSightTimer = 0f;
                _navigation.SetSpeed(_PatrolSpeed);
                _navigation.SetDestination(lastKnownPlayerPosition, true);

                break;
        }
    }

    // Called once when the state is exited
    private void OnExitState(AIState state)
    {
        switch (state)
        {
            case AIState.Idle:
                break;

            case AIState.Patrol:
                break;

            case AIState.Chase:
                break;

            case AIState.Search:
                break;
        }
    }

    #endregion

    #region State Updates
    private void UpdateIdle()
    {
        // If we can see the player, start chasing.
        if (_perception.CanSeePlayer)
        {
            SwitchState(AIState.Chase);
            return;
        }

        // If player is not even in detection range, start searching.
        if (!_perception.WithinVisionRange())
        {
            SwitchState(AIState.Patrol);
            return;
        }
    }

    private void UpdatePatrol()
    {
        // If we see the player at any time, start chasing.
        if (_perception.CanSeePlayer)
        {
            SwitchState(AIState.Chase);
            return;
        }

        // If no search points, just go back to idle.
        if (_PatrolPoints == null || _PatrolPoints.Length == 0)
        {
            SwitchState(AIState.Idle);
            return;
        }

        // Make sure we have somewhere to go.
        if (currentPatrolPoint == null)
        {
            PickNewPatrolPoint();
        }

        // Check if we've reached the current search point.
        if (_navigation.IsAtDestination())
        {
            // Stand still for a bit.
            idleTimer += Time.deltaTime;

            if (idleTimer >= _IdleAtPatrolPointTime)
            {
                PickNewPatrolPoint();
            }
        }
    }

    private void UpdateChase()
    {
        bool canSeePlayer = _perception.CanSeePlayer;
        float distanceToPlayer = Vector3.Distance(this.transform.position, _player.transform.position);

        if (canSeePlayer)
        {
            // Update last known position whenever we see the player.
            lastKnownPlayerPosition = _player.transform.position;
            lostSightTimer = 0f;

            // Too far: move closer.
            if (distanceToPlayer > _PreferredDistance + _DistanceTolerance)
            {
                _navigation.SetDestination(_player.transform.position);
            }
            // Too close: back away to our preferred distance.
            else if (distanceToPlayer < _PreferredDistance - _DistanceTolerance)
            {
                Vector3 directionAway = (this.transform.position - _player.transform.position).normalized;
                Vector3 targetPosition = _player.transform.position + directionAway * _PreferredDistance;
                _navigation.SetDestination(targetPosition);
            }

            // Perform the shoot action
            _shootMechanic.PerformShoot(_perception.GetPlayerCenterPosition());
        }
        else
        {
            // If we can't see the player, start searching.
            SwitchState(AIState.Search);
        }
    }

    private void UpdateSearch()
    {
        // We lost sight of the player. Start counting.
        lostSightTimer += Time.deltaTime;

        if (lostSightTimer >= _TimeToRememberPlayer)
        {
            // Move to last known location.
            _navigation.SetDestination(lastKnownPlayerPosition);

            // Once we arrive and still can't see them, go back to search.
            if (_navigation.IsAtDestination())
            {
                if (!_perception.CanSeePlayer)
                {
                    SwitchState(AIState.Patrol);
                }
                else
                {
                    // If they reappeared right as we arrived, go back to chase.
                    SwitchState(AIState.Chase);
                }
            }
        }

        // If we can see the player again, start chasing.
        if (_perception.CanSeePlayer)
        {
            SwitchState(AIState.Chase);
        }
    }

    #endregion

    private void PickNewPatrolPoint()
    {
        if (_PatrolPoints == null || _PatrolPoints.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, _PatrolPoints.Length);
        currentPatrolPoint = _PatrolPoints[index];

        _navigation.SetDestination(currentPatrolPoint.position);
        idleTimer = 0f;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("AI took damage: " + damage);
        _Health -= damage;
        if (_Health <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}