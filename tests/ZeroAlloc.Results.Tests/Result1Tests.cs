using Xunit;
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class Result1Tests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = Result<int>.Failure("went wrong");
        Assert.True(result.IsFailure);
        Assert.Equal("went wrong", result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromValue()
    {
        Result<int> result = 99;
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        Result<int> result = "bad";
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(Result<int>).IsValueType);
    }

    [Fact]
    public void ToString_Success_ReturnsSuccessWithValue()
    {
        Assert.Equal("Success(42)", Result<int>.Success(42).ToString());
    }

    [Fact]
    public void ToString_Failure_ReturnsFailureWithError()
    {
        Assert.Equal("Failure(bad)", Result<int>.Failure("bad").ToString());
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _ = Result<int>.Failure("err").Value);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _ = Result<int>.Success(1).Error);
    }
}
