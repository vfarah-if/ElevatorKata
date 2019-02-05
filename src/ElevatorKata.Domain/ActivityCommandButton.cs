using System;

namespace ElevatorKata.Domain
{
    public class ActivityCommandButton : IActivityState
    {
        private readonly Action action;
        public EventHandler Changed;
        public ActivityCommandButton(string description, bool isActive = false, bool isEnabled = true, Action action = null)
        {
            this.action = action;
            Description = description;
            IsActive = isActive;
            IsEnabled = isEnabled;
        }

        public string Description { get; }
        public bool IsActive { get; private set; }
        public bool IsEnabled { get; }
        public bool CanExecute => !IsActive && IsEnabled && action != null;

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

        public bool Execute()
        {
            if (!CanExecute) return false;
            Activate();
            action?.Invoke();
            return true;
        }

        public override string ToString()
        {
            return $"'{Description ?? "Unknown"}' activity set to {IsActive}";
        }

        public static ActivityCommandButton Create(string description, bool isActive = false, bool isEnabled = true, Action action = null)
        {
            return new ActivityCommandButton(description, isActive, isEnabled, action);
        }

        private void OnChanged()
        {
            var handler = Changed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
