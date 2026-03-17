using Xunit;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultAsyncExtensionsTests
{
    [Fact]
    public async Task MapAsync_OnSuccess_TransformsValue()
    {
        var result = await Result<int, string>.Success(5)
            .MapAsync(async x => { await Task.Yield(); return x * 2; });
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public async Task MapAsync_OnFailure_PassesThroughError()
    {
        var result = await Result<int, string>.Failure("err")
            .MapAsync(async x => { await Task.Yield(); return x * 2; });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task BindAsync_OnSuccess_ChainsResult()
    {
        var result = await Result<int, string>.Success(5)
            .BindAsync(async x =>
            {
                await Task.Yield();
                return Result<string, string>.Success(x.ToString());
            });
        Assert.True(result.IsSuccess);
        Assert.Equal("5", result.Value);
    }

    [Fact]
    public async Task TapAsync_OnSuccess_InvokesSideEffect()
    {
        int sideEffect = 0;
        await Result<int, string>.Success(7)
            .TapAsync(async v => { await Task.Yield(); sideEffect = v; });
        Assert.Equal(7, sideEffect);
    }

    [Fact]
    public async Task TapErrorAsync_OnFailure_InvokesSideEffect()
    {
        string? captured = null;
        await Result<int, string>.Failure("err")
            .TapErrorAsync(async e => { await Task.Yield(); captured = e; });
        Assert.Equal("err", captured);
    }

    [Fact]
    public async Task MatchAsync_OnSuccess_InvokesOnSuccess()
    {
        var result = await Result<int, string>.Success(3)
            .MatchAsync(
                async v => { await Task.Yield(); return v * 10; },
                async _ => { await Task.Yield(); return -1; });
        Assert.Equal(30, result);
    }
}
