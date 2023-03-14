// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
///     A custom <see cref="ServiceDescriptor" /> that supports unique checking
/// </summary>
/// <remarks>
///     This is required because the default implementation doesn't implement Equals or GetHashCode.
///     see: https://github.com/dotnet/runtime/issues/47262
/// </remarks>
public sealed class UniqueServiceDescriptor : ServiceDescriptor, IEquatable<UniqueServiceDescriptor>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UniqueServiceDescriptor" /> class.
    /// </summary>
    public UniqueServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        : base(serviceType, implementationType, lifetime)
    {
    }

    /// <inheritdoc />
    public bool Equals(UniqueServiceDescriptor? other) => other != null && Lifetime == other.Lifetime &&
                                                          EqualityComparer<Type>.Default.Equals(
                                                              ServiceType,
                                                              other.ServiceType) &&
                                                          EqualityComparer<Type?>.Default.Equals(
                                                              ImplementationType,
                                                              other.ImplementationType) &&
                                                          EqualityComparer<object?>.Default.Equals(
                                                              ImplementationInstance, other.ImplementationInstance) &&
                                                          EqualityComparer<Func<IServiceProvider, object>?>.Default
                                                              .Equals(
                                                                  ImplementationFactory,
                                                                  other.ImplementationFactory);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as UniqueServiceDescriptor);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = 493849952;
        hashCode = (hashCode * -1521134295) + Lifetime.GetHashCode();
        hashCode = (hashCode * -1521134295) + EqualityComparer<Type>.Default.GetHashCode(ServiceType);

        if (ImplementationType is not null)
        {
            hashCode = (hashCode * -1521134295) + EqualityComparer<Type?>.Default.GetHashCode(ImplementationType);
        }

        if (ImplementationInstance is not null)
        {
            hashCode = (hashCode * -1521134295) + EqualityComparer<object?>.Default.GetHashCode(ImplementationInstance);
        }

        if (ImplementationFactory is not null)
        {
            hashCode = (hashCode * -1521134295) +
                       EqualityComparer<Func<IServiceProvider, object>?>.Default.GetHashCode(ImplementationFactory);
        }

        return hashCode;
    }
}
