using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DiegoG.MonoGame.Extended;
using DiegoG.RemoteHud;
using DiegoG.RemoteHud.DTO;
using GLV.Shared.Common;
using GLV.Shared.Common.Text;
using GLV.Shared.Networking;
using ImGuiNET;
using Microsoft.Xna.Framework;

// ReSharper disable InconsistentlySynchronizedField

namespace DiegoG.RemoteHud.HudManagers;

// TODO: Include the hash as part of the handshake 

public abstract class HudManager(RemoteHudGame game) : IDrawable, IUpdateable, IGameComponent, IDebugExplorable
{
    protected class QueuedMessage
    {
        public required NetworkMessageType Type { get; init; }
        public virtual ValueTask<bool> Send(GlvNetworkClientBase client, CancellationToken ct)
            => client.SendMessage((short)Type, ct);
    }

    protected class QueuedMessage<T> : QueuedMessage
    {
        public required T Value { get; init; }
        public override ValueTask<bool> Send(GlvNetworkClientBase client, CancellationToken ct)
            => client.SendMessage<T>((short)Type, Value, ct);
    }

    public RemoteHudGame Game { get; } = game;

    private readonly Scene hudScene = new Scene(game);
    private readonly ConcurrentDictionary<long, HudItem> items = [];
    public IReadOnlyDictionary<long, HudItem> Items => items;

    public abstract IReadOnlyCollection<string> ConnectedUsers { get; }
    
    public void AddHudItem(HudItem item)
    {
        if (item.Manager != this)
            throw new ArgumentException("Cannot add a HudItem with a different manager than this one");

        lock (hudScene)
        {
            if (items.Remove(item.UniqueId, out var oldItem))
            {
                if (oldItem != item)
                {
                    hudScene.SceneComponents.Remove(oldItem);
                    hudScene.SceneComponents.Add(item);
                }
            }
            else
                hudScene.SceneComponents.Add(item);
            
            items[item.UniqueId] = item;
        }
    }

    public bool RemoveHudItem(long id)
    {
        lock (hudScene)
        {
            if (!items.TryRemove(id, out var old)) return false;
            
            hudScene.SceneComponents.Remove(old);
            return true;
        }
    }
    
    private Task? _task;
    public virtual void CheckMessages()
    {
        if (Game.HudManager is not { } manager) return;
        
        if (_task is not null)
        {
            if (!_task.IsCompleted)
                return;
            
            _task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        // TODO: Set a cancellation token here
        _task = manager.ProcessMessages(default);
    }

    protected abstract Task ProcessMessages(CancellationToken ct);

    protected readonly ConcurrentQueue<QueuedMessage> messageQueue = new();
    public void EnqueueSendMessage<T>(NetworkMessageType messageType)
    {
        messageQueue.Enqueue(new QueuedMessage()
        {
            Type = messageType
        });
    }

    public void EnqueueSendMessage<T>(NetworkMessageType messageType, T message)
    {
        messageQueue.Enqueue(new QueuedMessage<T>()
        {
            Type = messageType,
            Value = message
        });
    }
    public event EventHandler<EventArgs>? UpdateOrderChanged
    {
        add => hudScene.UpdateOrderChanged += value;
        remove => hudScene.UpdateOrderChanged -= value;
    }

    public void Draw(GameTime gameTime)
    {
        hudScene.Draw(gameTime);
    }

    public int DrawOrder
    {
        get => hudScene.DrawOrder;
        set => hudScene.DrawOrder = value;
    }

    public bool Visible
    {
        get => hudScene.Visible;
        set => hudScene.Visible = value;
    }

    public event EventHandler<EventArgs>? DrawOrderChanged
    {
        add => hudScene.DrawOrderChanged += value;
        remove => hudScene.DrawOrderChanged -= value;
    }

    public event EventHandler<EventArgs>? VisibleChanged
    {
        add => hudScene.VisibleChanged += value;
        remove => hudScene.VisibleChanged -= value;
    }

    public void Update(GameTime gameTime)
    {
        hudScene.Update(gameTime);
    }

    public bool Enabled => hudScene.Enabled;

    public int UpdateOrder => hudScene.UpdateOrder;

    public event EventHandler<EventArgs>? EnabledChanged
    {
        add => hudScene.EnabledChanged += value;
        remove => hudScene.EnabledChanged -= value;
    }

    public virtual void Initialize() { }
    
    public void RenderImGuiDebug()
    {
        foreach (var (key, item) in items)
        {
            if (item is IDebugExplorable expl)
            {
                if (!ImGui.TreeNode(item.DebugName)) continue;
                
                expl.RenderImGuiDebug();
                ImGui.TreePop();
            }
            else
                ImGui.LabelText(item.DebugName, "");
        }
    }
}
