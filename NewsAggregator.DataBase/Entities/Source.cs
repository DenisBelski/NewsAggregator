using NewsAggregator.Core;

namespace NewsAggregator.DataBase.Entities
{
    public class Source : IBaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public SourceType SourceType { get; set; }
        public List<Article> Articles { get; set; }
    }
}
