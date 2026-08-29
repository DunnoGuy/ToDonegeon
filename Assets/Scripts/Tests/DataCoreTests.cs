using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    // These run without entering play mode and exist both to lock the behaviour the later
    // pipeline stages depend on and to prove the engine-agnostic assembly compiles and is
    // reachable from a test assembly.
    public class CoordTests
    {
        [Test]
        public void Addition_And_Subtraction_Are_Componentwise()
        {
            var a = new Coord(2, 3);
            var b = new Coord(-1, 5);

            Assert.That(a + b, Is.EqualTo(new Coord(1, 8)));
            Assert.That(a - b, Is.EqualTo(new Coord(3, -2)));
        }

        [Test]
        public void Equality_And_HashCode_Agree()
        {
            var a = new Coord(7, 9);
            var b = new Coord(7, 9);
            var c = new Coord(9, 7);

            Assert.That(a == b, Is.True);
            Assert.That(a != c, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Orthogonal_Offsets_Are_The_Four_Neighbours()
        {
            Assert.That(Coord.Orthogonal.Count, Is.EqualTo(4));
            Assert.That(Coord.Orthogonal, Does.Contain(Coord.Up));
            Assert.That(Coord.Orthogonal, Does.Contain(Coord.Down));
            Assert.That(Coord.Orthogonal, Does.Contain(Coord.Left));
            Assert.That(Coord.Orthogonal, Does.Contain(Coord.Right));
        }

        [Test]
        public void ManhattanDistance_Sums_Axis_Deltas()
        {
            Assert.That(Coord.ManhattanDistance(new Coord(1, 1), new Coord(4, 5)), Is.EqualTo(7));
            Assert.That(Coord.ManhattanDistance(Coord.Zero, Coord.Zero), Is.EqualTo(0));
        }
    }

    public class GridRectTests
    {
        [Test]
        public void Bounds_Area_And_Center_Are_Consistent()
        {
            var rect = new GridRect(2, 3, 4, 6);

            Assert.That(rect.MaxX, Is.EqualTo(6));
            Assert.That(rect.MaxY, Is.EqualTo(9));
            Assert.That(rect.Area, Is.EqualTo(24));
            Assert.That(rect.Center, Is.EqualTo(new Coord(4, 6)));
        }

        [Test]
        public void Center_Stays_Inside_For_Small_And_Even_Rects()
        {
            var even = new GridRect(0, 0, 2, 2);
            var single = new GridRect(5, 5, 1, 1);

            Assert.That(even.Contains(even.Center), Is.True);
            Assert.That(single.Contains(single.Center), Is.True);
        }

        [Test]
        public void Contains_Respects_The_HalfOpen_Range()
        {
            var rect = new GridRect(0, 0, 3, 3);

            Assert.That(rect.Contains(new Coord(0, 0)), Is.True);
            Assert.That(rect.Contains(new Coord(2, 2)), Is.True);
            Assert.That(rect.Contains(new Coord(3, 0)), Is.False, "MaxX is exclusive");
            Assert.That(rect.Contains(new Coord(0, 3)), Is.False, "MaxY is exclusive");
        }

        [Test]
        public void Overlaps_Is_True_When_Sharing_Cells_And_False_When_Merely_Touching()
        {
            var a = new GridRect(0, 0, 3, 3);
            var overlapping = new GridRect(2, 2, 3, 3);
            var adjacent = new GridRect(3, 0, 3, 3); // shares an edge but no cell

            Assert.That(a.Overlaps(overlapping), Is.True);
            Assert.That(a.Overlaps(adjacent), Is.False);
        }
    }

    public class TileTypeTests
    {
        [Test]
        public void Wall_Is_Not_Walkable()
        {
            Assert.That(TileType.Wall.IsWalkable(), Is.False);
        }

        [Test]
        public void Floor_Corridor_Entrance_And_Exit_Are_Walkable()
        {
            Assert.That(TileType.Floor.IsWalkable(), Is.True);
            Assert.That(TileType.Corridor.IsWalkable(), Is.True);
            Assert.That(TileType.Entrance.IsWalkable(), Is.True);
            Assert.That(TileType.Exit.IsWalkable(), Is.True);
        }
    }

    public class GridTests
    {
        [Test]
        public void New_Grid_Has_Given_Size_And_Is_All_Wall()
        {
            var grid = new Grid(8, 5);

            Assert.That(grid.Width, Is.EqualTo(8));
            Assert.That(grid.Height, Is.EqualTo(5));
            Assert.That(grid.CellCount, Is.EqualTo(40));
            Assert.That(grid.Count(TileType.Wall), Is.EqualTo(40));
        }

        [Test]
        public void Indexers_RoundTrip_By_Coord_And_By_Pair()
        {
            var grid = new Grid(4, 4);

            grid[1, 2] = TileType.Floor;
            grid[new Coord(3, 0)] = TileType.Corridor;

            Assert.That(grid[1, 2], Is.EqualTo(TileType.Floor));
            Assert.That(grid[new Coord(1, 2)], Is.EqualTo(TileType.Floor));
            Assert.That(grid[3, 0], Is.EqualTo(TileType.Corridor));
        }

        [Test]
        public void InBounds_Is_True_Inside_And_False_Outside()
        {
            var grid = new Grid(3, 3);

            Assert.That(grid.InBounds(0, 0), Is.True);
            Assert.That(grid.InBounds(2, 2), Is.True);
            Assert.That(grid.InBounds(3, 2), Is.False);
            Assert.That(grid.InBounds(new Coord(-1, 0)), Is.False);
        }

        [Test]
        public void Indexer_Throws_Outside_The_Grid()
        {
            var grid = new Grid(3, 3);

            Assert.Throws<System.IndexOutOfRangeException>(() => { var _ = grid[3, 0]; });
            Assert.Throws<System.IndexOutOfRangeException>(() => grid[0, -1] = TileType.Floor);
        }

        [Test]
        public void IsWalkable_Reads_The_Cell_And_Is_Safe_Out_Of_Bounds()
        {
            var grid = new Grid(3, 3);
            grid[1, 1] = TileType.Floor;

            Assert.That(grid.IsWalkable(new Coord(1, 1)), Is.True);
            Assert.That(grid.IsWalkable(new Coord(0, 0)), Is.False, "still a wall");
            Assert.That(grid.IsWalkable(new Coord(99, 99)), Is.False, "out of bounds is not walkable");
        }

        [Test]
        public void Fill_Rect_Carves_Only_That_Region()
        {
            var grid = new Grid(6, 6);

            grid.Fill(new GridRect(1, 1, 3, 2), TileType.Floor);

            Assert.That(grid.Count(TileType.Floor), Is.EqualTo(6));
            Assert.That(grid[1, 1], Is.EqualTo(TileType.Floor));
            Assert.That(grid[3, 2], Is.EqualTo(TileType.Floor));
            Assert.That(grid[0, 0], Is.EqualTo(TileType.Wall), "outside the rect is untouched");
            Assert.That(grid[4, 1], Is.EqualTo(TileType.Wall), "MaxX is exclusive");
        }

        [Test]
        public void Fill_Rect_Clamps_To_The_Grid()
        {
            var grid = new Grid(4, 4);

            // Rectangle pokes past the top-right corner; only the in-bounds part is written.
            grid.Fill(new GridRect(2, 2, 10, 10), TileType.Floor);

            Assert.That(grid.Count(TileType.Floor), Is.EqualTo(4), "only the 2x2 corner is in bounds");
            Assert.That(grid[3, 3], Is.EqualTo(TileType.Floor));
        }

        [Test]
        public void Fill_Type_Sets_Every_Cell()
        {
            var grid = new Grid(5, 5);

            grid.Fill(TileType.Floor);

            Assert.That(grid.Count(TileType.Floor), Is.EqualTo(25));
            Assert.That(grid.Count(TileType.Wall), Is.EqualTo(0));
        }
    }
}
