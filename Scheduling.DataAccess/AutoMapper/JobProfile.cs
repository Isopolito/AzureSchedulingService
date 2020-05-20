using AutoMapper;
using Scheduling.DataAccess.Entities;

namespace Scheduling.DataAccess.AutoMapper
{
    internal class JobProfile : Profile
    {
        public JobProfile()
        {
            CreateMap<Job, SharedPackage.Models.Job>();
            CreateMap<SharedPackage.Models.Job, Job>();
        }
    }
}