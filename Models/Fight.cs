using System.ComponentModel.DataAnnotations;

namespace UFCApp.Models
{
    public class Fight
    {
        public int Id { get; set; }
        public int EventId { get; set; }

        [Required(ErrorMessage = "Fighter 1 Name is required")]
        public String Fighter1Name { get; set; }

        public Decimal Fighter1Points { get; set; }

        [Required(ErrorMessage = "Fighter 2 Name is required")]
        public String Fighter2Name { get; set; }

        public Decimal Fighter2Points { get; set; }

        public String? Winner { get; set; }

        public Fight()
        {
            Fighter1Name = String.Empty;
            Fighter2Name = String.Empty;
        }
    }
}
