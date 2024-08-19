using System.ComponentModel.DataAnnotations;

namespace UFCApp.Models
{
    public class Event
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Event Name is required")] 
        public string Name { get; set; }

        [Required(ErrorMessage = "Event Date and Time are required")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date format")]
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
        public List<Fight> Fights { get; set; }
        public Event()
        {
            Fights = new List<Fight>(); 
        }
    }
}
