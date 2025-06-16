using AutoMapper;
using Tournament.Common.Dto_s;
using Tournament.Common.Enums;
using Tournament.Common.Helpers;
using Tournament.Domain.DataBase.Entity;
using Tournament.Workerservices.IWokerRepo;
using Tournament.Workerservices.IWorkerServices;
using Action = Tournament.Common.Enums.Action;
using System.Text.Json;

namespace Tournament.Workerservices.WorkerServices
{
    /// <summary>
    /// Service for handling tournament event processing and settlement logic.
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepo _eventRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<EventService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventService"/> class.
        /// </summary>
        public EventService(IEventRepo repository, IMapper mapper, ILogger<EventService> logger)
        {
            _eventRepo = repository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Handles the processing of a Bet or Spin event for a tournament.
        /// </summary>
        public async Task HandleBetSpinEvent(TournamentEventDto dto)
        {
            try
            {
                _logger.LogInformation("Processing Bet/Spin event for AccountId: {AccountId}, EventRefId: {EventRefId}", dto.AccountId, dto.EventRefId);

                var metaData = JsonToObjectDeserializer.StringToObjectConvertor<BetSpinMetadata>(dto.MetaData);

                if (metaData.points != null)
                {
                    var id = metaData.points.FirstOrDefault().TournamentId;
                    var tournament = await _eventRepo.GetByIdAsync(id);
                    bool isValid = false;

                    // Validate event based on action and tournament rules
                    isValid = dto.Action == "Bet" ? dto.SourceValue >= tournament.ParticipationRules.MinBetAmount : true
                                            && metaData.points.All(_ => _.Odd >= tournament.ParticipationRules.MinOdd);

                    if (isValid)
                    {
                        try
                        {
                            var Event = new TournamentEvent
                            {
                                TicketId = metaData.ticketId,
                                Action = Enum.TryParse<Action>(dto.Action, out var action) ? action : 0,
                                AccountId = dto.AccountId,
                                EventRefId = dto.EventRefId,
                                SourceValue = dto.SourceValue,
                                DateAdded = DateTime.SpecifyKind(dto.DateAdded, DateTimeKind.Utc),
                                ProcessedOn = dto.DateSend.HasValue
                                                ? DateTime.SpecifyKind(dto.DateSend.Value, DateTimeKind.Utc)
                                                : null,
                                MetaData = JsonSerializer.Serialize(metaData),
                            };
                            await _eventRepo.AddTournamenntEvent(Event, metaData.points.FirstOrDefault().TournamentId);

                            _logger.LogInformation("Event added successfully for AccountId: {AccountId}, TicketId: {TicketId}", dto.AccountId, metaData.ticketId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error adding event for AccountId: {AccountId}, TicketId: {TicketId}", dto.AccountId, metaData.ticketId);
                            Console.WriteLine($"Error adding event: {ex.Message}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid event for AccountId: {AccountId}, EventRefId: {EventRefId}", dto.AccountId, dto.EventRefId);
                        Console.WriteLine("Event is Invalid");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing Bet/Spin event for AccountId: {AccountId}, EventRefId: {EventRefId}", dto.AccountId, dto.EventRefId);
                Console.WriteLine($"Error adding event: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the processing of a BetWin or SpinWin event for a tournament, including settlement and leaderboard updates.
        /// </summary>
        public async Task HandleBetSpinWinEvent(TournamentEventDto dto)
        {
            try
            {
                _logger.LogInformation("Processing BetWin/SpinWin event for AccountId: {AccountId}, EventRefId: {EventRefId}", dto.AccountId, dto.EventRefId);

                var metaData = JsonToObjectDeserializer.StringToObjectConvertor<BetSpinMetadata>(dto.MetaData);
                if (metaData?.points == null || !metaData.points.Any())
                {
                    _logger.LogWarning("No points found in metadata for AccountId: {AccountId}, EventRefId: {EventRefId}", dto.AccountId, dto.EventRefId);
                    return;
                }

                var tournamentId = metaData.points.First().TournamentId;
                var tournament = await _eventRepo.GetByIdAsync(tournamentId);
                var eventData = await _eventRepo.GetTournamentEventByAccIdAndTicketId(dto.AccountId.ToString(), metaData.ticketId, tournamentId);

                var actionType = dto.Action.Contains("Bet") ? "Bet" : "Spin";
                var betSpinEvents = eventData.Where(e => e.Action.ToString() == actionType).FirstOrDefault();

                if (betSpinEvents == null)
                {
                    _logger.LogWarning("No matching Bet/Spin event found for settlement. AccountId: {AccountId}, TicketId: {TicketId}", dto.AccountId, metaData.ticketId);
                    return;
                }

                bool isValid = (dto.Action == "BetWin" || dto.Action == "SpinWin") &&  dto.SourceValue >= tournament.ParticipationRules.MinBetAmount 
                                            && metaData.points.All(_ => _.Odd >= tournament.ParticipationRules.MinOdd);

                if (!isValid)
                {
                    _logger.LogWarning("Invalid event for AccountId: {AccountId}, EventRefId: {EventRefId}", dto.AccountId, dto.EventRefId);
                    Console.WriteLine("Event is Invalid");
                    return;
                }
                decimal winAmount = 0;
                decimal betAmount = betSpinEvents.SourceValue;
                var pointsList = new List<TournamentPoint>();
                decimal multiplier = 0;

                if (metaData.points.Count == 1)
                {
                    // Single bet
                    var p = metaData.points.First();
                    winAmount = betSpinEvents.SourceValue * p.Odd;
                    pointsList.Add(new TournamentPoint
                    {
                        AccountId = dto.AccountId,
                        EventRefId = dto.EventRefId,
                        Points = winAmount / betSpinEvents.SourceValue,
                        WinAmount = winAmount
                    });

                    multiplier = p.Odd;
                }
                else if (metaData.stakes.systems == null || !metaData.stakes.systems.Any())
                {
                    // Multiple (Accumulator/Parlay) bet
                    bool allWon = metaData.points.All(p => p.Result?.ToLower() == "win");
                    if (allWon)
                    {
                        decimal totalOdd = metaData.points.Aggregate(1m, (acc, p) => acc * p.Odd);
                        winAmount = betSpinEvents.SourceValue * totalOdd;
                        multiplier = totalOdd;
                    }
                    else
                    {
                        winAmount = 0;
                    }
                    pointsList = metaData.points.Select(p => new TournamentPoint
                    {
                        AccountId = dto.AccountId,
                        EventRefId = dto.EventRefId,
                        Points = winAmount / betSpinEvents.SourceValue,
                        WinAmount = winAmount
                    }).ToList();
                }
                else
                {
                    // System Bet
                    int systemLevel = int.Parse(metaData.stakes.systems.Keys.First());
                    var combinations = GenerateCombinations(metaData.points, systemLevel);
                    foreach (var combo in combinations)
                    {
                        if (combo.All(p => p.Result?.ToLower() == "win"))
                        {
                            decimal comboOdd = combo.Aggregate(1m, (acc, p) => acc * (decimal)p.Odd);
                            winAmount += betSpinEvents.SourceValue * comboOdd;
                            multiplier += comboOdd;
                        }
                    }
                    pointsList.Add(new TournamentPoint
                    {
                        AccountId = dto.AccountId,
                        EventRefId = dto.EventRefId,
                        Points = winAmount / betSpinEvents.SourceValue,
                        WinAmount = winAmount,
                        CreatedOn = DateTime.UtcNow
                    });
                }

                var settlementEvent = new TournamentEvent
                {
                    TicketId = metaData.ticketId,
                    Action = Enum.TryParse<Action>(dto.Action, out var action) ? action : 0,
                    AccountId = dto.AccountId,
                    EventRefId = dto.EventRefId,
                    SourceValue = dto.SourceValue,
                    DateAdded = DateTime.SpecifyKind(dto.DateAdded, DateTimeKind.Utc),
                    ProcessedOn = dto.DateSend.HasValue ? DateTime.SpecifyKind(dto.DateSend.Value, DateTimeKind.Utc) : null,
                    WinAmount = winAmount,
                    Points = pointsList,
                    MetaData = JsonSerializer.Serialize(metaData)
                };

                await _eventRepo.AddTournamenntEvent(settlementEvent, tournamentId);

                _logger.LogInformation("Settlement event added for AccountId: {AccountId}, TicketId: {TicketId}, WinAmount: {WinAmount}", dto.AccountId, metaData.ticketId, winAmount);

                var existingLeaderboard = await _eventRepo.GetLeaderboardEntry(dto.AccountId, tournamentId);
                if (existingLeaderboard != null)
                {
                    existingLeaderboard.TotalStake += betAmount;
                    existingLeaderboard.TotalWin += winAmount;
                    existingLeaderboard.TotalPoints += winAmount / betSpinEvents.SourceValue;
                    existingLeaderboard.BestMultiplier = Math.Max(existingLeaderboard.BestMultiplier, multiplier);
                    existingLeaderboard.LastUpdated = DateTime.UtcNow;

                    await _eventRepo.UpdateLeaderboardEntry(existingLeaderboard);

                    _logger.LogInformation("Leaderboard entry updated for AccountId: {AccountId}, TournamentId: {TournamentId}", dto.AccountId, tournamentId);
                }
                else
                {
                    var participant = await _eventRepo.GetParticipantByIdAsync(dto.AccountId, tournamentId);
                    var newLeaderboard = new LeaderboardEntry
                    {
                        TournamentId = tournamentId,
                        AccountId = dto.AccountId,
                        TotalStake = betAmount,
                        TotalWin = winAmount,
                        TotalPoints = winAmount / betSpinEvents.SourceValue,
                        BestMultiplier = multiplier,
                        LastUpdated = DateTime.UtcNow,
                        ParticipantId = participant.Id
                    };
                    await _eventRepo.AddLeaderboardEntry(newLeaderboard);

                    _logger.LogInformation("New leaderboard entry created for AccountId: {AccountId}, TournamentId: {TournamentId}", dto.AccountId, tournamentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing BetWin/SpinWin event for AccountId: {AccountId}, EventRefId: {EventRefId}", dto.AccountId, dto.EventRefId);
                Console.WriteLine($"Error processing BetWin/SpinWin event: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates all possible combinations of a specified size from a list.
        /// </summary>
        private List<List<T>> GenerateCombinations<T>(List<T> list, int r)
        {
            List<List<T>> result = new List<List<T>>();
            void Combine(List<T> temp, int start)
            {
                if (temp.Count == r)
                {
                    result.Add([.. temp]);
                    return;
                }
                for (int i = start; i < list.Count; i++)
                {
                    temp.Add(list[i]);
                    Combine(temp, i + 1);
                    temp.RemoveAt(temp.Count - 1);
                }
            }
            Combine(new List<T>(), 0);
            return result;
        }
    }
}
