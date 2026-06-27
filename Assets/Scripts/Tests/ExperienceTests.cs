using NUnit.Framework;
using ToDonegeon.Gameplay;

namespace ToDonegeon.Tests
{
    public class ExperienceTests
    {
        [Test]
        public void Levels_Up_From_Total_Xp()
        {
            var xp = new Experience();
            Assert.That(xp.Level, Is.EqualTo(1));

            xp.Add(Experience.XpPerLevel); // one level
            Assert.That(xp.Total, Is.EqualTo(100));
            Assert.That(xp.Level, Is.EqualTo(2));

            xp.Add(-50); // ignored
            Assert.That(xp.Total, Is.EqualTo(100));
        }
    }
}
