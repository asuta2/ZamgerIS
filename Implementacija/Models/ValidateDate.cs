using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ooadproject.Models
{
    public class ValidateDate : ValidationAttribute
    {
        protected override ValidationResult IsValid
        (object date, ValidationContext validationContext)
        {
            return ((DateTime)date >= DateTime.Now &&
           (DateTime)date <= DateTime.Now.AddDays(14))
            ? ValidationResult.Success
            : new ValidationResult("Validan je upis između danas i 14 dana u budućnosti!");
        }
    }

}
