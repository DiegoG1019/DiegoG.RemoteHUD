using System;
using System.Threading.Tasks;
using DiegoG.MonoGame.Extended;
using DiegoG.RemoteHud.DTO;
using DiegoG.RemoteHud.HudManagers;
using GLV.Shared.Data.Identifiers;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace DiegoG.RemoteHud;

public abstract class HudItem(HudManager manager) : DrawableGameComponent(manager.Game), IPositionable
{
    public long UniqueId { get; } = Snowflake.New().AsLong();
    public string DebugName => field ??= $"{UniqueId}:{GetType().Name}";
    
    public HudManager Manager { get; } = manager ?? throw new ArgumentNullException(nameof(manager));
    
    protected abstract bool HasChanged { get; }
    protected abstract IHudItemState? GetSerializableState();
    protected abstract void SetDeserializedState(IHudItemState? state);
    
    public abstract void ProcessCommand(IHudItemState? state);

    public void CheckState()
    {
        if (HasChanged)
            Manager.EnqueueSendMessage(NetworkMessageType.HudItemUpdate, new HudCommand(UniqueId, GetSerializableState()));
    }

    public void SubmitState(IHudItemState? state) => SetDeserializedState(state);
    public void SubmitCommand(IHudItemState? state) => ProcessCommand(state);

    public void AddToManager() => manager.AddHudItem(this);
    public bool RemoveFromManager() => manager.RemoveHudItem(UniqueId);
    public Vector2 Position { get; set; }
}