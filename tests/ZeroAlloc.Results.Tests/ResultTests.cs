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

    [Fact]
    public void Success_IsFailure_False()
    {
        var result = Result.Success();
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        var result = Result.Success();
        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void ToString_Success_ReturnsSuccess()
    {
        Assert.Equal("Success", Result.Success().ToString());
    }

    [Fact]
    public void ToString_Failure_ReturnsFailureWithMessage()
    {
        Assert.Equal("Failure(oops)", Result.Failure("oops").ToString());
    }
}
