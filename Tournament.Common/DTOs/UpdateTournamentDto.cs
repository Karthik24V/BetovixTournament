using System.Collections.Generic;
using FluentValidation;

namespace Tournament.Common.DTOs
{
    public class UpdateTournamentDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int GameType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TournamentParticipationRuleDto Rules { get; set; }
    }


    public class UpdateTournamentDtoValidator : AbstractValidator<UpdateTournamentDto>
    {
        public UpdateTournamentDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.GameType).NotEmpty().WithMessage("GameType is required.")
                .GreaterThan(0).WithMessage("GameType must be a valid non-zero value.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .LessThan(x => x.EndDate).WithMessage("Start date must be before the end date.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");

            RuleFor(x => x.Rules)
                .NotNull().WithMessage("Participation rules are required.")
                .SetValidator(new TournamentParticipationRuleDtoValidator());
        }
    }

}