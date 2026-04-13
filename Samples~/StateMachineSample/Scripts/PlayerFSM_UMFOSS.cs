using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    public class PlayerFSM_UMFOSS : MonoBehaviour
    {
        // components shared across all states
        [HideInInspector] public Rigidbody2D    rb;
        [HideInInspector] public SpriteRenderer sr;
        [HideInInspector] public GroundDetector groundDetector;

        [Header("Movement")]
        public float moveSpeed    = 5f;
        public float jumpForce    = 12f;
        public float dashSpeed    = 18f;
        public float dashDuration = 0.18f;
        public float coyoteTime   = 0.12f;

        [Header("Combat")]
        public float hurtDuration  = 0.3f;
        public float knockbackForce = 6f;

        // shared data written/read across states
        [HideInInspector] public float   coyoteTimer;
        [HideInInspector] public float   dashTimer;
        [HideInInspector] public float   hurtTimer;
        [HideInInspector] public Vector2 dashDirection;
        [HideInInspector] public bool    hurtFlag;
        [HideInInspector] public bool    deadFlag;
        [HideInInspector] public bool    isInvincible;

        private StateMachine_UMFOSS fsm;

        private void Awake()
        {
            rb             = GetComponent<Rigidbody2D>();
            sr             = GetComponent<SpriteRenderer>();
            groundDetector = GetComponent<GroundDetector>();
            BuildFSM();
        }

        private void BuildFSM()
        {
            fsm = new StateMachine_UMFOSS(gameObject);

            var idle  = new IdleState(this);
            var run   = new RunState(this);
            var jump  = new JumpState(this);
            var fall  = new FallState(this);
            var dash  = new DashState(this);
            var hurt  = new HurtState(this);
            var dead  = new DeadState(this);

            fsm.AddTransition(idle, run,  () => Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f);
            fsm.AddTransition(run,  idle, () => Mathf.Abs(Input.GetAxis("Horizontal")) < 0.1f);
            fsm.AddTransition(idle, jump, () => Input.GetKeyDown(KeyCode.Space) && (groundDetector.IsGrounded || coyoteTimer > 0));
            fsm.AddTransition(run,  jump, () => Input.GetKeyDown(KeyCode.Space) && (groundDetector.IsGrounded || coyoteTimer > 0));
            fsm.AddTransition(jump, fall, () => rb.velocity.y < 0);
            fsm.AddTransition(fall, idle, () => groundDetector.IsGrounded);
            fsm.AddTransition(idle, dash, () => Input.GetKeyDown(KeyCode.LeftShift));
            fsm.AddTransition(run,  dash, () => Input.GetKeyDown(KeyCode.LeftShift));
            fsm.AddTransition(dash, idle, () => dashTimer <= 0);

            // hurt and dead fire from ANY state
            fsm.AddAnyTransition(hurt, () => hurtFlag && !isInvincible);
            fsm.AddAnyTransition(dead, () => deadFlag);

            fsm.ChangeState(idle);
        }

        private void Update()      => fsm.Tick();
        private void FixedUpdate() => fsm.FixedTick();

        // call this from damage system to trigger hurt state
        public void TakeDamage() => hurtFlag = true;
        public void Kill()       => deadFlag  = true;

        // ── States ────────────────────────────────────────────────────────────

        private class IdleState : IState_UMFOSS
        {
            private readonly PlayerFSM_UMFOSS p;
            public IdleState(PlayerFSM_UMFOSS p) => this.p = p;

            public void OnEnter()     { p.sr.color = Color.white; }
            public void OnExit()      { }

            public void OnTick()
            {
                // coyote timer counts down when player just walked off a ledge
                if (!p.groundDetector.IsGrounded)
                    p.coyoteTimer -= Time.deltaTime;
                else
                    p.coyoteTimer = p.coyoteTime;
            }

            public void OnFixedTick()
            {
                // decelerate to zero
                p.rb.velocity = new Vector2(
                    Mathf.MoveTowards(p.rb.velocity.x, 0f, p.moveSpeed * 0.2f),
                    p.rb.velocity.y
                );
            }
        }

        private class RunState : IState_UMFOSS
        {
            private readonly PlayerFSM_UMFOSS p;
            public RunState(PlayerFSM_UMFOSS p) => this.p = p;

            public void OnEnter() { p.sr.color = Color.cyan; }
            public void OnExit()  { }

            public void OnTick()
            {
                float h = Input.GetAxis("Horizontal");
                if (h != 0)
                    p.sr.flipX = h < 0;
            }

            public void OnFixedTick()
            {
                float h = Input.GetAxis("Horizontal");
                p.rb.velocity = new Vector2(h * p.moveSpeed, p.rb.velocity.y);
            }
        }

        private class JumpState : IState_UMFOSS
        {
            private readonly PlayerFSM_UMFOSS p;
            public JumpState(PlayerFSM_UMFOSS p) => this.p = p;

            public void OnEnter()
            {
                // apply jump force once on enter
                p.rb.velocity = new Vector2(p.rb.velocity.x, p.jumpForce);
                p.coyoteTimer = 0f;
                p.sr.color    = Color.yellow;
            }

            public void OnExit() { }

            public void OnTick()
            {
                float h = Input.GetAxis("Horizontal");
                if (h != 0) p.sr.flipX = h < 0;
            }

            public void OnFixedTick()
            {
                float h = Input.GetAxis("Horizontal");
                p.rb.velocity = new Vector2(h * p.moveSpeed, p.rb.velocity.y);
            }
        }

        private class FallState : IState_UMFOSS
        {
            private readonly PlayerFSM_UMFOSS p;
            public FallState(PlayerFSM_UMFOSS p) => this.p = p;

            public void OnEnter() { p.sr.color = Color.blue; }

            public void OnExit()
            {
                // landing event — other systems can subscribe via EventBus
                EventBus_UMFOSS.Publish(new PlayerLandedEvent { owner = p.gameObject });
            }

            public void OnTick()
            {
                float h = Input.GetAxis("Horizontal");
                if (h != 0) p.sr.flipX = h < 0;
            }

            public void OnFixedTick()
            {
                float h = Input.GetAxis("Horizontal");
                p.rb.velocity = new Vector2(h * p.moveSpeed, p.rb.velocity.y);
            }
        }

        private class DashState : IState_UMFOSS
        {
            private readonly PlayerFSM_UMFOSS p;
            public DashState(PlayerFSM_UMFOSS p) => this.p = p;

            public void OnEnter()
            {
                // store direction at moment of dash press
                float h = Input.GetAxis("Horizontal");
                p.dashDirection        = new Vector2(h == 0 ? (p.sr.flipX ? -1 : 1) : h, 0).normalized;
                p.dashTimer            = p.dashDuration;
                p.rb.gravityScale      = 0f;
                p.sr.color             = Color.magenta;
            }

            public void OnExit()
            {
                p.rb.gravityScale = 1f;
                p.rb.velocity     = Vector2.zero;
            }

            public void OnTick()      => p.dashTimer -= Time.deltaTime;

            public void OnFixedTick() => p.rb.velocity = p.dashDirection * p.dashSpeed;
        }

        private class HurtState : IState_UMFOSS
        {
            private readonly PlayerFSM_UMFOSS p;
            public HurtState(PlayerFSM_UMFOSS p) => this.p = p;

            public void OnEnter()
            {
                p.hurtTimer     = p.hurtDuration;
                p.isInvincible  = true;
                p.sr.color      = Color.red;

                // knockback away from center
                float dir = p.sr.flipX ? 1f : -1f;
                p.rb.velocity = new Vector2(dir * p.knockbackForce, p.knockbackForce * 0.5f);
            }

            public void OnExit()
            {
                p.isInvincible = false;
                p.hurtFlag     = false;
                p.sr.color     = Color.white;
            }

            public void OnTick()
            {
                p.hurtTimer -= Time.deltaTime;
                if (p.hurtTimer <= 0)
                    p.fsm.ChangeState(new IdleState(p)); // return to idle after hurt window
            }

            public void OnFixedTick() { }
        }

        private class DeadState : IState_UMFOSS
        {
            private readonly PlayerFSM_UMFOSS p;
            public DeadState(PlayerFSM_UMFOSS p) => this.p = p;

            public void OnEnter()
            {
                p.rb.velocity  = Vector2.zero;
                p.sr.color     = Color.gray;
                p.enabled      = false; // stop Update/FixedUpdate
                EventBus_UMFOSS.Publish(new PlayerDiedEvent { owner = p.gameObject });
            }

            public void OnTick()      { }
            public void OnFixedTick() { }
            public void OnExit()      { p.enabled = true; } // on respawn
        }
    }

    // events published by player states
    public struct PlayerLandedEvent { public object owner; }
    public struct PlayerDiedEvent   { public object owner; }
}
