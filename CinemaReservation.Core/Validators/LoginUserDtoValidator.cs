using CinemaReservation.Core.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Validators
{
    public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
    {
        public LoginUserDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .Length(3, 20).WithMessage("Username must be between 3 and 20 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        }
    }
}





