using Xunit;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultLinqExtensionsTests
{
    [Fact]
    public void Select_OnSuccess_TransformsValue()
    {
        var result = from x in Result<int, string>.Success(5)
                     select x * 2;
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void Select_OnFailure_PassesThroughError()
    {
        var result = from x in Result<int, string>.Failure("err")
                     select x * 2;
        Assert.True(result.IsFailure);
        Assert.Equal("err", result.Error);
    }

    [Fact]
    public void SelectMany_BothSuccess_ChainsResult()
    {
        var result =
            from user in Result<string, string>.Success("alice")
            from greeting in Result<string, string>.Success($"Hello, {user}")
            select greeting;
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello, alice", result.Value);
    }

    [Fact]
    public void SelectMany_FirstFailure_ShortCircuits()
    {
        var secondCalled = false;
        Result<string, string> GetGreeting(string user)
        {
            secondCalled = true;
            return Result<string, string>.Success($"Hello, {user}");
        }

        var result =
            from user in Result<string, string>.Failure("no user")
            from greeting in GetGreeting(user)
            select greeting;
        Assert.False(secondCalled);
        Assert.True(result.IsFailure);
        Assert.Equal("no user", result.Error);
    }

    [Fact]
    public void SelectMany_SecondBindFailure_PropagatesError()
    {
        var result =
            from user in Result<string, string>.Success("alice")
            from greeting in Result<string, string>.Failure("greeting failed")
            select greeting;
        Assert.True(result.IsFailure);
        Assert.Equal("greeting failed", result.Error);
    }
}
