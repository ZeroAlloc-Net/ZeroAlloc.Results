using Xunit;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultExtensionsMatchTapTests
{
    [Fact]
    public void Match_OnSuccess_InvokesOnSuccess()
    {
        var result = Result<int, string>.Success(42).Match(v => v * 2, _ => -1);
        Assert.Equal(84, result);
    }

    [Fact]
    public void Match_OnFailure_InvokesOnFailure()
    {
        var result = Result<int, string>.Failure("err").Match(_ => 0, e => e.Length);
        Assert.Equal(3, result);
    }

    [Fact]
    public void Tap_OnSuccess_InvokesSideEffect()
    {
        int sideEffect = 0;
        var result = Result<int, string>.Success(5).Tap(v => sideEffect = v);
        Assert.Equal(5, sideEffect);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Tap_OnFailure_DoesNotInvoke()
    {
        var called = false;
        Result<int, string>.Failure("err").Tap(_ => called = true);
        Assert.False(called);
    }

    [Fact]
    public void TapError_OnFailure_InvokesSideEffect()
    {
        string? captured = null;
        var result = Result<int, string>.Failure("err").TapError(e => captured = e);
        Assert.Equal("err", captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TapError_OnSuccess_DoesNotInvoke()
    {
        var called = false;
        Result<int, string>.Success(1).TapError(_ => called = true);
        Assert.False(called);
    }
}
