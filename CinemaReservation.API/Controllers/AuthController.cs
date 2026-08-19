using CinemaReservation.Core.DTOs;
using CinemaReservation.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaReservation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly IValidator<LoginUserDto> _loginValidator;
        public AuthController(IAuthService authService, IValidator<RegisterUserDto> registerValidator,IValidator<LoginUserDto> loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);

            if (!validationResult.IsValid)            
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
                                    
             await _authService.RegisterUserAsync(request.Username, request.Email, request.Password);
             return Ok("user registerd successfully");            
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto request)
        {
            var validationResult =await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)            
                return BadRequest(validationResult.Errors.Select(e =>e.ErrorMessage));
            
            
            //we try to log the user in
            string token = await _authService.LoginAsync(request.Username, request.Password);

            // if successful we return the token in a JSON object and frontend can easily pars it
            return Ok(new { Token = token });         
        }
    }
}
