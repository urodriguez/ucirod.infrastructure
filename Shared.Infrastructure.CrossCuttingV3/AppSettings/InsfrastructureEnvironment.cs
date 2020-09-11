namespace Shared.Infrastructure.CrossCuttingV3.AppSettings
{
    public class InsfrastructureEnvironment
    {
        public string Name { get; set; }
        public bool IsLocal()
        {
            return Name == "DEV";
        }
    }
}