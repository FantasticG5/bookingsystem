using System;

namespace Data.Entities;

public class Booking // Domänmodell för en bokning
    {
        public int Id { get; set; } // Primärnyckel för bokningen
        public int ClassId { get; set; } // FK till TrainingClass
        public int UserId { get; set; } // Identifierar medlemmen som bokar
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // När bokningen skapades
        public bool IsCancelled { get; set; } // Flagga för om bokningen är avbokad
    }
