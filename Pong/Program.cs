using System.Threading.Tasks;
using Pong.Client.App;
using Pong.Server.App;

namespace Pong;

public class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0].ToLower() == "server")
        {
            await ServerApplication.Main(args);
        }
        else
        {
            ClientApplication.Main();
        }
    }
}