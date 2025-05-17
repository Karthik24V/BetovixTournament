using MassTransit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament.Common.Dto_s;
using Tournament.Common.Enums;
using Tournament.Workerservices.IWorkerServices;

namespace Tournament.Workerservices.Consumers
{
    public class TournamentConsumer : IConsumer<TournamentEventDto>
    {
        private readonly IEventService _eventService;
        public TournamentConsumer(IEventService eventService)
        {
            _eventService = eventService;
        }
        public Task Consume(ConsumeContext<TournamentEventDto> context)
        {
            switch (context.Message.Action)
            {
                case nameof(Common.Enums.Action.Bet):
                case nameof(Common.Enums.Action.Spin):
                    _eventService.HandleBetSpinEvent(context.Message);
                    break;

                case nameof(Common.Enums.Action.BetWin):
                case nameof(Common.Enums.Action.SpinWin):
                    _eventService.HandleBetSpinWinEvent(context.Message);
                    break;

                default:
                    // Optionally log or handle unknown actions
                    Console.WriteLine($"Unhandled action type: {context.Message.Action}");
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
