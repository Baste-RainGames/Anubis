using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

using static BT<Dood.DoodAIInput, Dood.DoodAIOutput>;

[SelectionBase]
public class /*HTML Rulez */Dood : MonoBehaviour {

    public BehaviourTree behaviourTree;
    public Animator animator;

    private NavMeshAgent navMeshAgent;
    private Player player;

    public float attackRange;
    public float visionRange;
    public float attackDuration;

    private void Start() {
        player = FindObjectOfType<Player>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        behaviourTree = new BehaviourTree(
            Selector(
                Sequence(
                    PlayerInAttackRange(),
                    Attack(),
                    Wait(1f)
                ),
                Sequence(
                    CanSeePlayer(),
                    MoveToPlayer()
                ),
                Sequence(
                    MoveRandom(),
                    WaitRandom(2, 2.5f)
                )
            )
        );

        behaviourTree.data.agent = navMeshAgent;
    }

    private void Update() {
        UpdateAIData(ref behaviourTree.data);
        behaviourTree.Tick();
        ApplyAICommand(behaviourTree.command);
    }

    private void UpdateAIData(ref DoodAIInput btData) {
        if (player == null)
            player = FindObjectOfType<Player>();

        btData.playerExists = player != null;
        if (player != null)
            btData.playerPos = player.transform.position;

        btData.doodPos = transform.position;
        btData.doodAttackRange = attackRange;
        btData.doodVisionRange = visionRange;
        btData.attackDuration = attackDuration;
    }

    private void ApplyAICommand(DoodAIOutput btCommand) {
        if (btCommand.moveTo.HasValue) {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(btCommand.moveTo.Value);
        }

        if (btCommand.stopMoving)
            navMeshAgent.isStopped = true;

        if (!string.IsNullOrEmpty(btCommand.playAnimation)) {
            animator.Play(btCommand.playAnimation);
        }
    }

#region AI


    public struct DoodAIInput {
        public NavMeshAgent agent;

        public Vector3 playerPos;
        public Vector3 doodPos;
        public bool playerExists;

        public float doodAttackRange;
        public float doodVisionRange;
        public float attackDuration;
    }

    public struct DoodAIOutput : BTCommand {
        public Vector3? moveTo;
        public bool stopMoving;
        public string playAnimation;

        public void Clear() {
            moveTo = null;
            stopMoving = false;
        }
    }

    private static BTState MoveUntilDone(DoodAIInput data) {
        if (data.agent.pathPending)
            return BTState.Continue;

        if (data.agent.isPathStale) {
            return BTState.Failure;
        }

        if (data.agent.remainingDistance <= data.agent.stoppingDistance || !data.agent.hasPath)
            return BTState.Success;

        return BTState.Continue;
    }

    private BTNode PlayerInAttackRange() => new PlayerInAttackRangeNode();
    private BTNode Attack() => new AttackPlayerNode();
    private BTNode CanSeePlayer() => new CanSeePlayerNode();
    private BTNode MoveToPlayer() => new MoveToPlayerNode();
    private BTNode MoveRandom() => new MoveRandomNode();

    public class MoveToPlayerNode : BTNode {
        private bool hasMoved;

        protected override void OnStartedTicking() {
            hasMoved = false;
        }

        protected override BTState OnTick() {
            if (!hasMoved) {
                hasMoved = true;
                tree.command = new DoodAIOutput {
                    moveTo = data.playerPos
                };
                return BTState.Continue;
            }

            if (Vector3.Distance(data.agent.destination, data.playerPos) > 1f) {
                tree.command = new DoodAIOutput {
                    moveTo = data.playerPos
                };
                return BTState.Continue;
            }

            return MoveUntilDone(data);
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Move to player");
        }
    }

    public class CanSeePlayerNode : BTNode {
        protected override BTState OnTick() {
            if (Vector3.Distance(data.playerPos, data.doodPos) < data.doodVisionRange)
                return BTState.Success;
            return BTState.Failure;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Can see Player");
        }
    }

    public class AttackPlayerNode : BTNode {
        private float startTime;

        protected override void OnStartedTicking() {
            command = new DoodAIOutput {
                stopMoving = true,
                playAnimation = "weapon_swing"
            };

            startTime = Time.time;
        }

        protected override BTState OnTick() {
            if (Time.time - startTime > data.attackDuration)
                return BTState.Success;

            return BTState.Continue;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Attack");
        }
    }

    public class PlayerInAttackRangeNode : BTNode {
        protected override BTState OnTick() {
            if (!data.playerExists)
                return BTState.Failure;

            if (Vector3.Distance(data.playerPos, data.doodPos) < data.doodAttackRange)
                return BTState.Success;
            return BTState.Failure;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Player In Attack Range?");
        }
    }

    public class MoveRandomNode : BTNode {
        private Vector3 moveTo;
        private bool hasMoved;

        protected override void OnStartedTicking() {
            base.OnStartedTicking();

            hasMoved = false;
            moveTo = FindRandomTargetPos();
        }

        private Vector3 FindRandomTargetPos() {
            Vector3 targetPos = default;
            for (int i = 0; i < 10; i++) {
                var randomDir = Random.insideUnitCircle.normalized;
                var random3D = new Vector3(randomDir.x, 0, randomDir.y);

                targetPos = data.doodPos + 3f * random3D;

                if (NavMesh.Raycast(data.doodPos, targetPos, out var hit, -1))
                    targetPos = hit.position;

                if (Vector3.Distance(targetPos, data.doodPos) > 2f)
                    break;
            }

            return targetPos;
        }

        protected override BTState OnTick() {
            if (!hasMoved) {
                hasMoved = true;
                command = new DoodAIOutput {
                    moveTo = moveTo
                };
                return BTState.Continue;
            }
            return MoveUntilDone(data);
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Move randomly");
        }
    }

#endregion

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        if (navMeshAgent && navMeshAgent.hasPath) {
            var corners = navMeshAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
                Gizmos.DrawLine(corners[i], corners[i + 1]);
        }
    }
}