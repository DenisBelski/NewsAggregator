using NewsAggregator.Core;

namespace NewsAggregatorAspNetCore.Models
{
    public class SourceModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public SourceType SourceType { get; set; }
    }
}
