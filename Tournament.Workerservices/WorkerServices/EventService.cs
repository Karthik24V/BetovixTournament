using AutoMapper;
using Tournament.Common.Dto_s;
using Tournament.Common.Enums;
using Tournament.Common.Helpers;
using Tournament.Domain.DataBase.Entity;
using Tournament.Workerservices.IWokerRepo;
using Tournament.Workerservices.IWorkerServices;
using Action = Tournament.Common.Enums.Action;

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
                var tournament = await _eventRepo.GetByIdAsync(id);
                bool isValid = false;

                isValid = dto.Action == "Bet" ? dto.SourceValue >= tournament.ParticipationRules.MinBetAmount : true
                                        && metaData.points.All(_ => _.Odd >= tournament.ParticipationRules.MinOdd);
                if (isValid)
                {
                    try
                    {
                        var Event = new TournamentEvent
                        {
                            Action = Enum.TryParse<Action>(dto.Action, out var action) ? action : 0,
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
                else {
                    Console.WriteLine("Event is Invalid");
                }
               
            }
        }

        public async Task HandleBetSpinWinEvent(TournamentEventDto dto)
        {
            var metaData = JsonToObjectDeserializer.StringToObjectConvertor<BetSpinMetadata>(dto.MetaData);
            if (metaData.points != null)
            {
                var tournamentId = metaData.points.FirstOrDefault().TournamentId;
                var tournament = await _eventRepo.GetByIdAsync(tournamentId);
                var EventData = await _eventRepo.GetTournamentEventByAccIdAndEventRefId(dto.AccountId.ToString(),dto.EventRefId, tournamentId);
                var actionType = dto.Action.Contains("Bet") ? "Bet" : "Spin";   
                var betSpintEvent = EventData.Where(_ => _.Action.ToString() == actionType);
                bool isValid = false;
                decimal winAmount = dto.SourceValue - betSpintEvent.Sum(_ => _.SourceValue); // Assumption source value as the total win amount.

                isValid = EventData.Count() >= tournament.ParticipationRules.MinEvents && actionType == "Spin" ?
                                    EventData.Count() >= tournament.ParticipationRules.MinSpinsCount : true;

                if (isValid)
                {
                    var Event = new TournamentEvent
                    {
                        Action = Enum.TryParse<Action>(dto.Action, out var action) ? action : 0,
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
                    await _eventRepo.AddTournamenntEvent(Event, tournamentId);
                }
                else
                {
                    Console.WriteLine("Event is Invalid");
                }
            }
        }
      
    }
}
