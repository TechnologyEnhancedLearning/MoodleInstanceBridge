namespace MoodleInstanceBridge.Contracts.Aggregate
{
    public class AggregateResult<T>
    {
        public string Instance { get; set; } = default!;
        public T Data { get; set; } = default!;
    }
}
