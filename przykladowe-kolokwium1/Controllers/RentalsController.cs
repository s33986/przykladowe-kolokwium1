using Microsoft.AspNetCore.Mvc;
using przykladowe_kolokwium1.DTOs;
using przykladowe_kolokwium1.Exceptions;
using przykladowe_kolokwium1.Services;

namespace przykladowe_kolokwium1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentalsController(IRentalService service) : ControllerBase
{

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ReturnRental([FromRoute] int id, [FromBody] ReturnRentalDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.ReturnRentalAsync(id, dto, cancellationToken);
            return NoContent();
        }
        catch (ItemAlreadyReturnedException e)
        {
            return Conflict(e.Message);
        } catch (NotFoundException e)
        {
            return NotFound(e.Message);
        } catch (InvalidDateException e)
        {
            return BadRequest(e.Message);
        }

        
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRental([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            await service.DeleteRentalAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ItemAlreadyReturnedException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return  NotFound(e.Message);
        }
    }
    
}