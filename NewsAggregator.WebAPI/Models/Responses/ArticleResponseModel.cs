namespace NewsAggregator.WebAPI.Models.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public class ArticleResponseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? ShortDescrtiption { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime PublicationDate { get; set; }
    }
}