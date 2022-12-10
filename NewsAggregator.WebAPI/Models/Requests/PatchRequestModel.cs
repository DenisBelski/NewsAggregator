using System.ComponentModel.DataAnnotations;

namespace NewsAggregator.WebAPI.Models.Requests
{
    /// <summary>
    /// Request model for update only necessary data.
    /// </summary>
    public class PatchRequestModel
    {
        /// <summary>
        /// Field name.
        /// </summary>
        public string? FieldName { get; set; }

        /// <summary>
        /// Field value.
        /// </summary>
        public string? FieldValue { get; set; }
    }
}