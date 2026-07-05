using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class RoomTests
    {
        private static BspNode Leaf(GridRect region) => new BspNode(region);

        [Test]
        public void Center_Falls_Back_To_The_Bounds_Middle()
        {
            var bounds = new GridRect(4, 6, 8, 10);
            var room = new Room(bounds, Leaf(bounds));
            Assert.That(room.Center, Is.EqualTo(bounds.Center));
        }

        [Test]
        public void Explicit_Anchor_Overrides_The_Bounds_Middle()
        {
            var bounds = new GridRect(4, 6, 8, 10);
            var anchor = new Coord(5, 7);
            var room = new Room(bounds, Leaf(bounds), anchor);
            Assert.That(room.Center, Is.EqualTo(anchor));
            Assert.That(room.Center, Is.Not.EqualTo(bounds.Center));
        }
    }
}
