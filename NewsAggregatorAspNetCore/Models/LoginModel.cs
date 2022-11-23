using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NewsAggregatorAspNetCore.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        [Remote("CheckEmail", "Account", HttpMethod = WebRequestMethods.Http.Post, ErrorMessage = "Email is already exists")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}