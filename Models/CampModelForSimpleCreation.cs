using System;
using System.ComponentModel.DataAnnotations;

namespace AspRestApiWorkshop.Models
{
    public class CampModelForSimpleCreation
    {
        [Required(ErrorMessage = "Oh Man! Das braucht man doch...")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Moniker { get; set; }

        public DateTime EventDate { get; set; } = DateTime.MinValue;

        [Range(1, 100)]
        public int Length { get; set; } = 1;
    }
}
















