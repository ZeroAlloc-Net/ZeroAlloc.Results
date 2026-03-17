using Xunit;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultExtensionsMapBindTests
{
    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int, string>.Success(5).Map(x => x * 2);
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void Map_OnFailure_PassesThroughError()
    {
        var result = Result<int, string>.Failure("err").Map(x => x * 2);
        Assert.True(result.IsFailure);
        Assert.Equal("err", result.Error);
    }

    [Fact]
    public void MapError_OnFailure_TransformsError()
    {
        var result = Result<int, string>.Failure("err").MapError(e => e.Length);
        Assert.True(result.IsFailure);
        Assert.Equal(3, result.Error);
    }

    [Fact]
    public void MapError_OnSuccess_PassesThroughValue()
    {
        var result = Result<int, string>.Success(42).MapError(e => e.Length);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Bind_OnSuccess_ChainsResult()
    {
        var result = Result<int, string>.Success(5)
            .Bind(x => x > 0
                ? Result<string, string>.Success("positive")
                : Result<string, string>.Failure("not positive"));
        Assert.True(result.IsSuccess);
        Assert.Equal("positive", result.Value);
    }

    [Fact]
    public void Bind_OnFailure_SkipsFunction()
    {
        var called = false;
        var result = Result<int, string>.Failure("err")
            .Bind(x => { called = true; return Result<string, string>.Success("x"); });
        Assert.False(called);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Map_Result1_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Success(5).Map(x => x * 2);
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void Map_Result1_OnFailure_PassesThroughError()
    {
        var result = Result<int>.Failure("err").Map(x => x * 2);
        Assert.True(result.IsFailure);
        Assert.Equal("err", result.Error);
    }

    [Fact]
    public void Bind_Result1_OnSuccess_ChainsResult()
    {
        var result = Result<int>.Success(5)
            .Bind(x => Result<string>.Success(x.ToString()));
        Assert.True(result.IsSuccess);
        Assert.Equal("5", result.Value);
    }

    [Fact]
    public void Bind_Result1_OnFailure_SkipsFunction()
    {
        var called = false;
        Result<int>.Failure("err")
            .Bind(x => { called = true; return Result<string>.Success("x"); });
        Assert.False(called);
    }
}
