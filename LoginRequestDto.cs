using System.ComponentModel.DataAnnotations;

namespace Classes7.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string IndexNumber { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}