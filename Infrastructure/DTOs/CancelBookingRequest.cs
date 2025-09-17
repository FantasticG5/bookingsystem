namespace Infrastructure.DTOs;

// DTO (Data Transfer Object) för avboknings-API:et.
// Används för att ta emot input från frontend/användare.
public class CancelBookingRequest
{
    public int BookingId { get; set; }               // ID på bokningen som ska avbokas
    public string MemberEmail { get; set; } = "";    // Mailadress till medlemmen
    public string? MemberName { get; set; }          // (Valfritt) namn på medlemmen
}
