using DiegoG.RemoteHud.HudElements;
using MessagePack;
using Microsoft.Xna.Framework;

namespace DiegoG.RemoteHud.DTO;

[Union(0, typeof(LortressianDateTimeElement.ItemState))]
public interface IHudItemState
{
    public Vector2 Position { get; }
}
