using System;
using System.Collections.Generic;
using NUnit.Framework;
using ToDonegeon.Analysis;

namespace ToDonegeon.Tests
{
    public class DifficultyProgressionTests
    {
        private static IReadOnlyList<DepthDifficulty> Run(int first, int last)
            => DifficultyProgression.Run(first, last, samplesPerDepth: 40, baseSeed: 0);

        [Test]
        public void One_Row_Per_Depth_In_Order()
        {
            IReadOnlyList<DepthDifficulty> rows = Run(1, 5);

            Assert.That(rows.Count, Is.EqualTo(5));
            for (int i = 0; i < rows.Count; i++)
                Assert.That(rows[i].Depth, Is.EqualTo(i + 1));
        }

        [Test]
        public void Mean_Threat_Rises_With_Depth()
        {
            IReadOnlyList<DepthDifficulty> rows = Run(1, 6);

            for (int i = 1; i < rows.Count; i++)
                Assert.That(rows[i].MeanThreat, Is.GreaterThan(rows[i - 1].MeanThreat));
        }

        [Test]
        public void Every_Depth_Is_Fully_Playable()
        {
            foreach (DepthDifficulty row in Run(1, 6))
                Assert.That(row.ValidRate, Is.EqualTo(1f));
        }

        [Test]
        public void Within_A_Depth_Threat_Stays_Tightly_Clustered()
        {
            foreach (DepthDifficulty row in Run(1, 6))
                Assert.That(row.ThreatSpread, Is.LessThan(0.5));
        }

        [Test]
        public void Result_Is_Deterministic()
        {
            IReadOnlyList<DepthDifficulty> a = Run(1, 4);
            IReadOnlyList<DepthDifficulty> b = Run(1, 4);

            for (int i = 0; i < a.Count; i++)
                Assert.That(a[i].MeanThreat, Is.EqualTo(b[i].MeanThreat));
        }

        [Test]
        public void Rejects_Bad_Ranges()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DifficultyProgression.Run(0, 4, 10, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => DifficultyProgression.Run(5, 4, 10, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => DifficultyProgression.Run(1, 4, 0, 0));
        }

        [Test]
        public void Markdown_Has_A_Table_And_Verdicts()
        {
            string markdown = DifficultyProgression.ToMarkdown(Run(1, 4));

            Assert.That(markdown, Does.Contain("# Difficulty balance and progression"));
            Assert.That(markdown, Does.Contain("| Depth | Map |"));
            Assert.That(markdown, Does.Contain("Progression"));
            Assert.That(markdown, Does.Contain("Balance"));
        }
    }
}
