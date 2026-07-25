using NUnit.Framework;
using ToDonegeon.Generation;
using ToDonegeon.Gameplay.AI;

namespace ToDonegeon.Tests
{
    public class LineOfSightTests
    {
        private const int Size = 20;
        private const int Range = 15;

        private static Grid OpenGrid() => new Grid(Size, Size, TileType.Floor);

        [Test]
        public void A_Clear_Line_Within_Range_Is_Visible()
        {
            Grid grid = OpenGrid();
            Assert.That(LineOfSight.IsVisible(grid, new Coord(2, 2), new Coord(9, 2), Range), Is.True);
            Assert.That(LineOfSight.IsVisible(grid, new Coord(2, 2), new Coord(9, 9), Range), Is.True);
        }

        [Test]
        public void A_Wall_On_The_Line_Blocks_Sight()
        {
            Grid grid = OpenGrid();
            grid[5, 2] = TileType.Wall;
            Assert.That(LineOfSight.IsVisible(grid, new Coord(2, 2), new Coord(9, 2), Range), Is.False);
        }

        [Test]
        public void Targets_Beyond_The_Range_Are_Not_Visible()
        {
            Grid grid = OpenGrid();
            Assert.That(LineOfSight.IsVisible(grid, new Coord(2, 2), new Coord(9, 2), range: 5), Is.False);
        }

        [Test]
        public void Adjacent_Cells_Are_Visible()
        {
            Grid grid = OpenGrid();
            Assert.That(LineOfSight.IsVisible(grid, new Coord(4, 4), new Coord(5, 4), Range), Is.True);
            Assert.That(LineOfSight.IsVisible(grid, new Coord(4, 4), new Coord(5, 5), Range), Is.True);
        }

        [Test]
        public void Visibility_Is_Symmetric()
        {
            Grid grid = OpenGrid();
            grid[6, 5] = TileType.Wall;

            var a = new Coord(3, 3);
            var b = new Coord(10, 7);
            Assert.That(LineOfSight.IsVisible(grid, a, b, Range), Is.EqualTo(LineOfSight.IsVisible(grid, b, a, Range)));

            var c = new Coord(2, 2);
            var d = new Coord(9, 9);
            Assert.That(LineOfSight.IsVisible(grid, c, d, Range), Is.EqualTo(LineOfSight.IsVisible(grid, d, c, Range)));
        }

        [Test]
        public void A_Null_Grid_Is_Never_Visible()
        {
            Assert.That(LineOfSight.IsVisible(null, new Coord(0, 0), new Coord(1, 1), Range), Is.False);
        }
    }
}
