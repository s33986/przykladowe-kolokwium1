using przykladowe_kolokwium1.DTOs;

namespace przykladowe_kolokwium1.Services;

public interface ICustomerService
{
    Task<CustomerRentalsDTO> GetCustomerRentalsAsync(int customerId, CancellationToken cancellationToken);
}