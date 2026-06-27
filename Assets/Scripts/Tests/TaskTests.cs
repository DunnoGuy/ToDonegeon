using System;
using NUnit.Framework;
using ToDonegeon.Tasks;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class TaskListTests
    {
        [Test]
        public void Add_Appends_With_Id_And_Reward()
        {
            var list = new TaskList();

            TaskItem task = list.Add("Write report", TaskDifficulty.Hard);

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(task.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(task.XpReward, Is.EqualTo(TaskRewards.HardXp));
            Assert.That(task.IsDone, Is.False);
        }

        [Test]
        public void Complete_Awards_Xp_Once()
        {
            var list = new TaskList();
            TaskItem task = list.Add("Workout", TaskDifficulty.Medium);

            Assert.That(list.Complete(task.Id), Is.EqualTo(TaskRewards.MediumXp));
            Assert.That(task.IsDone, Is.True);
            Assert.That(list.Complete(task.Id), Is.EqualTo(0), "no double XP from re-completing");
        }

        [Test]
        public void Complete_Unknown_Task_Awards_Nothing()
        {
            Assert.That(new TaskList().Complete(Guid.NewGuid()), Is.EqualTo(0));
        }

        [Test]
        public void Edit_Changes_Text_And_Blank_Title_Falls_Back()
        {
            var task = new TaskItem(Guid.NewGuid(), "   ", TaskDifficulty.Easy);
            Assert.That(task.Title, Is.EqualTo("Untitled task"));

            task.Edit("Buy milk", "semi-skimmed");

            Assert.That(task.Title, Is.EqualTo("Buy milk"));
            Assert.That(task.Description, Is.EqualTo("semi-skimmed"));
        }

        [Test]
        public void Move_Reorders_By_Id()
        {
            var list = new TaskList();
            list.Add("A", TaskDifficulty.Easy);
            list.Add("B", TaskDifficulty.Easy);
            TaskItem c = list.Add("C", TaskDifficulty.Easy);

            list.Move(c.Id, 0);

            Assert.That(list.Items[0].Id, Is.EqualTo(c.Id));
        }

        [Test]
        public void Remove_Drops_The_Task()
        {
            var list = new TaskList();
            TaskItem task = list.Add("X", TaskDifficulty.Easy);

            Assert.That(list.Remove(task.Id), Is.True);
            Assert.That(list.Count, Is.EqualTo(0));
        }
    }

    public class PlayerProgressionTests
    {
        [Test]
        public void Player_Gains_Experience_From_Completing_A_Task()
        {
            var player = new PlayerCharacter(30, 5);
            var list = new TaskList();
            TaskItem task = list.Add("Submit assignment", TaskDifficulty.Epic);

            player.GainExperience(list.Complete(task.Id));

            Assert.That(player.Experience.Total, Is.EqualTo(TaskRewards.EpicXp));
            Assert.That(player.Experience.Level, Is.EqualTo(2));
        }

        [Test]
        public void Levelling_Up_Grows_Attack_And_Heals()
        {
            var player = new PlayerCharacter(30, 5);
            player.Health.TakeDamage(20); // 10 / 30

            int gained = player.GainExperience(TaskRewards.EpicXp); // 100 -> level 2

            Assert.That(gained, Is.EqualTo(1));
            Assert.That(player.AttackPower, Is.EqualTo(6), "+1 attack per level");
            Assert.That(player.Health.Current, Is.EqualTo(player.Health.Max), "healed on level up");
        }

        [Test]
        public void Crossing_Several_Levels_Applies_Each()
        {
            var player = new PlayerCharacter(30, 5);

            int gained = player.GainExperience(250); // level 1 -> 3

            Assert.That(player.Experience.Level, Is.EqualTo(3));
            Assert.That(gained, Is.EqualTo(2));
            Assert.That(player.AttackPower, Is.EqualTo(7), "+1 per level crossed");
        }
    }
}
