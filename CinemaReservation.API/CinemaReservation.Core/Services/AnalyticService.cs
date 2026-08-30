using CinemaReservation.Core.DTOs.Anlaytics;
using CinemaReservation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Services
{
    public class AnalyticService : IAnalyticService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }       

        public async Task<IEnumerable<MovieRevenueDto>> GetMovieRevenuesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                throw new ArgumentException("The starting date  cannot be later than the ending date.");

            return await _analyticsRepository.GetMovieRevenuesAsync(fromDate, toDate);
        }

        public async Task<IEnumerable<ShowtimeCapacityDto>> GetShowtimeCapacitiedAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                throw new ArgumentException("The starting date  cannot be later than the ending date.");

            return await _analyticsRepository.GetShowtimeCapacitiesAsync(fromDate, toDate);
        }

        public async Task<IEnumerable<TopCustomersDto>> GetTopcustomersbyCountAsync(int count)
        {
            if (count <= 0 || count > 100)
                throw new ArgumentException("the Count must be in 1-100 range.");

            return await _analyticsRepository.GetTopcuctomersAsync(count);            
        }

        public async Task<IEnumerable<CancellationImpcatDto>> GetMostCanceledMovieAndLostRevenueServiceAsync()
        {
            return await _analyticsRepository.GetMostCanceledMovieAndLostRevenueRepoAsync();
        }
    }
}
