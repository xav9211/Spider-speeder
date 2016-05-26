namespace Assets.Scripts.Map {
    internal class Tile {
        public enum Type {
            Wall,
            Chamber,
            Corridor,
            Exit
        }

        public Type type { get; private set; }
        public bool isOccupied { get; private set; }

        public Tile(Type type, bool isOccupied) {
            this.type = type;
            this.isOccupied = isOccupied;
        }
    }
}