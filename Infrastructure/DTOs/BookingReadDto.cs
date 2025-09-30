using System;

namespace Infrastructure.DTOs;

public record BookingReadDto(
    int Id,
    int ClassId,
    DateTime CreatedAt,
    bool IsCancelled
);