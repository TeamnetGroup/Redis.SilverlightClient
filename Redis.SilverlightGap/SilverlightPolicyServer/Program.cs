using Topshelf;

namespace SilverlightPolicyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(configurator =>
            {
                configurator.Service<PolicyService>(sc =>
                {
                    sc.ConstructUsing(name => new PolicyService());
                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                });

                configurator.RunAsLocalSystem();
            });                
        }
    }
}
