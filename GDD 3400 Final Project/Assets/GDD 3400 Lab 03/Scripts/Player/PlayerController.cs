using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Cinemachine3rdPersonAim _ThirdPersonAim;
    [SerializeField] ShootMechanic _ShootMechanic;
    [SerializeField] int _Health;

    [SerializeField] Slider _HealthSlider;
    [SerializeField] TextMeshProUGUI _HealthText;

    public void Awake()
    {
        
    }

    public void Update()
    {
        if (_ThirdPersonAim == null || _ShootMechanic == null) return;
        
        // Pass in the aim target point to the shoot mechanic
        _ShootMechanic.AimTargetPoint = _ThirdPersonAim.AimTarget;

        // Start and stop the shoot action based on the shoot action input
        if (Mouse.current.leftButton.wasPressedThisFrame) PerformShoot();
    }

    private void PerformShoot()
    {
        // Perform the shoot action
        _ShootMechanic.PerformShoot(_ShootMechanic.AimTargetPoint);

        // Look at the aim target, this helps make the character look more natural when shooting
        this.transform.LookAt(_ThirdPersonAim.AimTarget);
        this.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
            TakeDamage(collision.gameObject.GetComponent<SimpleAIController>()._Damage);
        Debug.Log("colliding");
    }

    public void TakeDamage(int damage)
    {
        _Health -= damage;
        Debug.Log("Player took damage: " + damage);
        if(_Health <= 0)
        {
            SceneManager.LoadScene("GameOver");
        }
        _HealthSlider.value = _Health;
        _HealthText.text = $"Health: {_Health} / 100";
    }
}
