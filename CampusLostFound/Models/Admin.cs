
using System.ComponentModel.DataAnnotations;

namespace CampusLostFound.Models
{
    public class Admin
    {
        public int AdminId { get; set; }

        [Required(ErrorMessage = "Please enter your username.")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; }

        public string PasswordHash { get; set; }
    }
}
