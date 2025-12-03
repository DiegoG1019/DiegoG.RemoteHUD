using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DiegoG.RemoteHud;
using DiegoG.RemoteHud.DTO;
using GLV.Shared.Common;
using GLV.Shared.Networking;
using GLV.Shared.Networking.Encryption;
using GLV.Shared.Networking.Presentation;
using GLV.Shared.Networking.Transports;

namespace DiegoG.RemoteHud.HudManagers;

public class HudManagerServer : HudManager
{
    protected readonly ConcurrentDictionary<string, GlvNetworkClientBase> connectedUsers = [];
    public IReadOnlyDictionary<string, GlvNetworkClientBase> ConnectedUserClients => connectedUsers;
    public override IReadOnlyCollection<string> ConnectedUsers => (ReadOnlyCollection<string>)connectedUsers.Keys;

    public IPEndPoint EndPoint { get; }
    public GlvNetworkListener Listener { get; }

    public HudManagerServer(RemoteHudGame game, IPEndPoint endpoint, string? password) : base(game)
    {
        EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        Listener = new GlvNetworkListener(
            new SocketTransportListener(
                new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp),
                endpoint
            ),
            new NetworkPresentationSalter(MessagePackPresentation.Default),
            string.IsNullOrWhiteSpace(password = password?.Trim()) ? null : new AesNetworkEncryption(password.Trim().ToSHA256Array())
        )
        {
            DefaultClientMessageFilter = (l, m) =>
            {
                if ((NetworkMessageType)m.MessageType is NetworkMessageType.HudCommand or NetworkMessageType.UserLogin)
                    return ValueTask.FromResult(true);
                return ValueTask.FromResult(false);
            }
        };
    }

    protected override async Task ProcessMessages(CancellationToken ct)
    {
        bool msgProcessed = true;
        
        while (msgProcessed)
        {
            msgProcessed = false;

            Task? msgRun = messageQueue.IsEmpty is false
                ? Task.Run(async () =>
                {
                    List<Task> taskList = [];
                    while (messageQueue.TryDequeue(out var msg))
                        foreach (var client in Listener.Clients)
                            taskList.Add(msg.Send(client, ct).AsTask());

                    if (taskList.Count > 0)
                        await Task.WhenAll(taskList);
                }, ct)
                : null;

            foreach (var client in Listener.Clients)
            {
                if (client.TryGetMessage(out var message))
                {
                    msgProcessed = true;
                    var type = (NetworkMessageType)message.Header.MessageType;
                    switch (type)
                    {
                        case NetworkMessageType.HudCommand when message.Content is null:
                            continue;
                        
                        case NetworkMessageType.HudCommand:
                        {
                            var hudcmd = await message.Content.ReadAndDeserialize<HudCommand>(ct);
                            if (Items.TryGetValue(hudcmd.Id, out var item))
                                item.ProcessCommand(hudcmd.State);
                            break;
                        }
                        
                        case NetworkMessageType.UserLogin when message.Content is null:
                            continue;
                        
                        case NetworkMessageType.UserLogin:
                        {
                            var logreq = await message.Content.ReadAndDeserialize<UserLoginRequest>(ct);
                            if (connectedUsers.Remove(logreq.UserName, out var ucl))
                                await ucl.Disconnect();

                            connectedUsers[logreq.UserName] = client;
                            break;
                        }
                        
                        case NetworkMessageType.Unknown:
                        case NetworkMessageType.HudItemUpdate:
                        case NetworkMessageType.HudItemAdd:
                        case NetworkMessageType.HudItemRemove:
                        case NetworkMessageType.UserUpdate:
                        default:
                            break;
                    }
                }
            }

            if (msgRun is not null) await msgRun;
        }

        // The purpose of the way this method is written is that it will ensure that all clients get about equal processing priority
        // But also that it won't wait the 100ms the worker waits before calling this method again
    }
}
