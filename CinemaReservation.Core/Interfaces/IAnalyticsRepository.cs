using CinemaReservation.Core.DTOs.Anlaytics;
using CinemaReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IAnalyticsRepository
    {
        // Fetches capacity for all showtimes optionally filtered by by a date rang
        Task<IEnumerable<ShowtimeCapacityDto>> GetShowtimeCapacitiesAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // Calulates Revenu per movie, optionally filterd by Date
        Task<IEnumerable<MovieRevenueDto>> GetMovieRevenuesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<TopCustomersDto>> GetTopcuctomersAsync(int count);
        Task<IEnumerable<CancellationImpcatDto>> GetMostCanceledMovieAndLostRevenueRepoAsync();
    }
}
