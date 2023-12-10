using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProniaAB104.Models
{
    public class Slide
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title mutleq daxil edilmelidir")]
        [MaxLength(25, ErrorMessage = "Title max uzunlugu: 25")]
        public string Title { get; set; }
        public string SubTitle { get; set; }
        [MinLength(15,ErrorMessage ="Description min uzunlugu: 15")]
        [MaxLength(250, ErrorMessage = "Title max uzunlugu: 250")]
        public string Description { get; set; }
        public string Image { get; set; }
        public int Order { get; set; }
        [NotMapped]
        public IFormFile? Photo { get; set; }
    }
}
