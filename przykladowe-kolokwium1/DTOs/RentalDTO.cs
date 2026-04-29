namespace przykladowe_kolokwium1.DTOs;

public class RentalDTO
{
    public int id { get; set; }
    public DateTime rentalDate { get; set; }
    public DateTime? returnDate { get; set; }
    public string status { get; set; } = string.Empty;
    public IEnumerable<MovieDTO> movies { get; set; } = [];
}