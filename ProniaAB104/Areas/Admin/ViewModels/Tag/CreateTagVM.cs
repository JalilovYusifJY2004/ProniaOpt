using System.ComponentModel.DataAnnotations;

namespace ProniaAB104.Areas.Admin.ViewModels
{
    public class CreateTagVM
    {
        [MaxLength(10, ErrorMessage = "Max uzunluq 10 olmalidir")]
        public string Name { get; set; }
    }
}
