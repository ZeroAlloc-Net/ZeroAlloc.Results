namespace ZeroAlloc.Results;

/// <summary>
/// A zero-allocation optional value — equivalent to <c>Option&lt;T&gt;</c> in functional languages.
/// Prefer this over <see cref="Nullable{T}"/> for reference types.
/// </summary>
public readonly struct Maybe<T>
{
    private readonly bool _hasValue;
    private readonly T _value;

    private Maybe(T value)
    {
        _hasValue = true;
        _value = value;
    }

    /// <summary>Gets a value indicating whether this instance contains a value.</summary>
    public bool HasValue => _hasValue;

    /// <summary>Gets a value indicating whether this instance contains no value.</summary>
    public bool HasNoValue => !_hasValue;

    /// <summary>Gets the contained value. Throws <see cref="InvalidOperationException"/> if none.</summary>
    public T Value => _hasValue
        ? _value
        : throw new InvalidOperationException("Maybe has no value.");

    /// <summary>Returns the contained value, or <paramref name="defaultValue"/> if none.</summary>
    public T GetValueOrDefault(T defaultValue = default!) =>
        _hasValue ? _value : defaultValue;

    /// <summary>Creates a <see cref="Maybe{T}"/> containing <paramref name="value"/>.</summary>
    public static Maybe<T> Some(T value) => new(value);

    /// <summary>The empty <see cref="Maybe{T}"/> — no value present.</summary>
    public static readonly Maybe<T> None = default;

    /// <summary>Implicitly wraps a value as <see cref="Some(T)"/>.</summary>
    public static implicit operator Maybe<T>(T value) => Some(value);

    /// <inheritdoc/>
    public override string ToString() => _hasValue ? $"Some({_value})" : "None";
}
