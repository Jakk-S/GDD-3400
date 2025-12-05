using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AINavigation : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] float _ReNavigateInterval = 1f;

    private NavMeshAgent _agent;
    private float timeSinceLastNavigate = 0f;
    private Vector3 _nextMovePosition;

    void Awake()
    {
        _agent = this.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        timeSinceLastNavigate += Time.deltaTime;
        if (timeSinceLastNavigate >= _ReNavigateInterval)
        {
            timeSinceLastNavigate = 0f;
            MoveTo(_nextMovePosition);
        }
    }

    public void SetDestination(Vector3 destination, bool force = false)
    {
        if (_nextMovePosition == destination && !force)
        {
            return;
        }
        
        _nextMovePosition = destination;
        if (force) timeSinceLastNavigate = 0f;
    }

    public void MoveTo(Vector3 destination)
    {
        if (_agent.destination == destination)
        {
            return;
        }

        _agent.SetDestination(destination);
    }

    public void SetSpeed(float speed)
    {
        _agent.speed = speed;
    }

    public void Stop()
    {
        _agent.isStopped = true;
    }

    public bool IsAtDestination()
    {
        return _agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending;
    }
}
