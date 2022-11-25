using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NewsAggregatorAspNetCore.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        [Remote("CheckEmail", "Account", 
            HttpMethod = WebRequestMethods.Http.Post, 
            ErrorMessage = "Email is already exists")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [MaxLength(30)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [MinLength(8)]
        [MaxLength(30)]
        public string PasswordConfirmation { get; set; }
    }
}
