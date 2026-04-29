using System.ComponentModel.DataAnnotations;

namespace przykladowe_kolokwium1.DTOs;

public class CreateRentalMovieDTO
{
    [Required, MaxLength(200)]
    public string title { get; set; } = string.Empty;
    [Required]
    public decimal rentalPrice { get; set; }
}