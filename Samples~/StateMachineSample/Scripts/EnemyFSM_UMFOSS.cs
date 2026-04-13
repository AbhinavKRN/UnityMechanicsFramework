using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    public class EnemyFSM_UMFOSS : MonoBehaviour
    {
        [Header("Detection")]
        public float sightRange    = 8f;
        public float attackRange   = 1.5f;
        public float alertDuration = 2f;

        [Header("Movement")]
        public float patrolSpeed = 2f;
        public float chaseSpeed  = 4f;

        [Header("Patrol")]
        public Transform[] waypoints;

        // shared references
        [HideInInspector] public Transform   player;
        [HideInInspector] public Rigidbody2D rb;
        [HideInInspector] public SpriteRenderer sr;

        // shared state data
        [HideInInspector] public float alertDurationTimer;
        [HideInInspector] public bool  stunnedFlag;
        [HideInInspector] public bool  deadFlag;
        [HideInInspector] public float stunnedDuration = 1f;
        [HideInInspector] public float stunnedTimer;

        private StateMachine_UMFOSS fsm;

        private void Awake()
        {
            rb     = GetComponent<Rigidbody2D>();
            sr     = GetComponent<SpriteRenderer>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            BuildFSM();
        }

        private void BuildFSM()
        {
            fsm = new StateMachine_UMFOSS(gameObject);

            var patrol  = new PatrolState(this);
            var alert   = new AlertState(this);
            var chase   = new ChaseState(this);
            var attack  = new AttackState(this);
            var stunned = new StunnedState(this);
            var dead    = new DeadState(this);

            float DistToPlayer() => player != null
                ? Vector2.Distance(transform.position, player.position)
                : float.MaxValue;

            fsm.AddTransition(patrol, alert,  () => DistToPlayer() < sightRange);
            fsm.AddTransition(alert,  chase,  () => DistToPlayer() < sightRange);
            fsm.AddTransition(alert,  patrol, () => alertDurationTimer <= 0);
            fsm.AddTransition(chase,  attack, () => DistToPlayer() < attackRange);
            fsm.AddTransition(chase,  alert,  () => DistToPlayer() > sightRange);
            fsm.AddTransition(attack, chase,  () => DistToPlayer() > attackRange);

            fsm.AddAnyTransition(stunned, () => stunnedFlag);
            fsm.AddAnyTransition(dead,    () => deadFlag);

            fsm.ChangeState(patrol);
        }

        private void Update()      => fsm.Tick();
        private void FixedUpdate() => fsm.FixedTick();

        public void Stun() => stunnedFlag = true;
        public void Kill() => deadFlag    = true;

        // ── States ────────────────────────────────────────────────────────────

        private class PatrolState : IState_UMFOSS
        {
            private readonly EnemyFSM_UMFOSS e;
            private int waypointIndex;

            public PatrolState(EnemyFSM_UMFOSS e) => this.e = e;

            public void OnEnter() { e.sr.color = Color.white; }
            public void OnExit()  { }
            public void OnTick()  { }

            public void OnFixedTick()
            {
                if (e.waypoints == null || e.waypoints.Length == 0) return;

                var target = e.waypoints[waypointIndex].position;
                var dir    = ((Vector2)target - (Vector2)e.transform.position).normalized;

                e.rb.velocity = new Vector2(dir.x * e.patrolSpeed, e.rb.velocity.y);
                e.sr.flipX    = dir.x < 0;

                if (Vector2.Distance(e.transform.position, target) < 0.2f)
                    waypointIndex = (waypointIndex + 1) % e.waypoints.Length;
            }
        }

        private class AlertState : IState_UMFOSS
        {
            private readonly EnemyFSM_UMFOSS e;
            public AlertState(EnemyFSM_UMFOSS e) => this.e = e;

            public void OnEnter()
            {
                e.alertDurationTimer = e.alertDuration;
                e.sr.color           = Color.yellow;
                e.rb.velocity        = Vector2.zero;
            }

            public void OnExit() { }

            public void OnTick()      => e.alertDurationTimer -= Time.deltaTime;
            public void OnFixedTick() { }
        }

        private class ChaseState : IState_UMFOSS
        {
            private readonly EnemyFSM_UMFOSS e;
            public ChaseState(EnemyFSM_UMFOSS e) => this.e = e;

            public void OnEnter() { e.sr.color = Color.red; }
            public void OnExit()  { }
            public void OnTick()  { }

            public void OnFixedTick()
            {
                if (e.player == null) return;

                var dir = ((Vector2)e.player.position - (Vector2)e.transform.position).normalized;
                e.rb.velocity = new Vector2(dir.x * e.chaseSpeed, e.rb.velocity.y);
                e.sr.flipX    = dir.x < 0;
            }
        }

        private class AttackState : IState_UMFOSS
        {
            private readonly EnemyFSM_UMFOSS e;
            private float attackTimer;
            private const float AttackInterval = 0.8f;

            public AttackState(EnemyFSM_UMFOSS e) => this.e = e;

            public void OnEnter()
            {
                attackTimer   = 0f;
                e.rb.velocity = Vector2.zero;
                e.sr.color    = Color.magenta;
            }

            public void OnExit() { }

            public void OnTick()
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= AttackInterval)
                {
                    attackTimer = 0f;
                    Debug.Log($"{e.name} attacks player!");
                    EventBus_UMFOSS.Publish(new EnemyAttackedEvent { owner = e.gameObject });
                }
            }

            public void OnFixedTick() { }
        }

        private class StunnedState : IState_UMFOSS
        {
            private readonly EnemyFSM_UMFOSS e;
            public StunnedState(EnemyFSM_UMFOSS e) => this.e = e;

            public void OnEnter()
            {
                e.stunnedTimer = e.stunnedDuration;
                e.rb.velocity  = Vector2.zero;
                e.sr.color     = Color.cyan;
            }

            public void OnExit()
            {
                e.stunnedFlag = false;
                e.sr.color    = Color.white;
            }

            public void OnTick()
            {
                e.stunnedTimer -= Time.deltaTime;
                if (e.stunnedTimer <= 0)
                    e.fsm.ChangeState(new PatrolState(e));
            }

            public void OnFixedTick() { }
        }

        private class DeadState : IState_UMFOSS
        {
            private readonly EnemyFSM_UMFOSS e;
            public DeadState(EnemyFSM_UMFOSS e) => this.e = e;

            public void OnEnter()
            {
                e.rb.velocity = Vector2.zero;
                e.sr.color    = Color.gray;
                e.enabled     = false;
                EventBus_UMFOSS.Publish(new EnemyDiedEvent { owner = e.gameObject });
            }

            public void OnTick()      { }
            public void OnFixedTick() { }
            public void OnExit()      { }
        }
    }

    // events published by enemy states
    public struct EnemyAttackedEvent { public object owner; }
    public struct EnemyDiedEvent     { public object owner; }
}
