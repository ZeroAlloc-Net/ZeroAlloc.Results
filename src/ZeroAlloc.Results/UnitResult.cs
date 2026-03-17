namespace ZeroAlloc.Results;

/// <summary>
/// A zero-allocation result with no success value and a typed error of <typeparamref name="E"/>.
/// Use when an operation either succeeds (with no return value) or fails with a typed error.
/// </summary>
public readonly struct UnitResult<E>
{
    private readonly bool _isSuccess;
    private readonly E _error;

    private UnitResult(bool isSuccess, E error)
    {
        _isSuccess = isSuccess;
        _error = error;
    }

    /// <summary>Gets a value indicating whether the result represents success.</summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>Gets a value indicating whether the result represents failure.</summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>Gets the typed error. Throws <see cref="InvalidOperationException"/> if the result is successful.</summary>
    public E Error => !_isSuccess
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful UnitResult.");

    /// <summary>Creates a successful unit result.</summary>
    public static UnitResult<E> Success() => new(true, default!);

    /// <summary>Creates a failed unit result with the given typed error.</summary>
    public static UnitResult<E> Failure(E error) => new(false, error);

    /// <summary>Implicitly wraps a typed error as a failed unit result.</summary>
    public static implicit operator UnitResult<E>(E error) => Failure(error);

    /// <inheritdoc/>
    public override string ToString() => _isSuccess ? "Success" : $"Failure({_error})";
}
