namespace Assets.Map {
    internal class Tile {
        public enum Type {
            Wall,
            Chamber,
            Corridor
        }

        public Type type { get; private set; }
        public bool isOccupied { get; private set; }

        public Tile(Type type, bool isOccupied) {
            this.type = type;
            this.isOccupied = isOccupied;
        }
    }
}