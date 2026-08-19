using CinemaReservation.Core.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;


namespace CinemaReservation.Core.Validators
{
    public class CreateShowtimeDtoValidator:AbstractValidator<CreateShowtimeDto>
    {
        public CreateShowtimeDtoValidator()
        {
            RuleFor(x => x.StartTime)
                .NotEmpty()
                // Ensure the showtime is strixtly in the future
                .GreaterThan(DateTime.UtcNow).WithMessage("Show time must be scheduled in the future.");

        }
    }
}
