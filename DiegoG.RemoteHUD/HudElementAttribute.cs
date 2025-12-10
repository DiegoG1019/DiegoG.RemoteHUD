using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace DiegoG.RemoteHud;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class HudElementAttribute : Attribute;

public static class HudElementStore
{
    private static readonly FrozenDictionary<long, HudElementInfo> elements = [];

    public readonly record struct HudElementInfo(long ElementId, Type ElementType, string Name);

    static HudElementStore()
    {
        foreach (var (type, attr) in GLV.Shared.Common.AssemblyInfo.GetTypesWithAttributeFromAssemblies<HudElementAttribute>(
                     AppDomain.CurrentDomain.GetAssemblies().ToHashSet()
                 ))
        {
            if (type.IsAssignableTo(typeof(HudItem)) is false)
                throw new InvalidOperationException($"Type '{type.Name}' cannot be decorated with HudElementAttribute because it is not a sub class of 'HudItem'");

            if (type.IsAbstract)
                throw new InvalidOperationException($"Type '{type.Name}' cannot be decorated with HudElementAttribute because it is an abstract class");
            
            if (type is { IsGenericType: true, IsConstructedGenericType: false })
                throw new InvalidOperationException($"Type '{type.Name}' cannot be decorated with HudElementAttribute because it is an open generic type");
        }
    }
}