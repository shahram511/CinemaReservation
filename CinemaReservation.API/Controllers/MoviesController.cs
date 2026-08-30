using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Interfaces;
using CinemaReservation.Core.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CinemaReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IValidator<CreateMovieDto> _movieValidator;     
        private readonly IWebHostEnvironment _environment;

        public MoviesController(IMovieService movieService, IValidator<CreateMovieDto> movieValidator, IWebHostEnvironment environment)
        {
            _movieService = movieService;
            _movieValidator = movieValidator;
            _environment = environment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetMovies()
        {
            var movies = await _movieService.GetMoviesAsync();
            return Ok(movies);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMovie(Guid id)
        {
            var movie = await _movieService.GetMovieAsync(id);
            return Ok(movie);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMovie([FromBody] CreateMovieDto request)
        {
            var validationResult  = await _movieValidator.ValidateAsync(request);

            if (!validationResult.IsValid)            
                return BadRequest(validationResult.Errors);
            

            var movie = await _movieService.CreateMovieAsync(request);

            return CreatedAtAction(nameof(GetMovie), new {id = movie.Id}, new {Message = "Movie Created Successfully.", movieId = movie.Id});
        }

        [HttpPut("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> UpdateMovie(Guid id, [FromBody] CreateMovieDto request)
        {
            var validationResult = await _movieValidator.ValidateAsync(request);

            if (!validationResult.IsValid)            
                return BadRequest(validationResult.Errors);
            

            await _movieService.UpdateMovieAsync(id, request);
            return Ok(new { Message = "Movie updated successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> DeleteMovie(Guid id)
        {
            await _movieService.DeleteMovieAsync(id);
            return Ok(new {Message = "Movie deleted successfully."});
        }

        [HttpPost("{id}/poster")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePoster(Guid id, IFormFile file)
        {
            //  validat the file
            if (file == null || file.Length == 0)            
                return BadRequest(new { Message = "No file was uploaded" });
            

            // optional: check if its an actual image(jpg.png)
            var extensin = Path.GetExtension(file.FileName).ToLower();

            if (extensin != ".jpg" && extensin != ".png" && extensin != ".jpeg")            
                return BadRequest(new { Message = "Only JPG , PNG , PGEG are allowed" });
            

            // define the save path using the  Movie  Id to gurantee a unique name
            var folderPath = Path.Combine(_environment.WebRootPath, "posters");

            // Ensure the directory exist
            if (!Directory.Exists(folderPath))            
                Directory.CreateDirectory(folderPath);
            

            var fileName = $"{id}{extensin}";
            var exactFilePath = Path.Combine(folderPath, fileName);

            // Save the physical file to the hard drive
            using (var stream = new FileStream(exactFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update the database with the relative URL
            var relativeUrl = $"/posters/{fileName}";
            await _movieService.UpdatePosterUrlAsync(id, relativeUrl);

            return Ok(new { Message = "Poster Uploaded perfectlly.", PosterUrl = relativeUrl });
        }
    }
}
