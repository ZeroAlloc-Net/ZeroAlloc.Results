using Xunit;
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class FailureFactoryTests
{
    // The same one-time generic check a consumer such as a pipeline behaviour performs.
    private static Func<TError, TResult>? FactoryFor<TResult, TError>() =>
        default(TResult) is IFailureFactory<TResult, TError> factory ? factory.CreateFailure : null;

    [Fact]
    public void ResultTE_CreatesFailureWithError()
    {
        var create = FactoryFor<Result<int, string>, string>();

        Assert.NotNull(create);
        var result = create("boom");
        Assert.True(result.IsFailure);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public void ResultT_CreatesFailureWithError()
    {
        var create = FactoryFor<Result<int>, string>();

        Assert.NotNull(create);
        var result = create("boom");
        Assert.True(result.IsFailure);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public void UnitResult_CreatesFailureWithError()
    {
        var create = FactoryFor<UnitResult<string>, string>();

        Assert.NotNull(create);
        var result = create("boom");
        Assert.True(result.IsFailure);
        Assert.Equal("boom", result.Error);
    }

    [Fact]
    public void MismatchedErrorType_HasNoFactory()
    {
        Assert.Null(FactoryFor<Result<int, string>, object>());
        Assert.Null(FactoryFor<UnitResult<string>, int>());
    }

    [Fact]
    public void ReferenceTypeResponse_HasNoFactory()
    {
        Assert.Null(FactoryFor<string, string>());
    }

    [Fact]
    public void CreateFailure_IsNotAPublicMemberOfTheStructs()
    {
        Assert.Null(typeof(Result<int, string>).GetMethod("CreateFailure"));
        Assert.Null(typeof(Result<int>).GetMethod("CreateFailure"));
        Assert.Null(typeof(UnitResult<string>).GetMethod("CreateFailure"));
    }
}
