using AutoMapper;
using Scheduling.SharedPackage.Models;

namespace Scheduling.Api.AutoMapper
{
    internal class JobProfile : Profile
    {
        public JobProfile()
        {
            ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly || p.GetMethod.IsPrivate;

            CreateMap<DataAccess.Dto.JobDto, Job>()
                .ForMember(m => m.RepeatEndStrategy, opt => opt.MapFrom(src => src.RepeatEndStrategyId))
                .ForMember(m => m.RepeatInterval, opt => opt.MapFrom(src => src.RepeatIntervalId))
                .ForMember(m => m.RepeatInterval, opt => opt.Ignore())
                .ForMember(m => m.RepeatEndStrategy, opt => opt.Ignore());

            CreateMap<Job, DataAccess.Dto.JobDto>()
                .ForMember(m => m.RepeatEndStrategyId, opt => opt.MapFrom(src => (int)src.RepeatEndStrategy))
                .ForMember(m => m.RepeatIntervalId, opt => opt.MapFrom(src => (int)src.RepeatInterval));
        }
    }
}