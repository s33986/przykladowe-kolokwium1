using Microsoft.Data.SqlClient;
using przykladowe_kolokwium1.DTOs;
using przykladowe_kolokwium1.Exceptions;

namespace przykladowe_kolokwium1.Services;

public class RentalService(IConfiguration configuration) : IRentalService
{
    public async Task ReturnRentalAsync(int rentalId, ReturnRentalDTO dto, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await using var command = new SqlCommand();

        await connection.OpenAsync(cancellationToken);

        command.Connection = connection;

        command.CommandText = """
                              select status_id from rental where rental_id = @rentalId
                              """;

        command.Parameters.AddWithValue("@rentalId", rentalId);

        var statusId = await command.ExecuteScalarAsync(cancellationToken);

        if (statusId is null)
        {
            throw new NotFoundException($"Rental with id {rentalId} not found");
        }

        command.Parameters.Clear();

        command.CommandText = """
                              select status_id from Status where name = @statusName
                              """;
        command.Parameters.AddWithValue("@statusName", dto.statusName);

        var newStatus = await command.ExecuteScalarAsync(cancellationToken);

        if (newStatus is null)
        {
            throw new NotFoundException($"Status {dto.statusName} not found");
        }

        if (statusId.Equals(2))
        {
            throw new ItemAlreadyReturnedException($"Rental with id {rentalId} is already returned");
        }

        command.Parameters.Clear();

        command.CommandText = """
                              select rental_date from  rental where rental_id = @rentalId
                              """;
        command.Parameters.AddWithValue("@rentalId", rentalId);
        var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        await reader.ReadAsync(cancellationToken);

        var rentalDate = reader.GetDateTime(0);
        await reader.CloseAsync();

        if (rentalDate > dto.returnDate)
        {
            throw new InvalidDateException($"Return date cannot be before rental date: {rentalDate}");
        }

        command.Parameters.Clear();

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        command.Transaction = (SqlTransaction)transaction;

        try
        {
            command.CommandText = """
                                  update rental set return_date = @returnDate, status_id = @statusId where rental_id = @rentalId
                                  """;
            command.Parameters.AddWithValue("@returnDate", dto.returnDate);
            command.Parameters.AddWithValue("@statusId", newStatus);
            command.Parameters.AddWithValue("@rentalId", rentalId);

            await command.ExecuteNonQueryAsync(cancellationToken);

            command.Parameters.Clear();

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteRentalAsync(int rentalId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await using var command = new SqlCommand();

        await connection.OpenAsync(cancellationToken);

        command.Connection = connection;

        command.CommandText = """
                              select status_id from rental where rental_id = @rentalId
                              """;
        
        command.Parameters.AddWithValue("@rentalId", rentalId);
        
        var statusId = await command.ExecuteScalarAsync(cancellationToken);

        if (statusId is null)
        {
            throw new NotFoundException($"Rental with id {rentalId} not found");
        }

        if (statusId.Equals(2))
        {
            throw new ItemAlreadyReturnedException($"Rental with id {rentalId} is already correctly returned and cannot be deleted");
        }
        
        command.Parameters.Clear();
        
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        command.Transaction = (SqlTransaction)transaction;

        try
        {
            command.CommandText = """
                                  delete from rental_item where rental_id = @rentalId
                                  """;
            
            command.Parameters.AddWithValue("@rentalId", rentalId);
            
            await command.ExecuteNonQueryAsync(cancellationToken);
            
            command.CommandText = """
                                  delete from rental where rental_id = @rentalId
                                  """;
            command.Parameters.AddWithValue("@rentalId", rentalId);
            await command.ExecuteNonQueryAsync(cancellationToken);
            
            transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}