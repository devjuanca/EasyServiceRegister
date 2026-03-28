using EasyServiceRegister.Attributes;
using System;

namespace EasyServiceRegister.Tests.Fixtures
{
    // === Interfaces ===

    public interface ISingletonService
    {
        Guid Id { get; }
    }

    public interface IScopedService
    {
        Guid Id { get; }
    }

    public interface ITransientService
    {
        Guid Id { get; }
    }

    public interface IExplicitInterfaceService
    {
        string Name { get; }
    }

    public interface IKeyedService
    {
        string Key { get; }
    }

    public interface IDuplicateService
    {
        string Value { get; }
    }

    public interface IDecoratedService
    {
        string Execute();
    }

    public interface IGenericService<T>
    {
        T Process(T input);
    }

    public interface IAllInterfacesA { }

    public interface IAllInterfacesB { }

    public interface IFilteredService
    {
        string Value { get; }
    }

    public interface ITryAddService
    {
        string Value { get; }
    }

    public enum ServiceKeyEnum
    {
        Primary,
        Secondary
    }

    // === Singleton Services ===

    [RegisterAsSingleton]
    public class SingletonService : ISingletonService
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    // === Scoped Services ===

    [RegisterAsScoped]
    public class ScopedService : IScopedService
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    // === Transient Services ===

    [RegisterAsTransient]
    public class TransientService : ITransientService
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    // === Explicit Interface Registration ===

    [RegisterAsScoped(serviceInterface: typeof(IExplicitInterfaceService))]
    public class ExplicitInterfaceService : IExplicitInterfaceService, IDisposable
    {
        public string Name => "Explicit";
        public void Dispose() { }
    }

    // === Self Registration (no interface) ===

    [RegisterAsScoped]
    public class SelfRegisteredService
    {
        public string Value => "SelfRegistered";
    }

    // === Keyed Services (string key) ===

    [RegisterAsScopedKeyed("primary")]
    public class PrimaryKeyedService : IKeyedService
    {
        public string Key => "primary";
    }

    [RegisterAsScopedKeyed("secondary")]
    public class SecondaryKeyedService : IKeyedService
    {
        public string Key => "secondary";
    }

    // === Keyed Services (enum key) ===

    [RegisterAsSingletonKeyed(ServiceKeyEnum.Primary)]
    public class EnumPrimaryKeyedService : IKeyedService
    {
        public string Key => "EnumPrimary";
    }

    [RegisterAsSingletonKeyed(ServiceKeyEnum.Secondary)]
    public class EnumSecondaryKeyedService : IKeyedService
    {
        public string Key => "EnumSecondary";
    }

    // === Keyed Transient ===

    [RegisterAsTransientKeyed("transient-key")]
    public class KeyedTransientService : IKeyedService
    {
        public string Key => "transient-key";
    }

    // === Generic Services ===

    [RegisterAsScoped(typeof(IGenericService<>))]
    public class GenericService<T> : IGenericService<T>
    {
        public T Process(T input) => input;
    }

    // === Duplicate Services ===

    [RegisterAsSingleton]
    public class FirstDuplicateService : IDuplicateService
    {
        public string Value => "First";
    }

    [RegisterAsSingleton]
    public class SecondDuplicateService : IDuplicateService
    {
        public string Value => "Second";
    }

    // === Decorator Pattern ===

    [RegisterAsScoped]
    [DecorateWith(typeof(OuterDecorator), order: 0)]
    [DecorateWith(typeof(InnerDecorator), order: 1)]
    public class BaseDecoratedService : IDecoratedService
    {
        public string Execute() => "Base";
    }

    public class InnerDecorator : IDecoratedService
    {
        private readonly IDecoratedService _inner;
        public InnerDecorator(IDecoratedService inner) { _inner = inner; }
        public string Execute() => $"Inner({_inner.Execute()})";
    }

    public class OuterDecorator : IDecoratedService
    {
        private readonly IDecoratedService _inner;
        public OuterDecorator(IDecoratedService inner) { _inner = inner; }
        public string Execute() => $"Outer({_inner.Execute()})";
    }

    // === RegisterAsAllInterfaces ===

    [RegisterAsScoped(registerAsAllInterfaces: true)]
    public class AllInterfacesService : IAllInterfacesA, IAllInterfacesB
    {
    }

    // === TryAdd Services ===

    [RegisterAsSingleton(useTryAddSingleton: true)]
    public class FirstTryAddService : ITryAddService
    {
        public string Value => "First";
    }

    [RegisterAsSingleton(useTryAddSingleton: true)]
    public class SecondTryAddService : ITryAddService
    {
        public string Value => "Second";
    }

    // === Filter Test Services ===

    [RegisterAsScoped]
    public class IncludedFilteredService : IFilteredService
    {
        public string Value => "Included";
    }

    [RegisterAsScoped]
    public class ExcludedFilteredService : IFilteredService
    {
        public string Value => "Excluded";
    }
}
