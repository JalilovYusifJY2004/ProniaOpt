using System.ComponentModel.DataAnnotations;

namespace ProniaAB104.Models
{
    public class Color
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Ad mutleq daxil edilmelidir")]
        [MaxLength(10, ErrorMessage = "Max uzunlugu 210 olmalidir")]
        public string Name { get; set; }
        public List<ProductColor>? ProductColors { get; set; }
    }
}
