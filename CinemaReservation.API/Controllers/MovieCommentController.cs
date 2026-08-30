using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinemaReservation.API.Controllers
{
    [Route("api/movie/{movieId:guid}/[controller]")]
    [ApiController]
    
    public class MovieCommentController : ControllerBase
    {
        private readonly IMovieCommentService _commentService;
        private readonly IValidator<CreateCommentDto> _createCommentValidator;

        public MovieCommentController(IMovieCommentService commentService, IValidator<CreateCommentDto> createCommentValidator)
        {
            _commentService = commentService;
            _createCommentValidator = createCommentValidator;
        }

        [HttpGet]        
        public async Task<IActionResult> GetComments([FromRoute]Guid movieId)
        {
            var comments  = await _commentService.GetCommentsByMovieIdAsync(movieId);
            return Ok(comments);
        }

        [HttpPost]
        [Authorize]        
        public async Task<IActionResult> AddComment([FromRoute]Guid movieId, [FromBody] CreateCommentDto request)
        {
            if (movieId == Guid.Empty)            
                return BadRequest("A valid Movie ID is required in the URL to post a comment.");
            

            var validationResult = await _createCommentValidator.ValidateAsync(request);

            if (!validationResult.IsValid)            
                return BadRequest(validationResult.Errors);
            

            var userIdString = User.FindFirst("userId")?.Value;
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized("Invalid user token");

            var newComment = await _commentService.AddCommentAsync(movieId, userId, userName, request);

            return CreatedAtAction(nameof(GetComments), new { movieId }, newComment);
        }

        [HttpGet("info")]        
        public async Task<IActionResult> GetNumberCommentsAverageRateAsync([FromRoute] Guid movieId)
        {
            var result = await _commentService.GetCommentInfoByMovieIdAsync(movieId);

            return Ok(result);
        }
    }
}
