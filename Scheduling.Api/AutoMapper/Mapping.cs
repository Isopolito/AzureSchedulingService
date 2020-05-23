using System;
using AutoMapper;

namespace Scheduling.Api.AutoMapper
{
    // Doing it this way instead of using DI allows for the mapping to stay local to this project only,
    // The composition root doesn't have to be aware of the mapping that goes on in this assembly
    internal static class Mapping
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                // This line ensures that internal properties are also mapped over.
                cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
                cfg.AddProfile<JobProfile>();
            });
            var mapper = config.CreateMapper();
            return mapper;
        });

        public static IMapper Mapper => Lazy.Value;
    }
}