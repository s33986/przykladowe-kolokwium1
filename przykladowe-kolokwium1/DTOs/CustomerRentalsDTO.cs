namespace przykladowe_kolokwium1.DTOs;

public class CustomerRentalsDTO
{
    public string firstName { get; set; } =  string.Empty;
    public string lastName { get; set; } =  string.Empty;
    public IEnumerable<RentalDTO> rentals { get; set; } = [];
}