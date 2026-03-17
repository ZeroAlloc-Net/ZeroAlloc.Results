using System.ComponentModel;

namespace ZeroAlloc.Results;

/// <summary>
/// Marker interface for result types.
/// WARNING: Never use as a variable or return type — doing so boxes the struct onto the heap.
/// Only use as a generic type constraint: <c>where TResult : IResult&lt;T, E&gt;</c>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResult<out T, out E>
{
    /// <summary>Gets a value indicating whether the result represents a success.</summary>
    bool IsSuccess { get; }
    /// <summary>Gets a value indicating whether the result represents a failure.</summary>
    bool IsFailure { get; }
    /// <summary>Gets the success value. Only valid when <see cref="IsSuccess"/> is <c>true</c>.</summary>
    T Value { get; }
    /// <summary>Gets the error value. Only valid when <see cref="IsFailure"/> is <c>true</c>.</summary>
    E Error { get; }
}
