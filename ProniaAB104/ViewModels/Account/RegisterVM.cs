using System.ComponentModel.DataAnnotations;

namespace ProniaAB104.ViewModels
{
	public class RegisterVM
	{
		[Required]
		[MinLength(1)]
		[MaxLength(255)]
        public string Username { get; set; }
		[Required]
		[MinLength(3)]
		[MaxLength(50)]
        [RegularExpression(@"^[A-Za-z]+[A-Za-z\s]*$", ErrorMessage = "Invalid characters in the Name.")]
        public string Name { get; set; }
		[Required]
		[MinLength(8)]
		[MaxLength(255)]
        [RegularExpression(@"^[A-Za-z]+[A-Za-z\s]*$", ErrorMessage = "Invalid characters in the Name.")]
        public string Surname { get; set; }
		[Required]
		[MinLength(15)]
		[MaxLength(255)]
		[DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }
		[Required]
		[MinLength(8)]
		[MaxLength(25)]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		[Required]
		[MinLength(8)]
		[MaxLength(25)]
		[DataType(DataType.Password)]
		[Compare(nameof(Password))]
		public string ConfirmPassword { get; set; }
		public GenderType Gender { get; set; }
	}
	public enum GenderType
	{
		Male,
		Female
	}
}
