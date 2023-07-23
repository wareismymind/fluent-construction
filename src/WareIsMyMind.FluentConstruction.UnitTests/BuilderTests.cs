namespace WareIsMyMind.FluentConstruction.UnitTests;

public class BuilderTests
{
    public class Constructor
    {
        [Fact]
        public void TypeHasNoDefaultCtor_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => new Builder<ClassWithNoDefaultCtor>());
        }

        [Fact]
        public void TypeHasDefaultCtor_Constructs()
        {
            _ = new Builder<ClassWithDefaultCtor>();
        }
    }

    public class Set
    {
        [Fact]
        public void SelectorIsNotAPropertyExpression_Throws()
        {
            var underTest = new Builder<ClassWithMethod>();

            Assert.Throws<InvalidOperationException>(() => underTest.Set(x => x.Method(x), "wat"));
        }

        [Fact]
        public void SelectorIsAPropertyExpression_DoesNotThrow()
        {
            var underTest = new Builder<ClassWithNoRequiredProperties>();

            underTest.Set(x => x.Optional, "optional");
        }
    }

    public class Build
    {
        [Fact]
        public void PropertiesWereSet_PropertiesHaveExpectedValues()
        {
            var builder = new Builder<ClassWithNoRequiredProperties>();
            builder.Set(x => x.Optional, "optional value");

            var underTest = builder.Build();

            Assert.Equal("optional value", underTest.Optional);
        }

        [Fact]
        public void PropertiesWereNotSet_PropertiesHaveDefaultValues()
        {
            var builder = new Builder<ClassWithNoRequiredProperties>();

            var underTest = builder.Build();

            Assert.Equal("default value", underTest.WithDefault);
        }

        [Fact]
        public void NonNullablePropertyIsNull_Throws()
        {
            var builder = new Builder<ClassWithNonNullableProperty>();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void RequiredPropertyNotSet_Throws()
        {
            var builder = new Builder<ClassWithRequiredProperty>();

            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }
    }

    public class ClassWithNoDefaultCtor
    {
        public ClassWithNoDefaultCtor(object _) { }
    }

    public class ClassWithDefaultCtor { }

    public class ClassWithMethod
    {
        public string Method(ClassWithMethod _) => string.Empty;
    }

    public class ClassWithNoRequiredProperties
    {
        public string? Optional { get; init; }

        public string WithDefault { get; init; } = "default value";
    }

    public class ClassWithNonNullableProperty
    {
        public string NonNullable { get; init; }
    }

    public class ClassWithRequiredProperty
    {
        public required string Required { get; init; }
    }
}