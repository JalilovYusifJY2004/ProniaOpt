using System.ComponentModel.DataAnnotations;

namespace ProniaAB104.ViewModels
{
    public class LoginVM
    {
        [Required]
        [MaxLength(50,ErrorMessage ="Uzunlugu 25-den cox olmamalidir!")]
        public string UsernameOrEmail { get; set; }
        [Required]
        [MinLength(8,ErrorMessage ="Uzunlugu 10-den az olmamalidir!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IsRemembered { get; set; }
    }
}
