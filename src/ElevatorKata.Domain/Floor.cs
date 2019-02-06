using System;

namespace ElevatorKata.Domain
{
    public class Floor : IEquatable<Floor>
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

        public bool Equals(Floor other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Number == other.Number && string.Equals(Description, other.Description);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Floor)) return false;
            return Equals((Floor) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Number * 397) ^ (Description != null ? Description.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Floor left, Floor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Floor left, Floor right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{Number} - {Description}";
        }
     }
}
