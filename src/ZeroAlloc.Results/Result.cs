namespace ZeroAlloc.Results;

/// <summary>
/// A zero-allocation non-generic pass/fail result with an optional string error message.
/// </summary>
public readonly struct Result
{
    private readonly bool _isSuccess;
    private readonly string _error;

    private Result(bool isSuccess, string error)
    {
        _isSuccess = isSuccess;
        _error = error;
    }

    /// <summary>Gets a value indicating whether the result represents success.</summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>Gets a value indicating whether the result represents failure.</summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>Gets the error message. Throws <see cref="InvalidOperationException"/> if the result is successful.</summary>
    public string Error => !_isSuccess
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, string.Empty);

    /// <summary>Creates a failed result with the given error message.</summary>
    public static Result Failure(string error) => new(false, error);

    /// <inheritdoc/>
    public override string ToString() => _isSuccess ? "Success" : $"Failure({_error})";
}
