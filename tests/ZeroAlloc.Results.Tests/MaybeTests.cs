using Xunit;
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class MaybeTests
{
    [Fact]
    public void Some_HasValue_True()
    {
        var maybe = Maybe<int>.Some(42);
        Assert.True(maybe.HasValue);
        Assert.Equal(42, maybe.Value);
    }

    [Fact]
    public void None_HasValue_False()
    {
        var maybe = Maybe<int>.None;
        Assert.False(maybe.HasValue);
    }

    [Fact]
    public void Value_OnNone_Throws()
    {
        var maybe = Maybe<int>.None;
        Assert.Throws<InvalidOperationException>(() => _ = maybe.Value);
    }

    [Fact]
    public void ImplicitConversion_FromValue()
    {
        Maybe<int> maybe = 42;
        Assert.True(maybe.HasValue);
        Assert.Equal(42, maybe.Value);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(Maybe<int>).IsValueType);
    }

    [Fact]
    public void GetValueOrDefault_OnNone_ReturnsDefault()
    {
        var maybe = Maybe<int>.None;
        Assert.Equal(0, maybe.GetValueOrDefault());
        Assert.Equal(99, maybe.GetValueOrDefault(99));
    }
}
