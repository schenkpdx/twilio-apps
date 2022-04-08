namespace Rezzy.Models
{
    public enum ReservationStatus : int
    {
        Unconfirmed = 0,
        Confirmed = 1,
        Canceled = 2,
        Arrived = 3,
        NoShow 
    }
}
