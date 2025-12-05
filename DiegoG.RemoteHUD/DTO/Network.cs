using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GLV.Shared.Networking;
using GLV.Shared.Networking.Presentation;
using MessagePack;

namespace DiegoG.RemoteHud.DTO;

public enum NetworkMessageType : short
{
    Unknown = 0,
    HudItemUpdate = 1,
    HudItemAdd = 2,
    HudItemRemove = 3,
    UserLogin = 4,
    HudCommand = 5,
    UserUpdate = 6
}

[MessagePackObject]
public readonly record struct UserUpdate(
    [property: Key(0)] string UserName,
    [property: Key(1)] bool Deleted
);

[MessagePackObject]
public readonly record struct HudCommand([property: Key(0)] long Id, [property: Key(1)] IHudItemState? State);

[MessagePackObject]
public readonly record struct UserLoginRequest([property: Key(0)] string UserName);

[MessagePackObject]
public readonly record struct RemoteHudMessage([property: Key(0)] long Id, [property: Key(1)] ReadOnlyMemory<byte> State);

