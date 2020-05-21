using AutoMapper;

namespace Scheduling.DataAccess.AutoMapper
{
    internal class JobProfile : Profile
    {
        public JobProfile()
        {
            ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly || p.GetMethod.IsPrivate;

            CreateMap<Entities.Job, SharedPackage.Models.Job>()
                .ForMember(m => m.RepeatEndStrategy, opt => opt.MapFrom(src => src.RepeatEndStrategyId))
                .ForMember(m => m.RepeatInterval, opt => opt.MapFrom(src => src.RepeatIntervalId));

            CreateMap<SharedPackage.Models.Job, Entities.Job>()
                .ForMember(m => m.RepeatEndStrategyId, opt => opt.MapFrom(src => (int)src.RepeatEndStrategy))
                .ForMember(m => m.RepeatIntervalId, opt => opt.MapFrom(src => (int)src.RepeatInterval))
                .ForMember(m => m.RepeatInterval, opt => opt.Ignore())
                .ForMember(m => m.RepeatEndStrategy, opt => opt.Ignore());
        }
    }
}