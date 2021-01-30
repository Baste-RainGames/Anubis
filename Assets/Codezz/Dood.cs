using System;
using UnityEngine;
using UnityEngine.AI;

using static BT<Dood.DoodAIInput, Dood.DoodAIOutput>;

[SelectionBase]
public class /*HTML Rulez */Dood : MonoBehaviour {

    private BehaviourTree bt;
    private NavMeshAgent navMeshAgent;
    private Player player;

    public float attackRange;
    public float visionRange;

    private void Start() {
        player = FindObjectOfType<Player>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        bt = new BehaviourTree(
            Selector(
                Sequence(
                    PlayerInAttackRange(),
                    Attack()
                ),
                Sequence(
                    CanSeePlayer(),
                    MoveToPlayer()
                ),
                Sequence(
                    MoveRandom(),
                    WaitRandom(3, 5)
                )
            )
        );

        bt.data.agent = navMeshAgent;
    }

    private void Update() {
        UpdateAIData(ref bt.data);
        bt.Tick();
        ApplyAICommand(bt.command);
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
    }

    private void ApplyAICommand(DoodAIOutput btCommand) {
        if (btCommand.moveTo.HasValue)
            navMeshAgent.SetDestination(btCommand.moveTo.Value);

        if (btCommand.stopMoving)
            navMeshAgent.isStopped = true;
    }

#region AI


    public struct DoodAIInput {
        public NavMeshAgent agent;

        public Vector3 playerPos;
        public Vector3 doodPos;
        public bool playerExists;
        public float doodAttackRange;
        public float doodVisionRange;
    }

    public struct DoodAIOutput : BTCommand {
        public Vector3? moveTo;
        public bool stopMoving;

        public void Clear() {
            moveTo = null;
            stopMoving = false;
        }
    }

    private BTNode PlayerInAttackRange() => new PlayerInAttackRangeNode();
    private BTNode Attack() => new AttackPlayerNode();
    private BTNode CanSeePlayer() => new CanSeePlayerNode();
    private BTNode MoveToPlayer() => new MoveToPlayerNode();
    private BTNode MoveRandom() => new MoveRandomNode();

    public class MoveToPlayerNode : BTNode {
        protected override void OnStartedTicking() {
            tree.command = new DoodAIOutput {
                moveTo = data.playerPos
            };
        }

        protected override BTState OnTick() {
            if (data.agent.pathPending)
                return BTState.Continue;

            if (data.agent.isPathStale || !data.agent.hasPath)
                return BTState.Failure;

            if (data.agent.remainingDistance <= data.agent.stoppingDistance)
                return BTState.Success;

            return BTState.Continue;
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
        protected override BTState OnTick() {
            Debug.Log("Attack, yo!");
            return BTState.Success;
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
        protected override BTState OnTick() {
            return BTState.Continue;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Move randomly");
        }
    }

#endregion

}

