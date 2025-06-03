using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Tournament.Common.DTOs
{
    public class ParticipationDto
    {
        public long TournamentId { get; set; }
        public long AccountId { get; set; }
        public long UserId { get; set; }
    }


    public class ParticipationDtoValidator : AbstractValidator<ParticipationDto>
    {
        public ParticipationDtoValidator()
        {
            RuleFor(x => x.TournamentId).NotEmpty().WithMessage("TournamentId is Required.");

            RuleFor(x => x.AccountId)
               .NotEmpty().WithMessage("AccountId is Required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is Required.");
        }
    }

}
