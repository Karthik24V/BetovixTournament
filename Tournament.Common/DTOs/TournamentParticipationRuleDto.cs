using FluentValidation;

namespace Tournament.Common.DTOs
{
    public class TournamentParticipationRuleDto
    {
        public decimal MinBetAmount { get; set; }
        public decimal? MinOdd { get; set; }
        public int? MinSpinsCount { get; set; }
        public int? MinEvents { get; set; }
        public string GameCode { get; set; }
    }



    public class TournamentParticipationRuleDtoValidator : AbstractValidator<TournamentParticipationRuleDto>
    {
        public TournamentParticipationRuleDtoValidator()
        {
            RuleFor(x => x.MinBetAmount).NotEmpty().WithMessage("Minimum Bet  Amount is Required")
                .GreaterThan(0).WithMessage("MinBetAmount must be greater than 0.");

            RuleFor(x => x.MinOdd)
                .GreaterThan(0).When(x => x.MinOdd.HasValue)
                .WithMessage("MinOdd must be greater than 0.");

            RuleFor(x => x.MinSpinsCount)
                .GreaterThan(0).When(x => x.MinSpinsCount.HasValue)
                .WithMessage("MinSpinsCount must be 0 or greater.");

            RuleFor(x => x.MinEvents)
                .GreaterThan(1).When(x => x.MinEvents.HasValue)
                .WithMessage("MinEvents must be 1 or greater.");

            RuleFor(x => x.GameCode)
                .NotEmpty().WithMessage("GameCode is required.");
        }
    }

}
