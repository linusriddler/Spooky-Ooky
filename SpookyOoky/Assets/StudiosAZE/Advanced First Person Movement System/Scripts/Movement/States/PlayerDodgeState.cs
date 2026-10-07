using UnityEngine;

namespace AZE.AdvancedFirstPerson
{
    public class PlayerDodgeState : PlayerBaseState
    {
        private float _timer;
        private Vector3 _dodgeDirection;

        public PlayerDodgeState(PlayerMovementStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

        public override void EnterState()
        {
            if (!ctx.useDodge) return;

            _timer = 0f;
            ctx.LastDodgeTime = Time.time;
            ctx.TargetHeight = ctx.GetStandingHeight();

            // Get the direction the player is currently holding
            Vector2 moveInput = ctx.InputHandler.MoveInput;

            Vector3 localDir = Vector3.zero;

            // S = backward
            if (moveInput.y < -0.1f) localDir += Vector3.back;

            // A = left
            if (moveInput.x < -0.1f) localDir += Vector3.left;

            // D = right
            if (moveInput.x > 0.1f) localDir += Vector3.right;

            // W = forward
            if (moveInput.y > 0.1f) localDir += Vector3.forward;

            // If no direction is being held, 
            // dodge backward
            if (localDir == Vector3.zero)
            {
                localDir = Vector3.back;
            }

            _dodgeDirection = ctx.CameraTransform.TransformDirection(localDir);
            _dodgeDirection.y = 0;
            _dodgeDirection.Normalize();

            ctx.InputHandler.UseDodge();
        }

        public override void UpdateState()
        {
            ctx.HandleGravity();

            if (_timer < ctx.DodgeDuration)
            {
                float currentDodgeSpeed = Mathf.Lerp(ctx.DodgeSpeed, 0f, _timer / ctx.DodgeDuration);
                ctx.CurrentMoveVelocity = _dodgeDirection * currentDodgeSpeed;
                _timer += Time.deltaTime;
            }
            else
            {
                CheckSwitchStates();
            }
        }

        public override void CheckSwitchStates()
        {
            if (ctx.InputHandler.SprintPressed)
                ctx.SwitchState(factory.Sprint);
            else
                ctx.SwitchState(factory.Walk);
        }

        public override void ExitState()
        {
            ctx.CurrentMoveVelocity = Vector3.zero;
        }

        public override void InitializeSubState() { }
    }
}