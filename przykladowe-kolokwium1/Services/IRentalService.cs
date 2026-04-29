using przykladowe_kolokwium1.DTOs;

namespace przykladowe_kolokwium1.Services;

public interface IRentalService
{
    Task ReturnRentalAsync(int rentalId, ReturnRentalDTO dto, CancellationToken cancellationToken);
    Task DeleteRentalAsync(int rentalId, CancellationToken cancellationToken);
}