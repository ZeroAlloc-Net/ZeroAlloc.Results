using Xunit;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultExtensionsEnsureCombineTests
{
    [Fact]
    public void Ensure_PredicateTrue_ReturnsSuccess()
    {
        var result = Result<int, string>.Success(10).Ensure(x => x > 0, "must be positive");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Ensure_PredicateFalse_ReturnsFailure()
    {
        var result = Result<int, string>.Success(-1).Ensure(x => x > 0, "must be positive");
        Assert.True(result.IsFailure);
        Assert.Equal("must be positive", result.Error);
    }

    [Fact]
    public void Ensure_OnFailure_SkipsPredicate()
    {
        var called = false;
        var result = Result<int, string>.Failure("original")
            .Ensure(x => { called = true; return true; }, "irrelevant");
        Assert.False(called);
        Assert.Equal("original", result.Error);
    }

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        Span<Result<int, string>> results =
        [
            Result<int, string>.Success(1),
            Result<int, string>.Success(2),
            Result<int, string>.Success(3)
        ];
        var combined = ResultExtensions.Combine<int, string>(results);
        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void Combine_AnyFailure_ReturnsFirstFailure()
    {
        Span<Result<int, string>> results =
        [
            Result<int, string>.Success(1),
            Result<int, string>.Failure("first error"),
            Result<int, string>.Failure("second error")
        ];
        var combined = ResultExtensions.Combine<int, string>(results);
        Assert.True(combined.IsFailure);
        Assert.Equal("first error", combined.Error);
    }

    [Fact]
    public void Combine_EmptySpan_ReturnsSuccess()
    {
        var combined = ResultExtensions.Combine<int, string>(ReadOnlySpan<Result<int, string>>.Empty);
        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void Combine_SingleSuccess_ReturnsSuccess()
    {
        Span<Result<int, string>> results = [Result<int, string>.Success(1)];
        var combined = ResultExtensions.Combine<int, string>(results);
        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void Combine_SingleFailure_ReturnsFailure()
    {
        Span<Result<int, string>> results = [Result<int, string>.Failure("only error")];
        var combined = ResultExtensions.Combine<int, string>(results);
        Assert.True(combined.IsFailure);
        Assert.Equal("only error", combined.Error);
    }
}
