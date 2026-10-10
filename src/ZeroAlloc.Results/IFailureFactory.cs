using System.ComponentModel;

namespace ZeroAlloc.Results;

/// <summary>
/// Builds a failed result of type <typeparamref name="TSelf"/> from an error of type <typeparamref name="E"/>.
/// Lets generic code, such as a pipeline behaviour whose response type is a type parameter, return a
/// failure without reflection, which keeps it trimming and Native AOT safe.
/// WARNING: Never use as a variable or return type — doing so boxes the struct onto the heap.
/// Check for it once per closed type and cache the delegate:
/// <c>default(TResult) is IFailureFactory&lt;TResult, TError&gt; f ? f.CreateFailure : null</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IFailureFactory<TSelf, E>
{
    /// <summary>Creates a failed <typeparamref name="TSelf"/> containing <paramref name="error"/>.</summary>
    /// <param name="error">The error the failed result carries.</param>
    /// <remarks>
    /// This ignores the instance it is called on; the result depends only on <paramref name="error"/>.
    /// Both type parameters of the interface are invariant, so the error type must match exactly.
    /// </remarks>
    TSelf CreateFailure(E error);
}
