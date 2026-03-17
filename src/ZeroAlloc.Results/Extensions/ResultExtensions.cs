namespace ZeroAlloc.Results.Extensions;

/// <summary>
/// Combinator extension methods for all Result types.
/// All methods are zero-allocation — they return structs and never heap-allocate.
/// </summary>
public static class ResultExtensions
{
    // ── Map ──────────────────────────────────────────────────────────────

    /// <summary>Transforms the success value. Passes failure through unchanged.</summary>
    public static Result<U, E> Map<T, U, E>(
        this Result<T, E> result,
        Func<T, U> map) =>
        result.IsSuccess
            ? Result<U, E>.Success(map(result.Value))
            : Result<U, E>.Failure(result.Error);

    /// <summary>Transforms the success value. Passes failure through unchanged.</summary>
    public static Result<U> Map<T, U>(
        this Result<T> result,
        Func<T, U> map) =>
        result.IsSuccess
            ? Result<U>.Success(map(result.Value))
            : Result<U>.Failure(result.Error);

    // ── MapError ─────────────────────────────────────────────────────────

    /// <summary>Transforms the error value. Passes success through unchanged.</summary>
    public static Result<T, F> MapError<T, E, F>(
        this Result<T, E> result,
        Func<E, F> mapError) =>
        result.IsSuccess
            ? Result<T, F>.Success(result.Value)
            : Result<T, F>.Failure(mapError(result.Error));

    // ── Bind ─────────────────────────────────────────────────────────────

    /// <summary>Chains a result-returning function on success. Short-circuits on failure.</summary>
    public static Result<U, E> Bind<T, U, E>(
        this Result<T, E> result,
        Func<T, Result<U, E>> bind) =>
        result.IsSuccess
            ? bind(result.Value)
            : Result<U, E>.Failure(result.Error);

    /// <summary>Chains a result-returning function on success. Short-circuits on failure.</summary>
    public static Result<U> Bind<T, U>(
        this Result<T> result,
        Func<T, Result<U>> bind) =>
        result.IsSuccess
            ? bind(result.Value)
            : Result<U>.Failure(result.Error);

    // ── Match ─────────────────────────────────────────────────────────────

    /// <summary>Extracts a value by applying one of two functions depending on success/failure.</summary>
    public static U Match<T, E, U>(
        this Result<T, E> result,
        Func<T, U> onSuccess,
        Func<E, U> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

    /// <summary>Extracts a value by applying one of two functions depending on success/failure.</summary>
    public static U Match<T, U>(
        this Result<T> result,
        Func<T, U> onSuccess,
        Func<string, U> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

    // ── Tap ───────────────────────────────────────────────────────────────

    /// <summary>Executes a side-effect on success without transforming the result.</summary>
    public static Result<T, E> Tap<T, E>(
        this Result<T, E> result,
        Action<T> action)
    {
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    /// <summary>Executes a side-effect on failure without transforming the result.</summary>
    public static Result<T, E> TapError<T, E>(
        this Result<T, E> result,
        Action<E> action)
    {
        if (result.IsFailure) action(result.Error);
        return result;
    }

    // ── Ensure ────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts a successful result to failure if <paramref name="predicate"/> returns false.
    /// Passes failure through unchanged.
    /// </summary>
    public static Result<T, E> Ensure<T, E>(
        this Result<T, E> result,
        Func<T, bool> predicate,
        E error) =>
        result.IsSuccess
            ? predicate(result.Value) ? result : Result<T, E>.Failure(error)
            : result;

    // ── Combine ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns Success if all results succeed, otherwise returns the first failure.
    /// Accepts <see cref="ReadOnlySpan{T}"/> — pass a stack-local Span to keep the call zero-alloc.
    /// </summary>
    public static UnitResult<E> Combine<T, E>(ReadOnlySpan<Result<T, E>> results)
    {
        foreach (ref readonly var result in results)
        {
            if (result.IsFailure)
                return UnitResult<E>.Failure(result.Error);
        }
        return UnitResult<E>.Success();
    }
}
