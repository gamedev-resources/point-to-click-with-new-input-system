using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Scripts
{
    public enum MovementStates
    {
        None,
        Walk,
        Run
    }

    public class AnimationController : MonoBehaviour
    {
        public static AnimationController Instance;

        private MovementStates _currentState;
        public MovementStates CurrentState
        {
            get => _currentState;
            set
            {
                switch (value)
                {
                    case MovementStates.None:
                        _animatorController.SetBool("Walk", false);
                        _animatorController.SetBool("Run", false);
                        break;
                    case MovementStates.Walk:
                        _animatorController.SetBool("Walk", true);
                        _animatorController.SetBool("Run", false);
                        break;
                    case MovementStates.Run:
                        _animatorController.SetBool("Run", true);
                        _animatorController.SetBool("Walk", false);
                        break;
                }

                _currentState = value;
            }
        }

        [SerializeField]
        private Animator _animatorController;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this)
            {
                Debug.LogError($"Two instances of AnimationController found. Destroying object: {this.name}");
                Destroy(this);
            }
        }
    }
}
