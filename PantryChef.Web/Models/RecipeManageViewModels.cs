using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PantryChef.Web.Models
{
    public class RecipeCreateViewModel
    {
        [Required(ErrorMessage = "Вкажіть назву страви.")]
        [StringLength(120, ErrorMessage = "Назва має містити не більше 120 символів.")]
        [Display(Name = "Назва")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть опис страви.")]
        [StringLength(1000, ErrorMessage = "Опис має містити не більше 1000 символів.")]
        [Display(Name = "Опис")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть категорію страви.")]
        [StringLength(100, ErrorMessage = "Категорія має містити не більше 100 символів.")]
        [Display(Name = "Категорія")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть посилання на фото.")]
        [StringLength(500, ErrorMessage = "Посилання на фото має містити не більше 500 символів.")]
        [Display(Name = "Фото (URL або шлях)")]
        public string Photo { get; set; } = string.Empty;

        [Range(0, 10000, ErrorMessage = "Калорії мають бути від 0 до 10000.")]
        [Display(Name = "Калорії")]
        public double Calories { get; set; }

        [Range(0, 1000, ErrorMessage = "Білки мають бути від 0 до 1000.")]
        [Display(Name = "Білки")]
        public double Proteins { get; set; }

        [Range(0, 1000, ErrorMessage = "Жири мають бути від 0 до 1000.")]
        [Display(Name = "Жири")]
        public double Fats { get; set; }

        [Range(0, 1000, ErrorMessage = "Вуглеводи мають бути від 0 до 1000.")]
        [Display(Name = "Вуглеводи")]
        public double Carbohydrates { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();
    }

    public class RecipeEditViewModel : RecipeCreateViewModel
    {
        [Required]
        public int Id { get; set; }
    }

    public class RecipeDeleteViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }
}
