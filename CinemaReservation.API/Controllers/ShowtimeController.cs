using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Core.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowtimeController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IReservartionService _reservationService;
        private readonly IValidator<CreateShowtimeDto> _showtimeValidator;

        public ShowtimeController(IMovieService movieService, IReservartionService reservationService, IValidator<CreateShowtimeDto> showtimeValidator)
        {
            _movieService = movieService;
            _reservationService = reservationService;
            _showtimeValidator = showtimeValidator;
        }

        [HttpGet("{showtimeId}/seats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSeats(Guid showtimeId)
        {
            var seats = await _reservationService.GetSeatAvailabilityAsync(showtimeId);
            if (seats.Count == 0)            
                return NotFound(new { Message = "No Seats found for this showtime" });
            
            return Ok(seats);
        }

        [HttpGet("{movieId}/showtimes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetShowtimesByIdAsync(Guid movieId)
        {            
            var showtimes =await _movieService.GetShowtimesByMovieIdAsync(movieId);
            return Ok(showtimes);            
        }

        [HttpGet("{movieId}/showtime/{showtimeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetShowtimeById(Guid movieId,Guid showtimeId)
        {            
            var showtime = await _movieService.GetShowtimeResponseDtoAsync(movieId, showtimeId);
            return Ok(showtime);                                           
        }

        [HttpPost("{movieId}/showtimes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddShowtime(Guid movieId, [FromBody] CreateShowtimeDto request)
        {
            var validationResult = await _showtimeValidator.ValidateAsync(request);

            if (!validationResult.IsValid)            
                return BadRequest(validationResult.Errors);
            

            var showtime = await _movieService.AddShowTimeAsync(movieId, request);
            return Ok(new
            {
                Message = "showtime added successfully.",
                ShowtimeId = showtime.Id,
                showtime.MovieId,
                showtime.StartTime
            });
        }

        [HttpPut("{movieId}/showtimes/{showtimeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateShowtime(Guid movieId, Guid showtimeId, [FromBody] CreateShowtimeDto request)
        {
            var vadlidationResult = await _showtimeValidator.ValidateAsync(request);

            if (!vadlidationResult.IsValid)            
                return BadRequest(vadlidationResult.Errors);
            

            await _movieService.UpdateShowtimeAsync(movieId, showtimeId, request);
            return Ok(new { Message = "showtime is updated now" });
        }

        [HttpDelete("{movieId}/showtimes/{showtimeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteShowtime(Guid movieId, Guid showtimeId)
        {
            await _movieService.DeleteShowtimeAsync(movieId, showtimeId);
            return Ok(new { Message = "Showtime deleted successfully!!!" });
        }
    }
}
