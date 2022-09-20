﻿
namespace NewsAggregator.Core.DataTransferObjects
{
    public class ArticleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ArticleText { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Category { get; set; }
    }
}
