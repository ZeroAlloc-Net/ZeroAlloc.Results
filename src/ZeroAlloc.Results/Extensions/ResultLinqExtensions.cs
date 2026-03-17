namespace ZeroAlloc.Results.Extensions;

/// <summary>
/// LINQ query syntax support for <see cref="Result{T,E}"/>.
/// Enables <c>from x in result select f(x)</c> and <c>from x in r1 from y in r2 select ...</c>.
/// </summary>
public static class ResultLinqExtensions
{
    /// <summary>Enables <c>select</c> in query expressions — equivalent to <see cref="ResultExtensions.Map{T,U,E}"/>.</summary>
    public static Result<U, E> Select<T, U, E>(
        this Result<T, E> result,
        Func<T, U> selector) =>
        result.Map(selector);

    /// <summary>Enables <c>from ... from ...</c> in query expressions — equivalent to <see cref="ResultExtensions.Bind{T,U,E}"/>.</summary>
    public static Result<V, E> SelectMany<T, U, V, E>(
        this Result<T, E> result,
        Func<T, Result<U, E>> bind,
        Func<T, U, V> project)
    {
        if (result.IsFailure)
            return Result<V, E>.Failure(result.Error);

        var bound = bind(result.Value);
        if (bound.IsFailure)
            return Result<V, E>.Failure(bound.Error);

        return Result<V, E>.Success(project(result.Value, bound.Value));
    }
}
