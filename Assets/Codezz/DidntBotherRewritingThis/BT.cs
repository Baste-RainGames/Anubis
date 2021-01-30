using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BTState {
    Failure,
    Success,
    Continue,
}

public interface BTCommand {
    void Clear();
}

public static class BT<TData, TCommand> where TCommand : struct, BTCommand where TData : struct {

    public static BTNode Sequence(params BTNode[] children) => new SequenceNode(children);
    public static BTNode Selector(bool shuffle, params BTNode[] children) => new SelectorNode(shuffle, children);
    public static BTNode Selector(params BTNode[] children) => new SelectorNode(false, children);
    public static BTNode Call(System.Action fn, string name = null) => new CallNode(fn, name);
    public static BTNode While(Func<bool> fn, BTNode child) => new WhileNode(fn).SetChild(child);
    public static BTNode Parallel(params BTNode[] children) => new ParallelNode(children);
    public static BTNode RepeatForever(BTNode child) => new RepeatForeverNode().SetChild(child);
    public static BTNode Repeat(int count, BTNode child) => new RepeatNode(count).SetChild(child);
    public static BTNode WaitUntil(Func<bool> condition) => new WaitUntilNode(condition);
    public static BTNode Wait(float seconds) => new WaitNode(seconds);
    public static BTNode WaitRandom(float minSeconds, float maxSeconds) => new WaitRandomNode(minSeconds, maxSeconds);
    public static BTNode Log(string msg) => new LogNode(() => msg);
    public static BTNode Log(Func<string> msg) => new LogNode(msg);
    public static BTNode RandomSequence(int[] weights = null) => new RandomSequenceNode(weights);
    public static BTNode PlaySound(string sound, GameObject parentGO = null) => new PlaySoundNode(sound, parentGO);

    // public static Func<bool> Or  (Func<bool> cond1, Func<bool> cond2) => () => cond1() || cond2();
    // public static Func<bool> And (Func<bool> cond1, Func<bool> cond2) => () => cond1() && cond2();
    public static Func<bool> Not(Func<bool> cond) => () => !cond();

    internal interface INodeWithChildren {
        IReadOnlyList<BTNode> Children();
    }

    public abstract class BTNode {
        // This array is shared by all nodes in the same tree
        internal int index;
        internal int debugDepth;

        internal BehaviourTree tree;

        protected TData data => tree.data;
        protected TCommand command {
            set => tree.command = value;
        }

        internal BTState Tick() {
            if (!tree.tickedLastFrame[index] || tree.lastTickResult[index] != BTState.Continue)
                OnStartedTicking();

            var tickResult = OnTick();

            tree.tickedThisFrame[index] = true;
            tree.lastTickResult[index] = tickResult;

            return tickResult;
        }

        /// <summary>
        /// Called when ticked if either the node wasn't ticked last frame, or if the node reported success or failure on the last frame.
        /// Aka. "if you didn't return Continue on the last frame".
        /// </summary>
        protected virtual void OnStartedTicking() { }

        protected abstract BTState OnTick();

        public abstract void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation);

        /// <summary>
        /// Resets the node to the default state. Called when Reset is called on the tree.
        /// Not neccessary if all state is set in OnStartedTicking, as tickedLastFrame is set false on all nodes when the tree is reset.
        /// </summary>
        public virtual void Reset() {}
    }

    public abstract class Branch : BTNode, INodeWithChildren {
        protected int activeChild;
        protected readonly List<BTNode> children = new List<BTNode>();

        protected Branch(params BTNode[] children) {
            OpenBranch(children);
        }

        protected virtual void OpenBranch(params BTNode[] children) {
            for (var i = 0; i < children.Length; i++) {
                this.children.Add(children[i]);
            }
        }

        public IReadOnlyList<BTNode> Children() {
            return children;
        }

        public override void Reset() {
            activeChild = 0;
        }
    }

    public abstract class Decorator : BTNode, INodeWithChildren {
        protected BTNode child;
        private List<BTNode> children;

        internal Decorator SetChild(BTNode child) {
            this.child = child;
            this.children = new List<BTNode> {child};
            return this;
        }

        public IReadOnlyList<BTNode> Children() {
            return children;
        }
    }

    public class SequenceNode : Branch {
        public SequenceNode(params BTNode[] children) : base(children) { }

        protected override void OnStartedTicking() {
            activeChild = 0;
        }

        protected override BTState OnTick() {
            var childState = children[activeChild].Tick();
            switch (childState) {
                case BTState.Success:
                    activeChild++;
                    if (activeChild == children.Count) {
                        activeChild = 0;
                        return BTState.Success;
                    }
                    else
                        return BTState.Continue;
                case BTState.Failure:
                    activeChild = 0;
                    return BTState.Failure;
                case BTState.Continue:
                    return BTState.Continue;
            }

            throw new Exception("This should never happen, but clearly it has.");
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Sequence ({children.Count} children)");
            IncreaseIndentation();
            foreach (var child in children)
                child.ShowAsString(AppendLine, IncreaseIndentation, DecreaseIndentation);
            DecreaseIndentation();
        }
    }

    /// <summary>
    /// Execute each child until a child succeeds, then return success.
    /// If no child succeeds, return a failure.
    /// </summary>
    public class SelectorNode : Branch {
        private bool shuffle;

        public SelectorNode(bool shuffle, params BTNode[] children) : base(children) {
            this.shuffle = shuffle;
        }

        protected override void OnStartedTicking() {
            if (shuffle) {
                var n = children.Count;
                while (n > 1) {
                    n--;
                    var k = Mathf.FloorToInt(Random.value * (n + 1));
                    var value = children[k];
                    children[k] = children[n];
                    children[n] = value;
                }
            }
        }

        protected override BTState OnTick() {
            foreach (var child in children) {
                var result = child.Tick();
                if (result != BTState.Failure)
                    return result;
            }

            return BTState.Failure;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Selector ({children.Count} children)");
            IncreaseIndentation();
            foreach (var child in children)
                child.ShowAsString(AppendLine, IncreaseIndentation, DecreaseIndentation);
            DecreaseIndentation();
        }
    }

    /// <summary>
    /// Call a method
    /// </summary>
    public class CallNode : BTNode {
        private readonly System.Action fn;
        private readonly string name;

        public CallNode(System.Action fn, string name) {
            this.fn = fn;
            this.name = name;
        }

        protected override BTState OnTick() {
            fn();
            return BTState.Success;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            if (name != null)
                AppendLine(this, name);
            AppendLine(this, $"Call \"{fn.Method.Name}\"");
        }

        public override void Reset() { }
    }

    public abstract class ConditionalNode : Decorator {
        private readonly Func<bool> check;
        internal virtual bool Check() => check();

        internal bool checkPassed;

        protected ConditionalNode(Func<bool> check) {
            this.check = check;
        }

        protected override void OnStartedTicking() {
            checkPassed = false;
        }

        internal virtual string MethodName => check.Method.Name;

        public override void Reset() {
            checkPassed = false;
        }
    }

    public abstract class IfNodeSimple : BTNode {
        public bool CheckPassed { get; private set; }

        protected override BTState OnTick() {
            CheckPassed = Check();
            if (CheckPassed)
                return BTState.Success;
            return BTState.Failure;
        }

        public override void Reset() { }
        protected abstract bool Check();
        protected abstract string DebugDisplay { get; }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Checking " + DebugDisplay);
        }
    }

    public class WhileNode : ConditionalNode {
        public WhileNode(Func<bool> check) : base(check) { }

        internal override bool Check() {
            var result = base.Check();
            checkPassed = result; // Debugging purposes
            return result;
        }

        protected override BTState OnTick() {
            if (Check())
                child.Tick();
            else {
                return BTState.Failure;
            }

            return BTState.Continue;
        }

        public WhileNode And(Func<bool> check, BTNode child) {
            var node = new WhileAnd(check, this);
            node.SetChild(child);
            return node;
        }

        public WhileNode Or(Func<bool> check, BTNode child) {
            var node = new WhileOr(check, this);
            node.SetChild(child);
            return node;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            var whileOutput = $"While {MethodName}";
            if (tree.WasTickedLastFrame(index))
                whileOutput = $"<color={(checkPassed ? "green" : "red")}>{whileOutput}</color>";

            AppendLine(this, whileOutput);
            IncreaseIndentation();
            child.ShowAsString(AppendLine, IncreaseIndentation, DecreaseIndentation);
            DecreaseIndentation();
        }
    }

    public class RepeatForeverNode : WhileNode {
        public RepeatForeverNode() : base(() => true) { }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, "Repeat Forever");
            IncreaseIndentation();
            child.ShowAsString(AppendLine, IncreaseIndentation, DecreaseIndentation);
            DecreaseIndentation();
        }
    }

    internal class WhileAnd : WhileNode {
        private WhileNode otherNode;

        public WhileAnd(Func<bool> check, WhileNode otherNode) : base(check) {
            this.otherNode = otherNode;
        }

        internal override bool Check() => base.Check() && otherNode.Check();

        internal override string MethodName => $"{base.MethodName} and {otherNode.MethodName}";
    }

    internal class WhileOr : WhileNode {
        private WhileNode otherNode;

        public WhileOr(Func<bool> check, WhileNode otherNode) : base(check) {
            this.otherNode = otherNode;
        }

        internal override bool Check() => base.Check() || otherNode.Check();

        internal override string MethodName => $"{base.MethodName} or {otherNode.MethodName}";
    }

    public class ParallelNode : Branch {
        public ParallelNode(params BTNode[] children) : base(children) { }

        protected override BTState OnTick() {
            var anyFailed = false;
            var allSuccess = true;
            foreach (var child in children) {
                var result = child.Tick();
                if (result == BTState.Failure)
                    anyFailed = true;
                if (result != BTState.Success)
                    allSuccess = false;
            }

            if (anyFailed)
                return BTState.Failure;
            if (allSuccess)
                return BTState.Success;
            return BTState.Continue;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Parallel ({children.Count} children)");
            IncreaseIndentation();
            foreach (var child in children)
                child.ShowAsString(AppendLine, IncreaseIndentation, DecreaseIndentation);
            DecreaseIndentation();
        }
    }

    public class BehaviourTree {
        private List<BTNode> allNodes;
        internal BTState[] lastTickResult;
        internal bool[] tickedThisFrame;
        internal bool[] tickedLastFrame;
        public TData data;
        public TCommand command;
        public bool WasTickedLastFrame(int stateIndex) => tickedLastFrame[stateIndex];

        public BTNode root { get; }

        public void Tick() {
            for (int i = 0; i < tickedThisFrame.Length; i++)
                tickedThisFrame[i] = false;

            command.Clear();
            root.Tick();

            (tickedThisFrame, tickedLastFrame) = (tickedLastFrame, tickedThisFrame);
        }

        public BehaviourTree(BTNode root) : this(root, 0) { }

        // starting depth can be non-zero for nested behaviour trees
        internal BehaviourTree(BTNode root, int startingDepth) {
            this.root = root;
            data = default;
            command = default;

            allNodes = new List<BTNode>();
            GatherNodeInfoRecursively(root, startingDepth);

            tickedLastFrame = new bool[allNodes.Count];
            tickedThisFrame = new bool[allNodes.Count];
            lastTickResult = new BTState[allNodes.Count];
            for (int i = 0; i < allNodes.Count; i++) {
                allNodes[i].index = i;
                allNodes[i].tree = this;
                lastTickResult[i] = BTState.Failure;
            }

            void GatherNodeInfoRecursively(BTNode childNode, int depth) {
                allNodes.Add(childNode);
                childNode.debugDepth = depth;

                if (childNode is INodeWithChildren branch) {
                    foreach (var child in branch.Children())
                        GatherNodeInfoRecursively(child, depth + 1);
                }
            }
        }

        public void Reset() {
            for (int i = 0; i < tickedLastFrame.Length; i++)
                tickedLastFrame[i] = false;

            foreach (var node in allNodes) {
                node.Reset();
            }
        }

        public void ShowAsString(IndentingStringBuilder sb) {
            root.ShowAsString((node, line) => AppendLine(sb, node, line), () => sb.Indents++, () => sb.Indents--);
        }

        private void AppendLine(IndentingStringBuilder sb, BTNode node, string line) {
            if (tickedLastFrame[node.index]) {
                if (node is ConditionalNode conditionalNode)
                    sb.Append($"<color={(conditionalNode.checkPassed ? "green" : "red")}>");
                else if (node is IfNodeSimple ifNode)
                    sb.Append($"<color={(ifNode.CheckPassed ? "green" : "red")}>");
                sb.Append("<b>");
            }

            sb.Append(line);
            if (tickedLastFrame[node.index]) {
                sb.Append("</b>");
                if (node is ConditionalNode || node is IfNodeSimple)
                    sb.Append("</color>");
            }

            sb.AppendLine();

            if (node is RunSubtree subtree && subtree.currentSubtree != null) {
                sb.Indents++;
                subtree.currentSubtree.ShowAsString(sb);
                sb.Indents--;
            }
        }
    }

    public class RepeatNode : Decorator {
        public int count = 1;
        int currentCount = 0;

        public RepeatNode(int count) {
            this.count = count;
        }

        protected override void OnStartedTicking() {
            currentCount = 0;
        }

        protected override BTState OnTick() {
            if (count > 0 && currentCount < count) {
                var result = child.Tick();
                switch (result) {
                    case BTState.Continue:
                        return BTState.Continue;
                    default:
                        currentCount++;
                        if (currentCount == count) {
                            currentCount = 0;
                            return BTState.Success;
                        }

                        return BTState.Continue;
                }
            }

            return BTState.Success;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Repeat {count} times (current count is {currentCount})");
            IncreaseIndentation();
            child.ShowAsString(AppendLine, IncreaseIndentation, DecreaseIndentation);
            DecreaseIndentation();
        }

        public override void Reset() {
            currentCount = 0;
        }
    }

    public class RandomSequenceNode : Branch {
        private readonly int[] weights;
        private int totalWeight;

        /// <summary>
        /// Will select one random child everytime it get triggered again
        /// </summary>
        /// <param name="weights">Leave null so that all child node have the same weight.
        /// If there is less weight than children, all subsequent child will have weight = 1</param>
        public RandomSequenceNode(int[] weights = null) {
            this.weights = weights;
        }

        protected override void OpenBranch(params BTNode[] children) {
            totalWeight = 0;

            for (int i = 0; i < children.Length; ++i) {
                int weight = 0;

                if (weights == null || weights.Length <= i) {
                    weight = 1;
                }
                else {
                    weight = weights[i];
                }

                totalWeight += weight;
            }

            base.OpenBranch(children);
        }

        protected override void OnStartedTicking() {
            PickNewChild();
        }

        protected override BTState OnTick() {
            var result = children[activeChild].Tick();

            switch (result) {
                case BTState.Continue:
                    return BTState.Continue;
                default:
                    PickNewChild();
                    return result;
            }
        }

        void PickNewChild() {
            int choice = Random.Range(0, totalWeight);

            for (int i = 0; i < weights.Length; i++) {
                if (choice <= weights[0]) {
                    activeChild = i;
                    return;
                }
            }

            throw new Exception();
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Random Sequence ({children.Count} children");
            IncreaseIndentation();
            foreach (var child in children)
                child.ShowAsString(AppendLine, IncreaseIndentation, DecreaseIndentation);
            DecreaseIndentation();
        }
    }

    public class WaitUntilNode : BTNode {
        private Func<bool> condition;

        public WaitUntilNode(Func<bool> condition) {
            this.condition = condition;
        }

        protected override BTState OnTick() {
            if (condition())
                return BTState.Success;
            return BTState.Continue;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Wait until {condition.Method.Name}");
        }

        public override void Reset() { }
    }

    /// <summary>
    /// Pause execution for a number of seconds.
    /// </summary>
    public class WaitNode : WaitRandomNode {
        public WaitNode(float seconds) : base(seconds, seconds) { }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            var timeLeft = Mathf.Max(0, finishedTime - Time.time);
            AppendLine(this, $"Wait {minSeconds} seconds ({timeLeft} seconds left)");
        }
    }

    public class WaitRandomNode : BTNode {
        protected readonly float minSeconds = 0;
        protected readonly float maxSeconds = 0;

        protected float finishedTime = -1;

        public WaitRandomNode(float minSeconds, float maxSeconds) {
            this.minSeconds = minSeconds;
            this.maxSeconds = maxSeconds;
        }

        protected override void OnStartedTicking() {
            finishedTime = Time.time + Random.Range(minSeconds, maxSeconds);
        }

        protected override BTState OnTick() {
            if (Time.time >= finishedTime) {
                finishedTime = -1;
                return BTState.Success;
            }
            else
                return BTState.Continue;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            var timeLeft = Mathf.Max(0, finishedTime - Time.time);
            AppendLine(this, $"Wait {minSeconds}-{maxSeconds} seconds ({timeLeft} seconds left)");
        }

        public override void Reset() {
            finishedTime = -1;
        }
    }

    public class PlaySoundNode : BTNode {
        private string sound;
        private GameObject parentGameObject;

        public PlaySoundNode(string sound, GameObject parentGameObject) {
            this.sound = sound;
            this.parentGameObject = parentGameObject;
        }

        protected override void OnStartedTicking() {
            Debug.Log("Don't know how sounds are played!");
        }

        protected override BTState OnTick() {
            return BTState.Success;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Play sound {sound}");
        }

        public override void Reset() { }
    }

    public class LogNode : BTNode {
        private Func<string> msg;

        public LogNode(Func<string> msg) {
            this.msg = msg;
        }

        protected override BTState OnTick() {
            Debug.Log(msg());
            return BTState.Success;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Log \"{msg()}\"");
        }

        public override void Reset() { }
    }

    public class RunSubtree : BTNode {
        private readonly Func<BTNode> fetchSubtree;

        internal BehaviourTree currentSubtree;
        private BTNode currentRootNode;
        private string name;

        public RunSubtree(string name, Func<BTNode> fetchSubtree) {
            this.name = name;
            this.fetchSubtree = fetchSubtree;
        }

        protected override void OnStartedTicking() {
            var rootNode = fetchSubtree();

            if (currentSubtree == null || rootNode != currentRootNode) {
                currentRootNode = rootNode;
                currentSubtree = new BehaviourTree(rootNode, debugDepth);
            }

            currentSubtree.Reset(); // This should probably be an option?
        }

        protected override BTState OnTick() {
            currentSubtree.Tick();
            return BTState.Continue;
        }

        public override void ShowAsString(Action<BTNode, string> AppendLine, Action IncreaseIndentation, Action DecreaseIndentation) {
            AppendLine(this, $"Run subtree \"{name}\"");
            // recursive handling is done by BehaviourTree.ShowAsString, since that has access to the string builder.
        }

        public override void Reset() {
            currentSubtree?.Reset();
        }
    }
}