using System.ComponentModel.DataAnnotations;

namespace ProniaAB104.Models
{
    public class Size
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Ad mutleq daxil edilmelidir")]
        [MaxLength(10, ErrorMessage = "Max uzunlugu 210 olmalidir")]
        public string Name { get; set; }
        public List<ProductSize>? ProductSizes { get; set; }
    }
}
