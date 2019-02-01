using System;

namespace ElevatorKata.Domain
{
    public class ActivityButton : IActivityState
    {
        public EventHandler Changed;
        public ActivityButton(string description, bool isActive = false)
        {
            Description = description;
            IsActive = isActive;
        }

        public string Description { get; }
        public bool IsActive { get; private set; }

        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                OnChanged();
            }
        }

        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                OnChanged();
            }
        }

        public override string ToString()
        {
            return $"'{Description ?? "Unknown"}' activity set to {IsActive}";
        }

        public static ActivityButton Create(string description, bool isActive = false)
        {
            return new ActivityButton(description, isActive);
        }

        private void OnChanged()
        {
            var handler = Changed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
