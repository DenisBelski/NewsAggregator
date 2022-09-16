
namespace NewsAggregator.DataBase.Entities
{
    public class Source
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public List <Article> Articles { get; set; }
        public SourceType SourceType { get; set; }
    }

}
