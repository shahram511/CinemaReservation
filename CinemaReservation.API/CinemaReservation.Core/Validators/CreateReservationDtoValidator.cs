using CinemaReservation.Core.DTOs;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaReservation.Core.Validators
{
    public class CreateReservationDtoValidator: AbstractValidator<CreateReservationDto>
    {
        public CreateReservationDtoValidator()
        {
            RuleFor(x => x.ShowtimeId)
                .NotEmpty().WithMessage("showtime id is needed");

            RuleFor(x => x.SeatIds)
                .NotEmpty().WithMessage("At least one seat must be selected")
                .Must(seats => seats != null && seats.Count <= 10).WithMessage("You cannot reserve more than 10 seats in a single transaction.")
                .Must(seats => seats != null && seats.Distinct().Count() == seats.Count()).WithMessage("Duplicate seats are not allowed in the same request.");
                
            RuleForEach(x => x.SeatIds)
                .NotEmpty().WithMessage("Individual Seat IDs cannot be empty.");              
        }
    }
}
