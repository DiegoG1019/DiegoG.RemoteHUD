using System.Collections.Concurrent;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using GLV.Shared.Common.Collections;
using MessagePack;

namespace DiegoG.RemoteHud;

public static class Program
{
    public static RemoteHudGame Game { get; } = new RemoteHudGame();

    internal static void Main(string[] args)
        => Game.Run();
}
