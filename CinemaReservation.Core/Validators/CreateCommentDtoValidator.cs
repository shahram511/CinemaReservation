using CinemaReservation.Core.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Validators
{
    public class CreateCommentDtoValidator:  AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 10)
                .WithMessage("Rating must be a number between 1 to 10,");

            RuleFor(x => x.Text)
                .NotEmpty()
                .WithMessage("comment text cannot be empty")
                .MaximumLength(1000)
                .WithMessage("Comment lengh cannot exeed 1000 characters.");
        }
    }
}
