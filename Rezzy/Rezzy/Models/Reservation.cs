using System.ComponentModel.DataAnnotations;

namespace Rezzy.Models
{
    public class Reservation
    {
        public int ReservationID { get; set; }
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
        [Display(Name = "Date/Time")]
        public DateTime DateTime { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        [Display(Name = "Party Size")]
        public int PartySize { get; set; }
        public string? Comments { get; set; }
        public ReservationStatus Status { get; set; }

    }
}
