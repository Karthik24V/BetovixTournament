using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Tournament.Api.Controllers;
using Tournament.Business.IServices;
using Tournament.Common.DTOs;
using Tournament.Common.Response;

namespace Tournament.Api.Tests.ControllersTest
{
    public class TournamentControllerTests
    {
        private Mock<ITournamentService> _tournamentServiceMock;
        private TournamentController _controller;

        [SetUp]
        public void Setup()
        {
            _tournamentServiceMock = new Mock<ITournamentService>();
            _controller = new TournamentController(_tournamentServiceMock.Object);
        }

        private List<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Test]
        public async Task CreateTournament_ValidData()
        {
            var createDto = new CreateTournamentDto
            {
                Name = "Test Tournament",
                GameType = 1,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(2),
                Description = "Sample Description",
                ParticipationRule = new TournamentParticipationRuleDto
                {
                    GameCode = "102",
                    MinEvents = 1,
                    MinOdd = 1,
                    MinSpinsCount = 1,
                    MinBetAmount = 1000
                }
            };
            var tournamentDto = new TournamentDto { Id = 1, Name = createDto.Name };

            _tournamentServiceMock.Setup(s => s.CreateTournamentAsync(createDto)).ReturnsAsync(tournamentDto);

            var result = await _controller.CreateTournament(createDto);
            var createdResult = result.Result as CreatedAtActionResult;
            var response = createdResult?.Value as ApiResponse<TournamentDto>;

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Message, Is.EqualTo("Tournament created successfully"));
            Assert.That(response.Data.Id, Is.EqualTo(1));
            Assert.That(response.Data.Name, Is.EqualTo("Test Tournament"));
        }

        [Test]
        public async Task CreateTournament_InvalidData()
        {
            var invalidDto = new CreateTournamentDto
            {
                Name = null,
                GameType = 1,
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.Now.AddDays(2),
                ParticipationRule = null
            };

            _controller.ModelState.AddModelError("Name", "Name is required");

            var result = await _controller.CreateTournament(invalidDto);
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateTournament_ValidId()
        {
            var updateDto = new UpdateTournamentDto
            {
                Name = "Updated Tournament",
                Description = "Sample Description",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                GameType = 102,
                Rules = new TournamentParticipationRuleDto
                {
                    MinSpinsCount = 1,
                    GameCode = "102",
                    MinBetAmount = 1,
                    MinEvents = 1,
                    MinOdd = 1
                }
            };
            var tournamentDto = new TournamentDto { Id = 1, Name = "Updated Tournament" };

            _tournamentServiceMock.Setup(s => s.UpdateTournamentAsync(1, updateDto)).ReturnsAsync(tournamentDto);

            var result = await _controller.UpdateTournament(1, updateDto);
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<TournamentDto>;

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response, Is.Not.Null);
            Assert.That(response.Data.Name, Is.EqualTo("Updated Tournament"));
        }

        [Test]
        public async Task UpdateTournament_InvalidId_ReturnsNotFound()
        {
            var updateDto = new UpdateTournamentDto { Name = "Invalid" };
            _tournamentServiceMock.Setup(s => s.UpdateTournamentAsync(999, updateDto)).ReturnsAsync((TournamentDto)null);

            var result = await _controller.UpdateTournament(999, updateDto);
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteTournament_ValidId()
        {
            _tournamentServiceMock.Setup(s => s.DeleteTournamentAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteTournament(1);
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<string>;

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Message, Is.EqualTo("Tournament deleted successfully"));
        }

        [Test]
        public async Task DeleteTournament_InvalidId()
        {
            _tournamentServiceMock.Setup(s => s.DeleteTournamentAsync(999)).ReturnsAsync(false);

            var result = await _controller.DeleteTournament(999);
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetTournamentById_ValidId()
        {
            var dto = new TournamentDto { Id = 1, Name = "Tournament" };
            _tournamentServiceMock.Setup(s => s.GetTournamentByIdAsync(1)).ReturnsAsync(dto);

            var result = await _controller.GetTournamentById(1);
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<TournamentDto>;

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Data.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetTournamentById_InvalidId()
        {
            _tournamentServiceMock.Setup(s => s.GetTournamentByIdAsync(999)).ReturnsAsync((TournamentDto)null);

            var result = await _controller.GetTournamentById(999);
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task JoinTournament_ValidData()
        {
            var dto = new ParticipationDto { AccountId = 1, TournamentId = 2 };
            _tournamentServiceMock.Setup(s => s.JoinTournamentAsync(dto)).ReturnsAsync(true);

            var result = await _controller.JoinTournament(dto);
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<ParticipationDto>;

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Data.AccountId, Is.EqualTo(1));
        }

        [Test]
        public async Task JoinTournament_InvalidData()
        {
            var invalidDto = new ParticipationDto { AccountId = 0, TournamentId = 0 };
            _controller.ModelState.AddModelError("AccountId", "Required");

            var result = await _controller.JoinTournament(invalidDto);
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task ParticipationStatus_ValidData()
        {
            var statusDto = new ParticipationStatusDto { Joined = true };
            _tournamentServiceMock.Setup(s => s.ParticipantStatustAsync(1, 2)).ReturnsAsync(statusDto);

            var result = await _controller.ParticipationStatus(1, 2);
            var okResult = result.Result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<ParticipationStatusDto>;

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Data.Joined, Is.True);
        }

        [Test]
        public async Task ParticipationStatus_InvalidData()
        {
            _tournamentServiceMock.Setup(s => s.ParticipantStatustAsync(0, 0)).ReturnsAsync((ParticipationStatusDto)null);

            var result = await _controller.ParticipationStatus(0, 0);
            Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetLeaderboard_ValidData()
        {
            var leaderboard = new List<LeaderboardDto> { new LeaderboardDto { TotalPoints = 100 } };
            _tournamentServiceMock.Setup(s => s.GetLeaderBoardData(1, 1, 10, "points")).ReturnsAsync(leaderboard);

            var result = await _controller.GetLeaderboard(1);
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<ICollection<LeaderboardDto>>;

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Data.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetLeaderboard_InvalidTournamentId_ReturnsNotFound()
        {
            // Arrange
            _tournamentServiceMock.Setup(s => s.GetLeaderBoardData(0, 1, 10, "points")).ReturnsAsync((ICollection<LeaderboardDto>)null);

            // Act
            var result = await _controller.GetLeaderboard(0);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFound = result as NotFoundObjectResult;
            Assert.That(notFound?.StatusCode, Is.EqualTo(404));
        }


        [Test]
        public async Task GetCurrentTournament()
        {
            var tournaments = new List<TournamentDto> { new TournamentDto { Id = 1 } };
            _tournamentServiceMock.Setup(s => s.GetCurrentTournament()).ReturnsAsync(tournaments);

            var result = await _controller.GetCurrentTournament();
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<ICollection<TournamentDto>>;

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Data.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetCurrentTournament_NoData_ReturnsEmptyList()
        {
            _tournamentServiceMock.Setup(s => s.GetCurrentTournament()).ReturnsAsync(new List<TournamentDto>());

            var result = await _controller.GetCurrentTournament();
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<ICollection<TournamentDto>>;

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Data, Is.Empty);
        }

        [Test]
        public async Task GetUpComingTournament()
        {
            var tournaments = new List<TournamentDto> { new TournamentDto { Id = 2 } };
            _tournamentServiceMock.Setup(s => s.GetUpcomingTournament()).ReturnsAsync(tournaments);

            var result = await _controller.GetUpComingTournament();
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<ICollection<TournamentDto>>;

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Data.First().Id, Is.EqualTo(2));
        }

        [Test]
        public async Task GetUpComingTournament_NoData_ReturnsEmptyList()
        {
            _tournamentServiceMock.Setup(s => s.GetUpcomingTournament()).ReturnsAsync(new List<TournamentDto>());

            var result = await _controller.GetUpComingTournament();
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<ICollection<TournamentDto>>;

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(response.Data, Is.Empty);
        }
    }
}
