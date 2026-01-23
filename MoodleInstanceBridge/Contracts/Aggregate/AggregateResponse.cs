using MoodleInstanceBridge.Contracts.Errors;

namespace MoodleInstanceBridge.Contracts.Aggregate
{
    public class AggregateResponse<T>
    {
        public List<AggregateResult<T>> Results { get; set; } = new();
        public List<InstanceError> Errors { get; set; } = new();
    }
}
