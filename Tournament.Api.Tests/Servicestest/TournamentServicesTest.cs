using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament.Business.IServices;
using Tournament.Business.Services;
using Tournament.Common.DTOs;
using Tournament.Data.IRepository;
using Tournament.Domain.DataBase.Entity;

namespace Tournament.Api.Tests.Servicestest
{

    [TestFixture]
    public class TournamentServiceTests
    {
        private Mock<ITournamentRepository> _repositoryMock;
        private Mock<IMapper> _mapperMock;
        private ITournamentService _tournamentService;
        private Mock<ILogger<TournamentService>> _logger;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<ITournamentRepository>();
            _mapperMock = new Mock<IMapper>();
            _logger = new Mock<ILogger<TournamentService>>();
            _tournamentService = new TournamentService(_repositoryMock.Object, _mapperMock.Object, _logger.Object);
        }

        [Test]
        public async Task CreateTournamentAsync_ValidDto_ReturnsTournamentDto()
        {
            var createDto = new CreateTournamentDto
            {
                Name = "Test Tournament",
                Description = "A test",
                GameType = 1,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3),
                ParticipationRule = new TournamentParticipationRuleDto
                {
                    MinBetAmount = 10,
                    GameCode = "ABC"
                }
            };

            var tournamentEntity = new TournamentEntity();
            var ruleEntity = new TournamentParticipationRule();
            var tournamentDto = new TournamentDto { Name = "Test Tournament" };

            _mapperMock.Setup(m => m.Map<TournamentEntity>(createDto)).Returns(tournamentEntity);
            _mapperMock.Setup(m => m.Map<TournamentParticipationRule>(createDto.ParticipationRule)).Returns(ruleEntity);
            _mapperMock.Setup(m => m.Map<TournamentDto>(It.IsAny<TournamentEntity>())).Returns(tournamentDto);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<TournamentEntity>())).Returns(Task.CompletedTask);

            var result = await _tournamentService.CreateTournamentAsync(createDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Test Tournament"));
        }

        [Test]
        public async Task CreateTournamentAsync_InvalidDto_ReturnsNull()
        {
            // Arrange
            var invalidDto = new CreateTournamentDto
            {
                Name = null,
                Description = "",
                GameType = -1, 
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(5), 
                ParticipationRule = new TournamentParticipationRuleDto
                {
                    MinBetAmount = -10, 
                    MinOdd = null,
                    MinSpinsCount = null,
                    MinEvents = null,
                    GameCode = null
                }
            };

            _mapperMock.Setup(m => m.Map<TournamentEntity>(It.IsAny<CreateTournamentDto>()))
                       .Returns(new TournamentEntity());

            _mapperMock.Setup(m => m.Map<TournamentParticipationRule>(It.IsAny<TournamentParticipationRuleDto>()))
                       .Returns(new TournamentParticipationRule());

            _mapperMock.Setup(m => m.Map<TournamentDto>(It.IsAny<TournamentEntity>()))
                       .Returns((TournamentDto)null); 

            // Act
            var result = await _tournamentService.CreateTournamentAsync(invalidDto);

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public async Task UpdateTournamentAsync_ValidId_ReturnsUpdatedDto()
        {
            long id = 1;
            var updateDto = new UpdateTournamentDto
            {
                Name = "Updated",
                Description = "Updated Desc",
                GameType = 2,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                Rules = new TournamentParticipationRuleDto
                {
                    MinBetAmount = 20,
                    GameCode = "XYZ"
                }
            };

            var entity = new TournamentEntity { ParticipationRules = new TournamentParticipationRule() };
            var updatedDto = new TournamentDto { Name = "Updated" };

            _repositoryMock.Setup(r => r.GetTournamentWithRulesByIdAsync(id)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map(updateDto, entity)).Verifiable();
            _mapperMock.Setup(m => m.Map(updateDto.Rules, entity.ParticipationRules)).Verifiable();
            _repositoryMock.Setup(r => r.UpdateAsync(entity)).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<TournamentDto>(entity)).Returns(updatedDto);

            var result = await _tournamentService.UpdateTournamentAsync(id, updateDto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Updated"));
        }

        [Test]
        public async Task UpdateTournamentAsync_InvalidId_ReturnsNull()
        {
            long id = 999;
            var updateDto = new UpdateTournamentDto { Name = "Invalid" };

            _repositoryMock.Setup(r => r.GetTournamentWithRulesByIdAsync(id)).ReturnsAsync((TournamentEntity)null);

            var result = await _tournamentService.UpdateTournamentAsync(id, updateDto);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task JoinTournamentAsync_ValidDto_ReturnsTrue()
        {
            var dto = new ParticipationDto
            {
                TournamentId = 1,
                AccountId = 10,
                UserId = 100
            };

            var tournament = new TournamentEntity
            {
                Id = 1,
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Participants = new List<TournamentParticipant>()
            };

            var user = new User { Id = dto.UserId };

            _repositoryMock.Setup(r => r.GetByIdAsync(dto.TournamentId)).ReturnsAsync(tournament);
            _repositoryMock.Setup(r => r.GetUserByIdAsync(dto.UserId)).ReturnsAsync(user);
            _repositoryMock.Setup(r => r.AddParticipant(It.IsAny<TournamentParticipant>())).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.GetParticipantByIdAsync(dto.AccountId, dto.TournamentId))
                .ReturnsAsync(new TournamentParticipant { AccountId = dto.AccountId });

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TournamentEntity>())).Returns(Task.CompletedTask);

            var result = await _tournamentService.JoinTournamentAsync(dto);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task JoinTournamentAsync_OutsideDate_ReturnsFalse()
        {
            var dto = new ParticipationDto
            {
                TournamentId = 1,
                AccountId = 10,
                UserId = 100
            };

            var tournament = new TournamentEntity
            {
                Id = 1,
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(-5),
                Participants = new List<TournamentParticipant>()
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(dto.TournamentId)).ReturnsAsync(tournament);

            var result = await _tournamentService.JoinTournamentAsync(dto);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task JoinTournamentAsync_AlreadyJoined_ReturnsFalse()
        {
            var dto = new ParticipationDto
            {
                TournamentId = 1,
                AccountId = 10,
                UserId = 100
            };

            var tournament = new TournamentEntity
            {
                Id = 1,
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(3),
                Participants = new List<TournamentParticipant>
                {
                    new TournamentParticipant { AccountId = 10, TournamentId = 1 }
                }
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(dto.TournamentId)).ReturnsAsync(tournament);

            var result = await _tournamentService.JoinTournamentAsync(dto);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetRecentTournamentWinnersAsync_ReturnsMappedDtos_WhenWinnersExist()
        {
            // Arrange
            var minCount = 2;
            var winners = new List<(TournamentEntity Tournament, LeaderboardEntry Winner)>
            {
                (new TournamentEntity { Id = 1, Name = "T1" }, new LeaderboardEntry { AccountId = 10, BestMultiplier = 2.5m }),
                (new TournamentEntity { Id = 2, Name = "T2" }, new LeaderboardEntry { AccountId = 20, BestMultiplier = 3.1m })
            };
            _repositoryMock.Setup(r => r.GetRecentTournamentWinnersAsync(minCount))
                .ReturnsAsync(winners);

            // Act
            var result = await _tournamentService.GetRecentTournamentWinnersAsync(minCount);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].TournamentId, Is.EqualTo(1));
            Assert.That(result[0].TournamentName, Is.EqualTo("T1"));
            Assert.That(result[0].WinnerAccountId, Is.EqualTo(10));
            Assert.That(result[0].WinnerBestMultiplier, Is.EqualTo(2.5m));
        }

        [Test]
        public async Task GetRecentTournamentWinnersAsync_ReturnsEmptyList_WhenNoWinners()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetRecentTournamentWinnersAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<(TournamentEntity, LeaderboardEntry)>());

            // Act
            var result = await _tournamentService.GetRecentTournamentWinnersAsync(3);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}
