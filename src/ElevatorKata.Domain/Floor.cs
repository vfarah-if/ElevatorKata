namespace ElevatorKata.Domain
{
    public class Floor
    {
        public Floor(int number, string description)
        {
            Number = number;
            Description = description;
        }

        public int Number { get; }
        public string Description { get; }

        public static Floor Create(int number, string description)
        {
            return new Floor(number, description);
        }

        public override string ToString()
        {
            return $"{Number} - {Description}";
        }
    }
}
