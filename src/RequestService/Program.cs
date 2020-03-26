namespace RequestService
{
    using Topshelf;

    class Program
    {
        static int Main(string[] args)
        {
            return (int)HostFactory.Run(x => x.Service<RequestService>());
        }
    }
}