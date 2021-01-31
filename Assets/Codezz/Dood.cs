using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using static BT<Dood.DoodAIInput, Dood.DoodAIOutput>;

[SelectionBase]
public class /*HTML Rulez */Dood : MonoBehaviour {
    public BehaviourTree behaviourTree;
    public DoodAnimation anim;
    public Hitbox hitbox;

    private NavMeshAgent navMeshAgent;
    private Player player;

    public float attackRange;
    public float visionRange;
    public float timeBeforeHitbox;
    public int maxHealth = 5;
    public int currentHealth = 5;
    private bool dead;

    private void Start() {
        player = FindObjectOfType<Player>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        behaviourTree = new BehaviourTree(
            Selector(
                Sequence(
                    PlayerInAttackRange(),
                    Attack(),
                    TurnToFacePlayer()
                ),
                Sequence(
                    CanSeePlayer(),
                    Succeeder(
                        Sequence(
                            DoesNotHaveAggro(),
                            Aggro()
                        )
                    ),
                    TurnToFacePlayer(),
                    MoveToPlayer()
                ),
                Sequence(
                    NoteLostAggro(),
                    MoveRandom(),
                    WaitRandom(2, 2.5f)
                )
            )
        );

        behaviourTree.data.agent = navMeshAgent;
        behaviourTree.data.durationOfAnim = anim.ClipDurations;
    }

    private void Update() {
        if (dead)
            return;
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
        btData.doodRot = transform.rotation;
        btData.doodAttackRange = attackRange;
        btData.doodVisionRange = visionRange;
        btData.attackDuration = timeBeforeHitbox;
    }

    private void ApplyAICommand(DoodAIOutput btCommand) {
        if (btCommand.moveTo.HasValue) {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(btCommand.moveTo.Value);
        }

        if (btCommand.stopMoving)
            navMeshAgent.isStopped = true;

        if (!string.IsNullOrEmpty(btCommand.playAnimation)) {
            anim.Play(btCommand.playAnimation);
        }

        if (btCommand.markAggroAs.HasValue) {
            behaviourTree.data.hasAggro = btCommand.markAggroAs.Value;
            anim.HasAggro = btCommand.markAggroAs.Value;
        }

        if (btCommand.turnTowards.HasValue) {
            transform.LookAt2D(btCommand.turnTowards.Value, Time.deltaTime * 180f);
            anim.IsTurning = true;
        }
        else {
            anim.IsTurning = false;
        }
    }

    public void ActivateHitbox() {
        var hits = hitbox.PollHit();
        foreach (var hit in hits) {
            if (hit.gameObject.TryGetComponent<Player>(out var p))
                p.OnHit();
        }
    }

    public void OnHit(int damage) {
        currentHealth -= damage;
        if (currentHealth <= 0)
            OnDeath();
    }

    private void OnDeath() {
        Debug.Log("Die");
        var dur = anim.Play("zomb-die");
        dead = true;
        Destroy(gameObject, dur);
    }

#region AI

    public struct DoodAIInput {
        public NavMeshAgent agent;
        public Dictionary<string, float> durationOfAnim;

        public Vector3 playerPos;
        public Vector3 doodPos;
        public Quaternion doodRot;
        public bool playerExists;

        public bool hasAggro;

        public float doodAttackRange;
        public float doodVisionRange;
        public float attackDuration;
    }

    public struct DoodAIOutput : BTCommand {
        public Vector3? moveTo;
        public bool stopMoving;
        public string playAnimation;
        public bool? markAggroAs;
        public Vector3? turnTowards;

        public void Clear() {
            moveTo = null;
            stopMoving = false;
            playAnimation = null;
            markAggroAs = null;
            turnTowards = null;
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

            if (Vector3.Distance(data.playerPos, data.doodPos) > data.doodVisionRange)
                return BTState.Failure;

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
        private float endTime;

        protected override void OnStartedTicking() {
            command = new DoodAIOutput {
                stopMoving = true,
                playAnimation = "enemy-punch",
            };

            endTime = Time.time + data.durationOfAnim["enemy-punch"];
        }

        protected override BTState OnTick() {
            if (Time.time > endTime)
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


    private BTNode DoesNotHaveAggro() => new CheckAggroNode();

    private class CheckAggroNode : BTNode {
        protected override BTState OnTick() {
            if (!data.hasAggro)
                return BTState.Success;
            return BTState.Failure;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Check Has Aggro");
        }
    }

    private BTNode TurnToFacePlayer() => new TurnToFacePlayerNode();

    private class TurnToFacePlayerNode : BTNode {
        protected override BTState OnTick() {
            if (Vector3.Distance(data.playerPos, data.doodPos) > data.doodVisionRange)
                return BTState.Failure;

            var angleToPlayer = FindAngleToPlayer();
            if (angleToPlayer < 15f)
                return BTState.Success;

            command = new DoodAIOutput {
                turnTowards = data.playerPos
            };

            return BTState.Continue;
        }

        private float FindAngleToPlayer() {
            Vector3 target2D = new Vector3(data.playerPos.x, data.doodPos.y, data.playerPos.z);

            Quaternion wantedRotation  = Quaternion.LookRotation(target2D - data.doodPos);

            return Quaternion.Angle(wantedRotation, data.doodRot);
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Turn Towards Player");
        }
    }

    private BTNode NoteLostAggro() => new NoteLostAggroNode();

    private class NoteLostAggroNode : BTNode {
        protected override BTState OnTick() {
            command = new DoodAIOutput {
                markAggroAs = false
            };
            return BTState.Success;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Note Lost Aggro");
        }
    }

    private BTNode Aggro() => new AggroNode();

    private class AggroNode : BTNode {
        private float endTime;

        protected override void OnStartedTicking() {
            command = new DoodAIOutput {
                markAggroAs = true,
                playAnimation = "zomb-aggro",
                stopMoving = true
            };

            endTime = Time.time + data.durationOfAnim["zomb-aggro"];
        }

        protected override BTState OnTick() {
            if (Time.time > endTime)
                return BTState.Success;

            return BTState.Continue;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            if (tree.WasTickedLastFrame(index))
                AppendLine(this, $"Aggro ({endTime - Time.time} seconds left)");
            else
                AppendLine(this, "Aggro");
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