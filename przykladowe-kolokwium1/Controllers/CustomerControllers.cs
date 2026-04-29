using Microsoft.AspNetCore.Mvc;
using przykladowe_kolokwium1.Exceptions;
using przykladowe_kolokwium1.Services;

namespace przykladowe_kolokwium1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerControllers(ICustomerService service) : ControllerBase
{
    [HttpGet("{id:int}/rentals")]
    public async Task<IActionResult> GetCustomerRentals([FromRoute]int id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.GetCustomerRentalsAsync(id, cancellationToken));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}