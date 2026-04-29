using Microsoft.Data.SqlClient;
using przykladowe_kolokwium1.DTOs;
using przykladowe_kolokwium1.Exceptions;

namespace przykladowe_kolokwium1.Services;

public class CustomerService(IConfiguration configuration) : ICustomerService
{
    

    public async Task<CustomerRentalsDTO> GetCustomerRentalsAsync(int customerId, CancellationToken cancellationToken)
    {
        CustomerRentalsDTO? dto = null;
        Dictionary<int, RentalDTO> rentals = [];

        await using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await using var command = new SqlCommand();
        
        await connection.OpenAsync(cancellationToken);
        
        command.Connection = connection;

        command.CommandText = """
                              select c.first_name, c.last_name, r.rental_id, r.rental_date, r.return_date, s.name, m.title, ri.price_at_rental
                              from Customer c
                              left join Rental r on c.customer_id = r.customer_id
                              left join Status s on s.status_id = r.status_id
                              left join Rental_Item ri on ri.rental_id = r.rental_id
                              left join Movie m on m.movie_id = ri.movie_id
                              where c.customer_id = @id
                              """;
        command.Parameters.AddWithValue("@id", customerId);


        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            dto ??= new CustomerRentalsDTO
            {
                firstName = reader.GetString(0),
                lastName = reader.GetString(1),
                rentals = []
            };

            if (reader.IsDBNull(2)) continue;


            var rentalId = reader.GetInt32(2);


            if (!rentals.ContainsKey(rentalId))
            {
                rentals.Add(rentalId, new RentalDTO
                {
                    id = rentalId,
                    rentalDate = reader.GetDateTime(3),
                    returnDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    status = reader.GetString(5),
                    movies = []
                });
            }

            var rentalMovies = rentals[rentalId].movies.ToList();

            rentalMovies.Add(new MovieDTO
            {
                title = reader.GetString(6),
                priceAtRental = reader.GetDecimal(7),
            });

            rentals[rentalId].movies = rentalMovies;
        }

        if (dto is null)
        {
            throw new NotFoundException($"Customer with id {customerId} not found");
        }

        dto.rentals = rentals.Values;

        return dto;
    }
}