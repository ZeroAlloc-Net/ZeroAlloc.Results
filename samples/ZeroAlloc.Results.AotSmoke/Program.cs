using System;
using ZeroAlloc.Results;

// Exercise every public Result / Maybe primitive the library ships under
// PublishAot=true. Nothing here uses reflection, so if ILC complains or the
// binary crashes it means we leaked a reflection call into the hot path —
// this smoke test fails the build loudly.

// 1. Result (non-generic)
var ok = Result.Success();
var err = Result.Failure("boom");
if (!ok.IsSuccess) return Fail("Result.Success.IsSuccess should be true");
if (!err.IsFailure) return Fail("Result.Failure.IsFailure should be true");
if (!string.Equals(err.Error, "boom", StringComparison.Ordinal))
    return Fail($"Result.Failure.Error expected 'boom', got '{err.Error}'");

// 2. Result<T>
var r1 = Result<int>.Success(42);
var r1Err = Result<int>.Failure("no");
if (r1.Value != 42) return Fail($"Result<int>.Value expected 42, got {r1.Value}");
if (!r1Err.IsFailure) return Fail("Result<int>.Failure.IsFailure should be true");

// 3. Result<T, E>
var r2 = Result<int, string>.Success(7);
var r2Err = Result<int, string>.Failure("nope");
if (r2.Value != 7) return Fail($"Result<int,string>.Value expected 7, got {r2.Value}");
if (!string.Equals(r2Err.Error, "nope", StringComparison.Ordinal))
    return Fail($"Result<int,string>.Error expected 'nope', got '{r2Err.Error}'");

// 4. UnitResult<E>
var u = UnitResult<string>.Success();
var uErr = UnitResult<string>.Failure("broken");
if (!u.IsSuccess) return Fail("UnitResult<string>.Success.IsSuccess should be true");
if (!string.Equals(uErr.Error, "broken", StringComparison.Ordinal))
    return Fail($"UnitResult<string>.Error expected 'broken', got '{uErr.Error}'");

// 5. Maybe<T>
var some = Maybe<int>.Some(99);
var none = Maybe<int>.None;
if (!some.HasValue) return Fail("Maybe.Some.HasValue should be true");
if (some.Value != 99) return Fail($"Maybe.Some.Value expected 99, got {some.Value}");
if (!none.HasNoValue) return Fail("Maybe.None.HasNoValue should be true");
if (none.GetValueOrDefault(-1) != -1)
    return Fail("Maybe.None.GetValueOrDefault did not return fallback");

// 6. Implicit conversion: any T → Maybe<T>.Some(T)
Maybe<int> implicitSome = 123;
if (!implicitSome.HasValue || implicitSome.Value != 123)
    return Fail("Implicit T → Maybe<T> conversion broken");

Console.WriteLine("AOT smoke: PASS");
return 0;

static int Fail(string message)
{
    Console.Error.WriteLine($"AOT smoke: FAIL — {message}");
    return 1;
}
