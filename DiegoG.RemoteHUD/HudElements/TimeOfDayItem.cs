using System;
using System.Collections.Generic;
using System.Globalization;
using DiegoG.MonoGame.Extended;
using DiegoG.RemoteHUD;
using DiegoG.RemoteHud.DTO;
using DiegoG.RemoteHud.HudManagers;
using ImGuiNET;
using MessagePack;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace DiegoG.RemoteHud.HudElements;

[HudElement]
public class LortressianDateTimeElement(HudManager manager) : HudItem(manager), IDebugExplorable
{
    private DateTime date;
    private bool showNumericTime;
    private bool showDate;
    private bool hasChanged;
    
    public DateTime Date
    {
        get => date;
        set => date = value;
    }

    public bool ShowNumericTime
    {
        get => showNumericTime;
        set => showNumericTime = value;
    }

    public bool ShowDate
    {
        get => showDate;
        set => showDate = value;
    }

    protected override bool HasChanged => hasChanged;

    protected override IHudItemState? GetSerializableState()
        => new ItemState()
        {
            ShowDate = ShowDate,
            ShowNumericTime = ShowNumericTime,
            Date = Date,
            Position = Position
        };

    protected override void SetDeserializedState(IHudItemState? state)
    {
        if (state is null or not ItemState) return;
        SetState((ItemState)state);
    }

    public override void ProcessCommand(IHudItemState? state)
    {
        if (state is null or not ItemState) return;
        SetState((ItemState)state);
    }

    private void SetState(ItemState state)
    {
        Position = state.Position;
        Date = state.Date;
        ShowNumericTime = state.ShowNumericTime;
        ShowDate = state.ShowDate;
    }

    public void RenderImGuiDebug()
    {
        hasChanged = ImGui.Checkbox("Has Changed", ref hasChanged);
        hasChanged = ImGui.Checkbox("Show Date", ref showDate);
        hasChanged = ImGui.Checkbox("Show Numeric Time", ref showNumericTime);
        hasChanged = ImGuiHelpers.LortressianDate("Date and Time", ref date);
    }

    [MessagePackObject]
    public class ItemState : IHudItemState
    {
        [Key(0)]
        public bool ShowDate { get; set; }
        
        [Key(1)]
        public bool ShowNumericTime { get; set; }
        
        [Key(2)]
        public DateTime Date { get; set; }

        [Key(3)]
        public Vector2 Position { get; set; }
    }
}