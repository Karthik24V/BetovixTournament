using System.Collections.Generic;
using FluentValidation;

namespace Tournament.Common.DTOs
{
    public class CreateTournamentDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int GameType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TournamentParticipationRuleDto ParticipationRule { get; set; }
    }

    public class CreateTournamentDtoValidator : AbstractValidator<CreateTournamentDto>
    {
        public CreateTournamentDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.GameType)
                .NotEmpty().WithMessage("GameType is required.")
                .GreaterThan(0).WithMessage("GameType must be a valid non-zero value.");

            RuleFor(x => x.StartDate)
             .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required.");

            RuleFor(x => x.StartDate)
                .Must((dto, startDate) => startDate < dto.EndDate)
                .WithMessage("Start date must be before the end date.")
                .When(x => x.StartDate != default && x.EndDate != default);

            RuleFor(x => x.EndDate)
                .Must((dto, endDate) => endDate > dto.StartDate)
                .WithMessage("End date must be after the start date.")
                .When(x => x.StartDate != default && x.EndDate != default);

            RuleFor(x => x.ParticipationRule)
                .NotEmpty().WithMessage("Participation rules are required.")
                .SetValidator(new TournamentParticipationRuleDtoValidator());

        }
    }

}