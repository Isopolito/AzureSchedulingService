using AutoMapper;
using Scheduling.DataAccess.Dto;
using Scheduling.DataAccess.Entities;

namespace Scheduling.DataAccess.AutoMapper
{
    internal class JobProfile : Profile
    {
        public JobProfile()
        {
            //ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly || p.GetMethod.IsPrivate;

            CreateMap<Job, JobDto>();
            CreateMap<JobDto, Job>()
                .ForMember(m => m.RepeatInterval, opt => opt.Ignore())
                .ForMember(m => m.RepeatEndStrategy, opt => opt.Ignore());
        }
    }
}