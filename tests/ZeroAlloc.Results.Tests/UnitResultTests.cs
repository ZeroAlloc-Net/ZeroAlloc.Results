using Xunit;
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class UnitResultTests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = UnitResult<string>.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = UnitResult<string>.Failure("typed error");
        Assert.True(result.IsFailure);
        Assert.Equal("typed error", result.Error);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        var result = UnitResult<string>.Success();
        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(UnitResult<string>).IsValueType);
    }
}
