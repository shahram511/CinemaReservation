using CinemaReservation.Core.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Validators
{
    public class CreateMovieDtoValidator :AbstractValidator<CreateMovieDto>
    {
        public CreateMovieDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.PosterUrl).NotEmpty();
            RuleFor(x => x.Genre).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DurationInMinutes).GreaterThan(0).WithMessage("Duration must be grater than zero");
        }
    }
}
