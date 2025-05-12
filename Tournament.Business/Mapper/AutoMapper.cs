using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tournament.Common.DTOs;
using Tournament.Domain.DataBase.Entity;

namespace Tournament.Business.Mapper
{
    public class AutoMapper : Profile
    {
        /// <summary>
        /// Mappe the value with the registered types
        /// </summary>
        /// <param name="configuration"></param>
        public AutoMapper(IConfiguration configuration)
        {
            CreateMap<TournamentEntity, TournamentDto>()
                .ForMember(dest => dest.ParticipationRule, opt => opt.MapFrom(src => src.ParticipationRules));

            CreateMap<TournamentParticipationRule, TournamentParticipationRuleDto>();

            // DTO → Entity
            CreateMap<CreateTournamentDto, TournamentEntity>()
                .ForMember(dest => dest.ParticipationRules, opt => opt.Ignore()) // Will be handled manually
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));

            CreateMap<TournamentParticipationRuleDto, TournamentParticipationRule>();

            CreateMap<UpdateTournamentDto, TournamentEntity>()
                .ForMember(dest => dest.ParticipationRules, opt => opt.Ignore()) // handled manually
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
