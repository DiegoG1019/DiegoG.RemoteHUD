using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DiegoG.RemoteHud;

public static class HudElementStore
{
    public static FrozenDictionary<long, HudElementInfo> Elements { get; }

    public readonly record struct HudElementInfo(long ElementId, Type ElementType, string Name)
    {
        public HudItem Instantiate(RemoteHudGame game)
        {
            if (ElementType is null) throw new InvalidOperationException("Cannot instantiate a null ElementType (Such as in a default HudElementInfo)");
            return (HudItem)game.GameServices.GetRequiredService(ElementType);
        }
    }

    static HudElementStore()
    {
        var dict = new Dictionary<long, HudElementInfo>();
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

            long netcode = type.NetworkCode;
            dict[netcode] = new HudElementInfo(netcode, type, string.IsNullOrWhiteSpace(attr.Name) ? type.Name : attr.Name);
        }

        Elements = dict.ToFrozenDictionary();
    }
}