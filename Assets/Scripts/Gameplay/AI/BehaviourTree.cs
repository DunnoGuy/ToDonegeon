using System;

namespace ToDonegeon.Gameplay.AI
{
    public enum NodeStatus { Success, Failure, Running }

    public abstract class BehaviourNode<TContext>
    {
        public abstract NodeStatus Tick(TContext context);
    }

    // Ticks children in order and stops at the first that does not fail (a logical OR)
    public sealed class Selector<TContext> : BehaviourNode<TContext>
    {
        private readonly BehaviourNode<TContext>[] _children;

        public Selector(params BehaviourNode<TContext>[] children)
        {
            _children = children ?? throw new ArgumentNullException(nameof(children));
        }

        public override NodeStatus Tick(TContext context)
        {
            foreach (BehaviourNode<TContext> child in _children)
            {
                NodeStatus status = child.Tick(context);
                if (status != NodeStatus.Failure) return status;
            }
            return NodeStatus.Failure;
        }
    }

    // ticks children in order and stops at the first that does not succeed (a logical AND)
    public sealed class Sequence<TContext> : BehaviourNode<TContext>
    {
        private readonly BehaviourNode<TContext>[] _children;

        public Sequence(params BehaviourNode<TContext>[] children)
        {
            _children = children ?? throw new ArgumentNullException(nameof(children));
        }

        public override NodeStatus Tick(TContext context)
        {
            foreach (BehaviourNode<TContext> child in _children)
            {
                NodeStatus status = child.Tick(context);
                if (status != NodeStatus.Success) return status;
            }
            return NodeStatus.Success;
        }
    }

    // succeeds while its predicate holds, fails otherwise
    public sealed class Condition<TContext> : BehaviourNode<TContext>
    {
        private readonly Func<TContext, bool> _predicate;

        public Condition(Func<TContext, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public override NodeStatus Tick(TContext context)
            => _predicate(context) ? NodeStatus.Success : NodeStatus.Failure;
    }

    // does some work and reports its own status
    public sealed class ActionNode<TContext> : BehaviourNode<TContext>
    {
        private readonly Func<TContext, NodeStatus> _action;

        public ActionNode(Func<TContext, NodeStatus> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public override NodeStatus Tick(TContext context) => _action(context);
    }
}
