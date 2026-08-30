using CinemaReservation.Core.DTOs.Anlaytics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Interfaces
{
    public interface IAnalyticService
    {
        Task<IEnumerable<ShowtimeCapacityDto>> GetShowtimeCapacitiedAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<MovieRevenueDto>> GetMovieRevenuesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<TopCustomersDto>> GetTopcustomersbyCountAsync(int count);
        Task<IEnumerable<CancellationImpcatDto>> GetMostCanceledMovieAndLostRevenueServiceAsync();
    }
}
