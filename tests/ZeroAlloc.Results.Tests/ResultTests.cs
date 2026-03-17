using Xunit;
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = Result.Failure("went wrong");
        Assert.True(result.IsFailure);
        Assert.Equal("went wrong", result.Error);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(Result).IsValueType);
    }
}
