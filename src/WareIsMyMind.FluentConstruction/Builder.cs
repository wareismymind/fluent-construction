using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WareIsMyMind.FluentConstruction;

public sealed class Builder<T>
{
    private static readonly NullabilityInfoContext _nullabilityContext = new();

    private Func<T> _factory;

    private readonly Dictionary<string, Action<T>> _assignments = new();

    public Builder()
    {
        if (typeof(T).GetConstructor(Array.Empty<Type>()) is not ConstructorInfo ctor)
        {
            throw new InvalidOperationException($"The type '{typeof(T).Name}' has no default constructor.");
        }

        _factory = () => (T)ctor.Invoke(Array.Empty<object>());
    }

    public Builder<T> Set<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value)
    {
        var property = GetProperty(propertySelector);
        _assignments[property.Name] = x => property.SetValue(x, value);
        return this;
    }

    public T Build()
    {
        var instance = _factory();

        foreach (var property in typeof(T).GetProperties())
        {
            if (_assignments.TryGetValue(property.Name, out var assignment))
            {
                assignment(instance);
                continue;
            }

            if (Attribute.IsDefined(property, typeof(RequiredMemberAttribute)))
            {
                throw new InvalidOperationException($"Required property '{property.Name}' was not set.");
            }

            if (property.GetValue(instance) is not null)
            {
                continue;
            }

            var nullable = _nullabilityContext.Create(property);
            if (nullable.ReadState == NullabilityState.NotNull || nullable.WriteState == NullabilityState.NotNull)
            {
                throw new InvalidOperationException($"Non-nullable property '{property.Name}' is null.");
            }
        }

        return instance;
    }


    private static PropertyInfo GetProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression) =>
        (propertyExpression.Body as MemberExpression)?.Member as PropertyInfo
            ?? throw new InvalidOperationException(
                $"'{propertyExpression}' is not a property expression of {typeof(T).Name}.");   
}
