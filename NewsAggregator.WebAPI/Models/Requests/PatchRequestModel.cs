using System.ComponentModel.DataAnnotations;

namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for update only necessary data.
    /// </summary>
    public class PatchRequestModel
    {
        /// <summary>
        /// Field name. Specify one of the allowed values: Title, Category, ShortDescription, ArticleText.
        /// </summary>
        [Required]
        public string? FieldName { get; set; }

        /// <summary>
        /// Field value.
        /// </summary>
        [Required]
        public string? FieldValue { get; set; }
    }
}