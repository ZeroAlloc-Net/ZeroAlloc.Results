namespace ZeroAlloc.Results;

/// <summary>
/// A zero-allocation discriminated union representing either a success value of type <typeparamref name="T"/>
/// or a failure value of type <typeparamref name="E"/>.
/// </summary>
public readonly struct Result<T, E> : IResult<T, E>
{
    private readonly bool _isSuccess;
    private readonly T _value;
    private readonly E _error;

    private Result(T value)
    {
        _isSuccess = true;
        _value = value;
        _error = default!;
    }

    private Result(E error)
    {
        _isSuccess = false;
        _value = default!;
        _error = error;
    }

    /// <inheritdoc/>
    public bool IsSuccess => _isSuccess;

    /// <inheritdoc/>
    public bool IsFailure => !_isSuccess;

    /// <inheritdoc/>
    public T Value => _isSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    /// <inheritdoc/>
    public E Error => !_isSuccess
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    /// <summary>Creates a successful result containing <paramref name="value"/>.</summary>
    public static Result<T, E> Success(T value) => new(value);

    /// <summary>Creates a failed result containing <paramref name="error"/>.</summary>
    public static Result<T, E> Failure(E error) => new(error);

    /// <summary>Implicitly wraps a value as a successful result.</summary>
    public static implicit operator Result<T, E>(T value) => Success(value);

    /// <summary>Implicitly wraps an error as a failed result.</summary>
    public static implicit operator Result<T, E>(E error) => Failure(error);

    /// <inheritdoc/>
    public override string ToString() =>
        _isSuccess ? $"Success({_value})" : $"Failure({_error})";
}
