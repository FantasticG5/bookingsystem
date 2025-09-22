namespace Infrastructure.DTOs;

// Event som publiceras när en bokning skapas
public class BookingCreatedEvent
{
    public int BookingId { get; set; }
    public int ClassId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Placeholder email - i verkligheten hämtas från user-tjänst
    public string UserEmail { get; set; } = "user@example.com";
}