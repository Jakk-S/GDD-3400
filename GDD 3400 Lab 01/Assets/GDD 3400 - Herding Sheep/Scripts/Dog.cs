using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace GDD3400.Project01
{
    public class Dog : MonoBehaviour
    {
        // States to control the dog's behavior
        enum DogStates
        {
            Scan,
            Chase
        }
        private DogStates _state = DogStates.Scan;

        private bool _isActive = true;
        public bool IsActive 
        {
            get => _isActive;
            set => _isActive = value;
        }

        // Required Variables (Do not edit!)
        private float _maxSpeed = 5f;
        private float _sightRadius = 7.5f;

        // Turning and movement functionality
        private float _turnRate = 5f;
        private Rigidbody _rb;

        // Layers - Set In Project Settings
        private LayerMask _targetsLayer;
        private LayerMask _obstaclesLayer;

        // Tags - Set In Project Settings
        private const string friendTag = "Friend";
        private const string threatTag = "Threat";
        private const string safeZoneTag = "SafeZone";
        private const string untagged = "Untagged";

        // Timers and variables to control the robot dog scanning the level
        private float _timer = 0, firstTimer = 9f, xTimer = 13f, yTimer = 27f;
        private bool _across = false, _toStart = true, _chasing = false;
        private int _crossCount = 0, _sheepNum = 0;

        // Used to change the Dog's tag to threat or friend or otherwise
        [SerializeField]
        private GameObject _collTag;

        // Movement variables
        Vector3 targetPos = new Vector3(-17.5f, 0, 20), _velocity;

        // Storing safe zone and seen sheep
        private Vector3 _startPos;
        private Vector3[] _sheepPos = new Vector3[12];
        private List<int> _savedSheep = new List<int>();
        private Collider[] _tmpTargets = new Collider[16];

        public void Awake()
        {
            // Find the layers in the project settings
            _targetsLayer = LayerMask.GetMask("Targets");
            _obstaclesLayer = LayerMask.GetMask("Obstacles");

            // Gets rigidbody for movement and changes dog's tag
            _rb = GetComponent<Rigidbody>();
            _collTag.tag = friendTag;
        }

        private void Update()
        {
            if (!_isActive) return;
            
            // sets the safe zone's position here, doesn't work in Awake
            if (_timer == 0) _startPos = transform.position;

            // Shortens timer if spawning on top or left of level
            if ((_startPos.z >= 19 || _startPos.x <= -19) && _timer < 4) _timer = 5f;

            // Timer functionality for scanning
            _timer += Time.deltaTime;

            Perception();
            DecisionMaking();
        }

        private void Perception()
        {
            // Copied from Sheep
            int t = Physics.OverlapSphereNonAlloc(transform.position, _sightRadius, _tmpTargets, _targetsLayer);
            for (int i = 0; i < t; i++)
            {
                var c = _tmpTargets[i];
                if (c == null || c.gameObject == gameObject) continue;

                // Store the sheep
                switch (c.tag)
                {
                    case friendTag:
                        if (c.name == "Collision") break;
                        int sheepNum = int.Parse(c.name.Split(' ')[1]);
                        _sheepPos[sheepNum] = c.transform.position;
                        break;
                }
            }
        }

        private void DecisionMaking()
        {
            foreach (var c in _sheepPos)
            {
                // checks for known sheep locations and that they haven't been saved yet
                switch (_state)
                {
                    case DogStates.Scan:
                        // Functionality to scan the map across and down
                        if (_timer >= firstTimer && _toStart)
                        {
                            Debug.Log("REACHED DESTINATION");
                            targetPos.x *= -1;
                            _timer += 4f;
                            _toStart = false;
                            _across = true;
                        }
                        if (_timer >= xTimer && _timer <= yTimer && !_across)
                        {
                            Debug.Log("REACHED DESTINATION");
                            targetPos.x *= -1;
                            _across = true;
                        }
                        if (_timer >= yTimer && _crossCount < 4)
                        {
                            _crossCount++;
                            targetPos.z -= 10;
                            _timer = 9;
                            _across = false;
                        }
                        if (_timer >= yTimer && _crossCount == 4)
                        {
                            _state = DogStates.Chase;
                        }
                        break;
                    case DogStates.Chase:
                        // Lures the sheep back to the safe zone
                        targetPos = _startPos;
                        //if (!_chasing && !_savedSheep.Contains(_sheepNum))
                        //{
                        //    targetPos = _sheepPos[_sheepNum] + (5f * (_sheepPos[_sheepNum] - _startPos).normalized);
                        //    if (transform.position - _sheepPos[_sheepNum] == transform.position - _startPos)
                        //    {
                        //        _chasing = true;
                        //        _collTag.tag = threatTag;
                        //        targetPos = _startPos;
                        //    }
                        //}
                        //else if (_sheepNum < 11)
                        //{
                        //    _sheepNum++;
                        //    _collTag.tag = friendTag;
                        //}
                        break;
                }
            }
        }

        public void SaveSheep(int sheepNum)
        {
            _savedSheep.Add(sheepNum);
        }

        /// <summary>
        /// Make sure to use FixedUpdate for movement with physics based Rigidbody
        /// You can optionally use FixedDeltaTime for movement calculations, but it is not required since fixedupdate is called at a fixed rate
        /// </summary>
        private void FixedUpdate()
        {
            if (!_isActive) return;

            // speed control for scanning and luring sheep
            float speed = _maxSpeed;
            if (!_toStart) speed *= .575f;

            // Copied from Sheep
            // Calculate the direction to the target position
            Vector3 direction = (targetPos - transform.position).normalized;

            // Calculate the movement vector
            _velocity = direction * Mathf.Min(speed, Vector3.Distance(transform.position, targetPos));

            // Calculate the desired rotation towards the movement vector
            if (_velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_velocity);

                // Smoothly rotate towards the target rotation based on the turn rate
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnRate);
            }

            // Moves in target direction 
            _rb.linearVelocity = _velocity;
        }
    }
}
