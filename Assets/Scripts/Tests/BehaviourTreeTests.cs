using System;
using System.Collections.Generic;
using NUnit.Framework;
using ToDonegeon.Gameplay.AI;

namespace ToDonegeon.Tests
{
    public class BehaviourTreeTests
    {
        // a leaf that records that it ran and returns a fixed status
        private static BehaviourNode<List<int>> Leaf(int id, NodeStatus status)
            => new ActionNode<List<int>>(log => { log.Add(id); return status; });

        [Test]
        public void Selector_Returns_The_First_Non_Failure_And_Skips_The_Rest()
        {
            var log = new List<int>();
            NodeStatus status = new Selector<List<int>>(
                Leaf(1, NodeStatus.Failure),
                Leaf(2, NodeStatus.Success),
                Leaf(3, NodeStatus.Success)).Tick(log);

            Assert.That(status, Is.EqualTo(NodeStatus.Success));
            Assert.That(log, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Selector_Fails_When_Every_Child_Fails()
        {
            var log = new List<int>();
            NodeStatus status = new Selector<List<int>>(
                Leaf(1, NodeStatus.Failure),
                Leaf(2, NodeStatus.Failure)).Tick(log);

            Assert.That(status, Is.EqualTo(NodeStatus.Failure));
            Assert.That(log, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Sequence_Stops_At_The_First_Non_Success()
        {
            var log = new List<int>();
            NodeStatus status = new Sequence<List<int>>(
                Leaf(1, NodeStatus.Success),
                Leaf(2, NodeStatus.Failure),
                Leaf(3, NodeStatus.Success)).Tick(log);

            Assert.That(status, Is.EqualTo(NodeStatus.Failure));
            Assert.That(log, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Sequence_Succeeds_When_Every_Child_Succeeds()
        {
            var log = new List<int>();
            NodeStatus status = new Sequence<List<int>>(
                Leaf(1, NodeStatus.Success),
                Leaf(2, NodeStatus.Success)).Tick(log);

            Assert.That(status, Is.EqualTo(NodeStatus.Success));
            Assert.That(log, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Running_Short_Circuits_A_Selector()
        {
            var log = new List<int>();
            NodeStatus status = new Selector<List<int>>(
                Leaf(1, NodeStatus.Failure),
                Leaf(2, NodeStatus.Running),
                Leaf(3, NodeStatus.Success)).Tick(log);

            Assert.That(status, Is.EqualTo(NodeStatus.Running));
            Assert.That(log, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Condition_Maps_The_Predicate_To_Success_Or_Failure()
        {
            var positive = new Condition<int>(value => value > 0);
            Assert.That(positive.Tick(5), Is.EqualTo(NodeStatus.Success));
            Assert.That(positive.Tick(-3), Is.EqualTo(NodeStatus.Failure));
        }

        [Test]
        public void Nodes_Reject_Null_Arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new Selector<int>(null));
            Assert.Throws<ArgumentNullException>(() => new Sequence<int>(null));
            Assert.Throws<ArgumentNullException>(() => new Condition<int>(null));
            Assert.Throws<ArgumentNullException>(() => new ActionNode<int>(null));
        }
    }
}
