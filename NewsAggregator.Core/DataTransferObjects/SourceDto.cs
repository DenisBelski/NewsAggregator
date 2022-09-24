namespace NewsAggregator.Core.DataTransferObjects
{
    public class SourceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public SourceType SourceType { get; set; }
    }
}
