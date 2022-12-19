using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NewsAggregatorAspNetCore.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [Remote("CheckPassword", "Account",
            HttpMethod = WebRequestMethods.Http.Post,
            AdditionalFields = nameof(Email),
            ErrorMessage = "Password is incorrect.")]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [MaxLength(30)]
        public string Password { get; set; }
    }
}