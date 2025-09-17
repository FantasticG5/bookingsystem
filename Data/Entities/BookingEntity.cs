using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class BookingEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TrainingClassId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsCancelled { get; set; } = false;

}
