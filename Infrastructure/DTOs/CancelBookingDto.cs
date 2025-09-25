using System;

namespace Infrastructure.DTOs;

public class CancelBookingDto
{
    public int ClassId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = default!;
}
