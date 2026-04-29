using System.ComponentModel.DataAnnotations;

namespace przykladowe_kolokwium1.DTOs;

public class CreateRentalDTO
{
    [Required]
    public DateTime rentalDate { get; set; }
    [Required, MinLength(1)]
    public IEnumerable<CreateRentalMovieDTO> movies { get; set; } = [];
}