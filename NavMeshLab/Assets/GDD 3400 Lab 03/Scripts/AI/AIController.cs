using UnityEngine;

public class AIController : MonoBehaviour
{

    [SerializeField] int _Health = 100;
    [SerializeField] bool _TrackPlayer = false;
    [SerializeField] float _ReNavigateInterval = .5f;

    PlayerController _player;
    AINavigation _navigation;

    float _timeSinceLastNavigate = 0f;

    void Awake()
    {
        _player = FindFirstObjectByType<PlayerController>();
        _navigation = this.GetComponent<AINavigation>();
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
