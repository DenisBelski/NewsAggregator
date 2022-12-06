using System.ComponentModel.DataAnnotations;

namespace NewsAggregatorAspNetCore.Models
{
    public class ArticleCreationModel
    {
        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Category { get; set; }
        
        [Required]
        public string? ShortDescription { get; set; }
        
        [Required]
        public string? ArticleText { get; set; }
    }
}
