using EasyServiceRegister.Attributes;

namespace EasyServiceRegister.Tests.Fixtures
{
    // === Missing Dependency ===

    public interface IMissingDep { }

    public interface IHasMissingDep
    {
        string Value { get; }
    }

    [RegisterAsScoped]
    public class ServiceWithMissingDep : IHasMissingDep
    {
        private readonly IMissingDep _dep;
        public ServiceWithMissingDep(IMissingDep dep) { _dep = dep; }
        public string Value => "test";
    }

    // === Lifetime Mismatch: scoped into singleton ===

    public interface IScopedDependency
    {
        string Value { get; }
    }

    [RegisterAsScoped]
    public class ScopedDependency : IScopedDependency
    {
        public string Value => "Scoped";
    }

    [RegisterAsSingleton]
    public class SingletonWithScopedDep : ISingletonWithScopedDep
    {
        private readonly IScopedDependency _dep;
        public SingletonWithScopedDep(IScopedDependency dep) { _dep = dep; }
        public string Value => _dep.Value;
    }

    public interface ISingletonWithScopedDep
    {
        string Value { get; }
    }

    // === Lifetime Mismatch: transient into singleton ===

    public interface ITransientDependency
    {
        string Value { get; }
    }

    [RegisterAsTransient]
    public class TransientDependency : ITransientDependency
    {
        public string Value => "Transient";
    }

    [RegisterAsSingleton]
    public class SingletonWithTransientDep : ISingletonWithTransientDep
    {
        private readonly ITransientDependency _dep;
        public SingletonWithTransientDep(ITransientDependency dep) { _dep = dep; }
        public string Value => _dep.Value;
    }

    public interface ISingletonWithTransientDep
    {
        string Value { get; }
    }

    // === Circular Dependencies ===

    public interface ICircularA { }
    public interface ICircularB { }
    public interface ICircularC { }

    [RegisterAsScoped]
    public class CircularA : ICircularA
    {
        public CircularA(ICircularB b) { }
    }

    [RegisterAsScoped]
    public class CircularB : ICircularB
    {
        public CircularB(ICircularC c) { }
    }

    [RegisterAsScoped]
    public class CircularC : ICircularC
    {
        public CircularC(ICircularA a) { }
    }
}
