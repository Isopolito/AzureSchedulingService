using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Scheduling.SharedPackage.Extensions
{ 
    public class SchedulingApiServicesModule : Module
    {
        private readonly SchedulingApiServiceOptions options;

        public SchedulingApiServicesModule(SchedulingApiServiceOptions options)
        {
            this.options = options;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var services = new ServiceCollection();

            services.AddSchedulingApi(options);

            services.BuildServiceProvider();
            builder.Populate(services);

            base.Load(builder);
        }
    }
}
