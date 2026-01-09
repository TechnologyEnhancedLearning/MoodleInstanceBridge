namespace MoodleInstanceBridge.Models.Configuration
{
    /// <summary>
    /// Validation result for instance configuration
    /// </summary>
    public class ConfigurationValidationResult
    {
        private readonly List<string> _errors = new();

        public bool IsValid { get; private set; }
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        public string? InstanceShortName { get; set; }

        public ConfigurationValidationResult(bool isValid)
        {
            IsValid = isValid;
        }

        public void AddError(string error)
        {
            _errors.Add(error);
            IsValid = false;
        }
    }
}
