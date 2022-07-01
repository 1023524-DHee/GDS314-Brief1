using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace MyPlayer.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement current;

        private Rigidbody _rigidbody;
        private Vector3 _playerInput;
        private Vector3 _twitchForceModifier;
        private Vector3 _outwardForceModifier;
        
        private Vector3 _inputDir;
        private Vector3 _forceDir;
        
        private bool _isGrounded;

        public Transform playerModel;
        public Animator playerAnimator;

        public Transform groundDetector;
        public float groundDetectorRange;
        public LayerMask groundLayer;
        public float movementSpeed = 5f;

        public static Action<float, float> OnApplyForce;
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int Landed1 = Animator.StringToHash("Landed");
        private static readonly int Fall = Animator.StringToHash("Fall");

        private void Awake()
		{
			current = this;
            playerAnimator.SetTrigger(Fall);
		}

		private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _playerInput = Vector3.zero;
            _twitchForceModifier = Vector3.zero;
        }

        private void OnEnable()
        {
            OnApplyForce += SetTwitchForceModifier;
        }

        private void OnDisable()
        {
            OnApplyForce -= SetTwitchForceModifier;
        }

        private void Update()
        {
            CheckInput();
            CheckGrounded();
            HandleAnimation();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyRotation();
        }

        private void CheckInput()
        {
            _playerInput.x = Input.GetAxisRaw("Horizontal");
            _playerInput.z = Input.GetAxisRaw("Vertical");
        }

        private void ApplyMovement()
        {
            if (!CanMove()) return;
            
            _inputDir = _playerInput * (movementSpeed * Time.deltaTime);
            _forceDir = (_twitchForceModifier.normalized + _outwardForceModifier.normalized) * (movementSpeed * 0.9f * Time.deltaTime);
            Vector3 movementDir = _inputDir + _forceDir;
            movementDir.y = _rigidbody.velocity.y;
            _rigidbody.velocity = movementDir;
        }

        private void ApplyRotation()
        {
            float yRotation = Mathf.Rad2Deg * Mathf.Atan2(_inputDir.x, _inputDir.z);
            playerModel.DORotate(new Vector3(0,yRotation, 0f), 0.3f);
        }
        
        private void SetTwitchForceModifier(float xVal, float zVal)
        {
            _twitchForceModifier.x = xVal;
            _twitchForceModifier.z = zVal;
        }

        public void ApplyOutwardForce(Vector3 forceDir)
        {
            Vector3 force = forceDir;
            if (Math.Abs(_twitchForceModifier.x - 1) < 0.01f) force.x = 0;
            if (Math.Abs(_twitchForceModifier.z - 1) < 0.01f) force.z = 0;
            _outwardForceModifier = force;
        }

        private void CheckGrounded()
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, groundDetectorRange, groundLayer);
            if (!wasGrounded && _isGrounded) Landed();
        }

        private bool CanMove()
        {
            if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") ||
                playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Running")) return true;
            return false;
        }

        private void Landed()
        {
            playerAnimator.SetTrigger(Landed1);
        }
        
        private void HandleAnimation()
        {
            playerAnimator.SetFloat(MoveSpeed, _inputDir.magnitude);
        }
    }
}

