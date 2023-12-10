using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;

namespace ProniaAB104.Areas.Admin.ViewModels
{
    public class CreateCategoryVM
    {
        [MaxLength(15, ErrorMessage = "Max uzunluq 15 ")]
        public string Name { get; set; }
    }
}
