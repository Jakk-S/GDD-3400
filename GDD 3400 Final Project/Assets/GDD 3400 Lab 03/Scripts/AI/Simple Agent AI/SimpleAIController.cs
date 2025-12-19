using UnityEngine;
using UnityEngine.AI;

public class SimpleAIController : MonoBehaviour
{

    [SerializeField] int _Health = 100;
    [SerializeField] bool _TrackPlayer = false;
    [SerializeField] float _ReNavigateInterval = .5f;

    [SerializeField] public int _Damage = 10;

    PlayerController _player;
    SimpleAINavigation _navigation;

    float _timeSinceLastNavigate = 0f;

    void Awake()
    {
        _player = FindFirstObjectByType<PlayerController>();
        _navigation = this.GetComponent<SimpleAINavigation>();
    }

    void Update()
    {
        if (_TrackPlayer && _player != null)
        {
            _timeSinceLastNavigate += Time.deltaTime;
            if (_timeSinceLastNavigate >= _ReNavigateInterval)
            {
                _navigation.SetDestination(_player.transform.position);
                _timeSinceLastNavigate = 0f;
            }
        }
        //if (Vector3.Distance(transform.position, _player.transform.position) <= 5f)
        //    _player.GetComponent<PlayerController>().TakeDamage(_Damage);
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
