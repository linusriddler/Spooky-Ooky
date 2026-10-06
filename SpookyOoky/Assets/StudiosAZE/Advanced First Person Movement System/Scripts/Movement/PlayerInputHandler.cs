using UnityEngine;
using UnityEngine.InputSystem;

namespace AZE.AdvancedFirstPerson
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference crouchAction;
        [SerializeField] private InputActionReference dodgeAction;

        private float maxDoubleTapTime = 0.2f;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool SprintPressed { get; private set; }
        public bool CrouchTriggered { get; private set; }
        public bool DodgeTriggered { get; private set; }
        public Vector2 DodgeDirection { get; private set; }


        private int _dodgeTapCount = 0;
        private float _lastTapTime;
        private Vector2 _lastDodgeInput;

        private void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            jumpAction.action.Enable();
            sprintAction.action.Enable();
            crouchAction.action.Enable();
            dodgeAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable(); 
            lookAction.action.Disable(); 
            jumpAction.action.Disable(); 
            sprintAction.action.Disable(); 
            crouchAction.action.Disable(); 
            dodgeAction.action.Disable();
        }

        private void Update()
        {
            MoveInput = moveAction.action.ReadValue<Vector2>();
            LookInput = lookAction.action.ReadValue<Vector2>();

            JumpTriggered = jumpAction.action.WasPressedThisFrame();
            SprintPressed = sprintAction.action.IsPressed();

            if (crouchAction.action.WasPressedThisFrame())
                CrouchTriggered = !CrouchTriggered;

            // Press Tab to dodge
             DodgeTriggered = dodgeAction.action.WasPressedThisFrame();
        }

        public InputDevice GetLookDevice => lookAction.action.activeControl?.device;

        public void UseDodge() => DodgeTriggered = false;

       
    }
}