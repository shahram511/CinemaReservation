using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinemaReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservartionService _reservationService;
        private readonly IValidator<CreateReservationDto> _reservationValidator;      

        public ReservationController(IReservartionService reservationService, IValidator<CreateReservationDto> reservationValidator)
        {
            _reservationService = reservationService;
            _reservationValidator = reservationValidator;
        }

        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            // Extract the user ID string strictly using the userId claim in token
            var userIdString = User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))            
                return Unauthorized("invalid user token. the userId claim is mising or malformed.");
            

            var UserReservation = await _reservationService.GetUserReservationAsync(userId);
            return Ok(UserReservation);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRrservation([FromBody] CreateReservationDto request)
        {
            var validationResult = await _reservationValidator.ValidateAsync(request);

            if (!validationResult.IsValid)            
                return BadRequest(validationResult.Errors);
            

            // Extract the user ID string from the JWT claims (NameIdentifier represent the uniqe user ID)
            var userIdString = User.FindFirst("userId")?.Value; 
            if (!Guid.TryParse(userIdString, out Guid userId))            
                return Unauthorized("Invalid user token");
            
            
            var reservation = await _reservationService.CreateReservationAsync(userId, request.ShowtimeId, request.SeatIds);
            return Ok(reservation);                                      
            
                 
        }

        [HttpDelete("{reservationId}")]
        public async Task<IActionResult> CancelReservaion([FromRoute] Guid reservationId)
        {
            var UserIdString = User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(UserIdString, out Guid userId))            
                return Unauthorized("Invalid user token");
            

            await _reservationService.CancelReservationAsync(userId, reservationId);
            return Ok(new { message = "Reservation cancelled successfully." });                     
        }

        [HttpDelete("{reservationId}/seats/{seatId}")]
        public async Task<IActionResult> CancleSingleSeat([FromRoute] Guid reservationId, [FromRoute] Guid seatId)
        {
            var userIdString = User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(userIdString, out Guid userId))            
                return Unauthorized("Invalid user token");
            
            
            await _reservationService.CancelSingleSeatAsync(userId, reservationId, seatId);

            return Ok(new { message = "Seat successfully removed from your reservation" });           
        }
    }
}
