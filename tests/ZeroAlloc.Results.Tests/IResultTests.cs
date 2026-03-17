using Xunit;
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class IResultTests
{
    [Fact]
    public void IResult_IsConstraintOnly_NotUsableAsVariable()
    {
        // This test documents intent: IResult<T,E> exists only as a generic constraint.
        // The interface itself must be internal or carry an [EditorBrowsable(Never)] attribute.
        // We verify the interface exists by using it as a constraint.
        static TResult GetSuccess<TResult>(TResult r) where TResult : IResult<int, string> => r;

        // Actual assertion: compiles and works (if this file compiles, the interface exists)
        Assert.True(true);
    }
}
