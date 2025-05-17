using AutoMapper;
using Tournament.Common.Dto_s;
using Tournament.Common.Helpers;
using Tournament.Domain.DataBase.Entity;
using Tournament.Workerservices.IWokerRepo;
using Tournament.Workerservices.IWorkerServices;

namespace Tournament.Workerservices.WorkerServices
{
    public class EventService:IEventService
    {
        private readonly IEventRepo _eventRepo;
        private readonly IMapper _mapper;

        public EventService(IEventRepo repository, IMapper mapper)
        {
            _eventRepo = repository;
            _mapper = mapper;
        }
        public async Task HandleBetSpinEvent(TournamentEventDto dto)
        {
            var metaData = JsonToObjectDeserializer.StringToObjectConvertor<BetSpinMetadata>(dto.MetaData);

            if (metaData.points != null)
            {
                var id = metaData.points.FirstOrDefault().TournamentId;
                try
                {
                    var Event = new TournamentEvent
                    {
                        Action = Enum.TryParse<Common.Enums.Action>(dto.Action, out var action) ? action : 0,
                        AccountId = dto.AccountId,
                        EventRefId = dto.EventRefId,
                        SourceValue = dto.SourceValue,
                        DateAdded = DateTime.SpecifyKind(dto.DateAdded, DateTimeKind.Utc),
                        ProcessedOn = dto.DateSend.HasValue
                                        ? DateTime.SpecifyKind(dto.DateSend.Value, DateTimeKind.Utc)
                                        : (DateTime?)null,
                        MetaData = metaData.ToString(),
                    };
                    await _eventRepo.AddTournamenntEvent(Event, metaData.points.FirstOrDefault().TournamentId);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding event: {ex.Message}");
                }
            }
        }

        public async Task HandleBetSpinWinEvent(TournamentEventDto dto)
        {
            var metaData = JsonToObjectDeserializer.StringToObjectConvertor<BetSpinMetadata>(dto.MetaData);
            if (metaData.points != null)
            {
                var EventData = await _eventRepo.GetTournamentEventByAccountId(dto.AccountId.ToString());
                var betSpintEvent = EventData.Where(_ => _.Action == Common.Enums.Action.Bet || _.Action == Common.Enums.Action.Spin).ToList();
                decimal winAmount = dto.SourceValue - betSpintEvent.Sum(_ => _.SourceValue);

                var Event = new TournamentEvent
                {
                    Action = Enum.TryParse<Common.Enums.Action>(dto.Action, out var action) ? action : 0,
                    AccountId = dto.AccountId,
                    EventRefId = dto.EventRefId,
                    SourceValue = dto.SourceValue,
                    DateAdded = DateTime.SpecifyKind(dto.DateAdded, DateTimeKind.Utc),
                    ProcessedOn = dto.DateSend.HasValue
                                        ? DateTime.SpecifyKind(dto.DateSend.Value, DateTimeKind.Utc)
                                        : (DateTime?)null,
                    WinAmount = winAmount,  
                    Points = metaData.points.Select(_ => new TournamentPoint
                    {
                        AccountId = dto.AccountId,
                        EventId = dto.EventRefId,
                        Points = dto.SourceValue / betSpintEvent.Sum(_ => _.SourceValue),
                        WinAmount = winAmount,
                    }).ToList(),
                    MetaData = metaData.ToString(),
                };

                await _eventRepo.AddTournamenntEvent(Event,metaData.points.FirstOrDefault().TournamentId);
            }
        }
    }
}
