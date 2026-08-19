using CinemaReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static void SeedSeats(ApplicationDbContext context)
        {
            // Only generate seats if the theater is compeletly empty.
            if (!context.Seats.Any())
            {
                var seats = new List<Seat>();
                string[] rows = { "A", "B", "C", "D", "E", "F", };

                foreach (var row in rows)
                {
                    //generate 10 seate per row
                    for (int number = 1; number <= 10; number++)
                    {
                        seats.Add(new Seat()
                        {
                            Id = Guid.NewGuid(),
                            SeatRow = row,
                            SeatNumber = number,
                        });
                    }
                }

                //Add all 60 seate to the database
                context.Seats.AddRange(seats);
                context.SaveChanges();
            }
        }
    }
}
