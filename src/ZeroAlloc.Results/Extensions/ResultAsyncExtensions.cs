namespace ZeroAlloc.Results.Extensions;

/// <summary>
/// Async combinator extension methods for Result types using <see cref="ValueTask"/>.
/// The Result structs themselves remain zero-allocation; only the async state machine allocates.
/// </summary>
public static class ResultAsyncExtensions
{
    // ── MapAsync ──────────────────────────────────────────────────────────

    /// <summary>Asynchronously transforms the success value. Passes failure through unchanged.</summary>
    public static async ValueTask<Result<U, E>> MapAsync<T, U, E>(
        this Result<T, E> result,
        Func<T, ValueTask<U>> map) =>
        result.IsSuccess
            ? Result<U, E>.Success(await map(result.Value).ConfigureAwait(false))
            : Result<U, E>.Failure(result.Error);

    // ── BindAsync ─────────────────────────────────────────────────────────

    /// <summary>Asynchronously chains a result-returning function on success.</summary>
    public static async ValueTask<Result<U, E>> BindAsync<T, U, E>(
        this Result<T, E> result,
        Func<T, ValueTask<Result<U, E>>> bind) =>
        result.IsSuccess
            ? await bind(result.Value).ConfigureAwait(false)
            : Result<U, E>.Failure(result.Error);

    // ── TapAsync ──────────────────────────────────────────────────────────

    /// <summary>Asynchronously executes a side-effect on success without transforming the result.</summary>
    public static async ValueTask<Result<T, E>> TapAsync<T, E>(
        this Result<T, E> result,
        Func<T, ValueTask> action)
    {
        if (result.IsSuccess)
            await action(result.Value).ConfigureAwait(false);
        return result;
    }

    // ── TapErrorAsync ─────────────────────────────────────────────────────

    /// <summary>Asynchronously executes a side-effect on failure without transforming the result.</summary>
    public static async ValueTask<Result<T, E>> TapErrorAsync<T, E>(
        this Result<T, E> result,
        Func<E, ValueTask> action)
    {
        if (result.IsFailure)
            await action(result.Error).ConfigureAwait(false);
        return result;
    }

    // ── MatchAsync ────────────────────────────────────────────────────────

    /// <summary>Asynchronously extracts a value by applying one of two functions.</summary>
    public static async ValueTask<U> MatchAsync<T, E, U>(
        this Result<T, E> result,
        Func<T, ValueTask<U>> onSuccess,
        Func<E, ValueTask<U>> onFailure) =>
        result.IsSuccess
            ? await onSuccess(result.Value).ConfigureAwait(false)
            : await onFailure(result.Error).ConfigureAwait(false);

    // ── Continuation overloads on ValueTask<Result<T,E>> ─────────────────

    /// <summary>Chains Map onto an already-async result pipeline.</summary>
    public static async ValueTask<Result<U, E>> Map<T, U, E>(
        this ValueTask<Result<T, E>> resultTask,
        Func<T, U> map)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(map);
    }

    /// <summary>Chains Bind onto an already-async result pipeline.</summary>
    public static async ValueTask<Result<U, E>> Bind<T, U, E>(
        this ValueTask<Result<T, E>> resultTask,
        Func<T, Result<U, E>> bind)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(bind);
    }
}
