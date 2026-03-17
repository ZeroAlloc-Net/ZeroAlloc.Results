namespace ZeroAlloc.Results;

/// <summary>
/// A zero-allocation discriminated union with a success value of type <typeparamref name="T"/>
/// and a fixed <see cref="string"/> error.
/// </summary>
public readonly struct Result<T> : IResult<T, string>
{
    private readonly bool _isSuccess;
    private readonly T _value;
    private readonly string _error;

    private Result(T value)
    {
        _isSuccess = true;
        _value = value;
        _error = string.Empty;
    }

    private Result(string error)
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
    public string Error => !_isSuccess
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    /// <summary>Creates a successful result containing <paramref name="value"/>.</summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>Creates a failed result containing <paramref name="error"/>.</summary>
    public static Result<T> Failure(string error) => new(error);

    /// <summary>Implicitly wraps a value as a successful result.</summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>Implicitly wraps an error string as a failed result.</summary>
    public static implicit operator Result<T>(string error) => Failure(error);

    /// <inheritdoc/>
    public override string ToString() =>
        _isSuccess ? $"Success({_value})" : $"Failure({_error})";
}
