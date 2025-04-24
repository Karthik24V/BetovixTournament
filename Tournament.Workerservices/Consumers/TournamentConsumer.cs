using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament.Common.Dto_s;

namespace Tournament.Workerservices.Consumers
{
    public class TournamentConsumer : IConsumer<TournamentRequestDto>
    {
        public Task Consume(ConsumeContext<TournamentRequestDto> context)
        {
            Console.WriteLine($"{context.Message.TournamentId}");
            return  Task.CompletedTask;
        }
    }
}
