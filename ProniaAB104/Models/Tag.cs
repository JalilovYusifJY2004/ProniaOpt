using System.ComponentModel.DataAnnotations;

namespace ProniaAB104.Models
{
    public class Tag
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "Ad  daxil edilmelidir!")]
        [MaxLength(10, ErrorMessage = "Uzunlugu 15 xarakterden cox olmamalidir.")]
        public string Name { get; set; }
        public List<ProductTag>? ProductTags { get; set; }
    }
}
