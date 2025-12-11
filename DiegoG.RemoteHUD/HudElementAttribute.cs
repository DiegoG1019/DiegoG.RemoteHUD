using System;
using DiegoG.RemoteHud.HudManagers;

namespace DiegoG.RemoteHud;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class HudElementAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
}