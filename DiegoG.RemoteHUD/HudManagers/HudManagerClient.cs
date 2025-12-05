using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DiegoG.RemoteHud;
using DiegoG.RemoteHud.DTO;
using GLV.Shared.Common;
using GLV.Shared.Common.Collections;
using GLV.Shared.Networking;
using GLV.Shared.Networking.Encryption;
using GLV.Shared.Networking.Presentation;
using GLV.Shared.Networking.Transports;

namespace DiegoG.RemoteHud.HudManagers;

public class HudManagerClient : HudManager
{
    private readonly ConcurrentHashSet<string> connectedUsers = new();
    public override IReadOnlyCollection<string> ConnectedUsers => connectedUsers;
    
    public IPEndPoint EndPoint { get; }
    public GlvNetworkClient Client { get; }

    public HudManagerClient(RemoteHudGame game, IPEndPoint endpoint, string? password) : base(game)
    {
        EndPoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        Client = new GlvNetworkClient(
            new SocketTransportClient(
                new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp),
                endpoint
            ),
            new NetworkPresentationSalter(MessagePackPresentation.Default),
            string.IsNullOrWhiteSpace(password = password?.Trim())
                ? null
                : new AesNetworkEncryption(password.Trim().ToSHA256Array())
        );
    }
    
    protected override async Task ProcessMessages(CancellationToken ct)
    {
        Task? msgRun = messageQueue.IsEmpty is false
            ? Task.Run(async () =>
            {
                List<Task> taskList = [];
                while (messageQueue.TryDequeue(out var msg))
                    taskList.Add(msg.Send(Client, ct).AsTask());

                if (taskList.Count > 0)
                    await Task.WhenAll(taskList);
            }, ct)
            : null;
            
        while (Client.TryGetMessage(out var message))
        {
            if (message.Content is null) continue;
            
            var type = (NetworkMessageType)message.Header.MessageType;
            switch (type)
            {
                case NetworkMessageType.HudCommand:
                {
                    var hudcmd = await message.Content.ReadAndDeserialize<HudCommand>(ct);
                    if (Items.TryGetValue(hudcmd.Id, out var item))
                        item.ProcessCommand(hudcmd.State);
                    break;
                }

                case NetworkMessageType.HudItemUpdate:
                {
                    var hudcmd = await message.Content.ReadAndDeserialize<HudCommand>(ct);
                    if (Items.TryGetValue(hudcmd.Id, out var item))
                        item.SubmitState(hudcmd.State);
                    break;
                }

                case NetworkMessageType.HudItemAdd:
                {
                    // TODO: Implement
                    break;
                }

                case NetworkMessageType.HudItemRemove:
                {
                    var hudcmd = await message.Content.ReadAndDeserialize<HudCommand>(ct);
                    RemoveHudItem(hudcmd.Id);
                    break;
                }

                case NetworkMessageType.UserUpdate:
                {
                    var list = await message.Content.ReadAndDeserialize<List<UserUpdate>>(ct);
                    if (list is not null) 
                        foreach (var u in list)
                        {
                            if (u.Deleted)
                                connectedUsers.Remove(u.UserName);
                            else
                                connectedUsers.Add(u.UserName);
                        }

                    break;
                }

                case NetworkMessageType.Unknown:
                case NetworkMessageType.UserLogin:
                default:
                    break;
            }
        }

        if (msgRun is not null) await msgRun;
    }
}