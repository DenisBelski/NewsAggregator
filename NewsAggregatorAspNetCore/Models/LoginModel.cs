using System.ComponentModel.DataAnnotations;

namespace NewsAggregatorAspNetCore.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [MaxLength(30)]
        public string Password { get; set; }
    }
}