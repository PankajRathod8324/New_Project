using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModel
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }
}
