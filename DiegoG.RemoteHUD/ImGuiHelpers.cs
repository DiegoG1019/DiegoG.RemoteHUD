using System;
using DiegoG.RemoteHud.HudElements;
using ImGuiNET;

namespace DiegoG.RemoteHUD;

public static class ImGuiHelpers
{
    public static bool LortressianDate(ReadOnlySpan<char> label, ref DateTime datetime)
    {
        var cal = LortressianCalendar.Instance;
        
        int day = cal.GetDayOfMonth(datetime);
        int month = cal.GetMonth(datetime);
        int year = cal.GetYear(datetime);
        int hour = cal.GetHour(datetime);
        int minute = cal.GetMinute(datetime);
        int second = cal.GetSecond(datetime);

        bool mod = false;
        ImGui.Text(label);
        mod = ImGui.SliderInt("", ref day, 1, 30);
        ImGui.SameLine();
        ImGui.Text("/");
        mod = ImGui.SliderInt("", ref month, 1, 12);
        ImGui.SameLine();
        ImGui.Text("/");
        mod = ImGui.InputInt("", ref year, 1, 10);
        
        mod = ImGui.SliderInt("", ref hour, 0, 23);
        ImGui.SameLine();
        ImGui.Text(":");
        mod = ImGui.SliderInt("", ref minute, 0, 59);
        ImGui.SameLine();
        ImGui.Text(":");
        mod = ImGui.SliderInt("", ref second, 0, 59);

        if (mod)
            datetime = cal.ToDateTime(year, month, day, hour, minute, second, 0);

        return mod;
    }
}