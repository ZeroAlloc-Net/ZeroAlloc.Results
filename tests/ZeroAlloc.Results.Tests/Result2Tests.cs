using Xunit;
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class Result2Tests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result<int, string>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = Result<int, string>.Failure("error");
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.Error);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result<int, string>.Failure("error");
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        var result = Result<int, string>.Success(42);
        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_IsSuccess()
    {
        Result<int, string> result = 42;
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_IsFailure()
    {
        Result<int, string> result = "error";
        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(Result<int, string>).IsValueType);
    }
}
