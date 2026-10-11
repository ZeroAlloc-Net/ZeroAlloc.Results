using System;
using System.Threading.Tasks;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;
using ZeroAlloc.TestHelpers;

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

// 4b. IFailureFactory — the generic, reflection-free failure construction pipeline behaviours use
if (!FailureVia<Result<int, string>, string>("f1", out var f1) || !string.Equals(f1.Error, "f1", StringComparison.Ordinal))
    return Fail("IFailureFactory on Result<int,string> did not build a failure");
if (!FailureVia<Result<int>, string>("f2", out var f2) || !string.Equals(f2.Error, "f2", StringComparison.Ordinal))
    return Fail("IFailureFactory on Result<int> did not build a failure");
if (!FailureVia<UnitResult<string>, string>("f3", out var f3) || !string.Equals(f3.Error, "f3", StringComparison.Ordinal))
    return Fail("IFailureFactory on UnitResult<string> did not build a failure");

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

// 7. Combinators, LINQ and the async extensions under AOT. Behaviour first, then the
//    allocation gate: the README claims 0 B for every combinator, and the async ones are 0 B
//    when each awaited ValueTask has already completed. Lambdas are static so the delegate
//    is cached and any byte counted comes from the library.
var five = Result<int, string>.Success(5);
var bad = Result<int, string>.Failure("bad");
if (five.Map(static x => x * 2).Value != 10) return Fail("Map");
if (!bad.Map(static x => x * 2).IsFailure) return Fail("Map on failure");
if (bad.MapError(static e => e.Length).Error != 3) return Fail("MapError");
if (five.Bind(static x => Result<int, string>.Success(x + 1)).Value != 6) return Fail("Bind");
if (five.Match(static v => v, static e => -1) != 5) return Fail("Match");
if (bad.Match(static v => v, static e => e.Length) != 3) return Fail("Match on failure");
if (!five.Tap(static _ => { }).IsSuccess) return Fail("Tap");
if (!bad.TapError(static _ => { }).IsFailure) return Fail("TapError");
if (!five.Ensure(static v => v < 0, "neg").IsFailure) return Fail("Ensure");
if (!ResultExtensions.Combine<int, string>([five, five]).IsSuccess) return Fail("Combine");
if (!string.Equals(ResultExtensions.Combine<int, string>([five, bad]).Error, "bad", StringComparison.Ordinal))
    return Fail("Combine on failure");
var linq = from x in five
           from y in Result<int, string>.Success(x + 1)
           select x + y;
if (linq.Value != 11) return Fail($"LINQ from-from-select expected 11, got {linq.Value}");
if ((from x in five select x * 3).Value != 15) return Fail("LINQ select");
if (Completed(five.MapAsync(static x => new ValueTask<int>(x * 2))).Value != 10) return Fail("MapAsync");
if (Completed(five.BindAsync(static x => new ValueTask<Result<int, string>>(Result<int, string>.Success(x + 1)))).Value != 6)
    return Fail("BindAsync");
if (!Completed(five.TapAsync(static _ => ValueTask.CompletedTask)).IsSuccess) return Fail("TapAsync");
if (!Completed(bad.TapErrorAsync(static _ => ValueTask.CompletedTask)).IsFailure) return Fail("TapErrorAsync");
if (Completed(five.MatchAsync(static v => new ValueTask<int>(v), static e => new ValueTask<int>(-1))) != 5)
    return Fail("MatchAsync");
if (Completed(new ValueTask<Result<int, string>>(five).Map(static x => x * 2)).Value != 10)
    return Fail("ValueTask<Result>.Map");
if (Completed(new ValueTask<Result<int, string>>(five).Bind(static x => Result<int, string>.Success(x + 1))).Value != 6)
    return Fail("ValueTask<Result>.Bind");

const int Iterations = 1000;
var sink = 0;
AllocationGate.AssertBudget(0, Iterations, () => sink += five.Map(static x => x * 2).Value, "Map");
AllocationGate.AssertBudget(0, Iterations, () => _ = bad.MapError(static e => e.Length), "MapError");
AllocationGate.AssertBudget(0, Iterations,
    () => sink += five.Bind(static x => Result<int, string>.Success(x + 1)).Value, "Bind");
AllocationGate.AssertBudget(0, Iterations, () => sink += five.Match(static v => v, static e => e.Length), "Match");
AllocationGate.AssertBudget(0, Iterations, () => _ = five.Tap(static _ => { }), "Tap");
AllocationGate.AssertBudget(0, Iterations, () => _ = bad.TapError(static _ => { }), "TapError");
AllocationGate.AssertBudget(0, Iterations, () => _ = five.Ensure(static v => v > 0, "neg"), "Ensure");
AllocationGate.AssertBudget(0, Iterations, () => _ = ResultExtensions.Combine<int, string>([five, bad, five]), "Combine");
AllocationGate.AssertBudget(0, Iterations, () =>
{
    var r = from x in five
            from y in Result<int, string>.Success(x + 1)
            select x + y;
    sink += r.Value;
}, "LINQ from-from-select");
AllocationGate.AssertBudgetValueTask(0, Iterations, () => five.MapAsync(static x => new ValueTask<int>(x * 2)), "MapAsync");
AllocationGate.AssertBudgetValueTask(0, Iterations,
    () => five.BindAsync(static x => new ValueTask<Result<int, string>>(Result<int, string>.Success(x + 1))), "BindAsync");
AllocationGate.AssertBudgetValueTask(0, Iterations, () => five.TapAsync(static _ => ValueTask.CompletedTask), "TapAsync");
AllocationGate.AssertBudgetValueTask(0, Iterations, () => bad.TapErrorAsync(static _ => ValueTask.CompletedTask), "TapErrorAsync");
AllocationGate.AssertBudgetValueTask(0, Iterations,
    () => five.MatchAsync(static v => new ValueTask<int>(v), static e => new ValueTask<int>(e.Length)), "MatchAsync");
AllocationGate.AssertBudgetValueTask(0, Iterations,
    () => new ValueTask<Result<int, string>>(five).Map(static x => x * 2), "ValueTask<Result>.Map");
AllocationGate.AssertBudgetValueTask(0, Iterations,
    () => new ValueTask<Result<int, string>>(five).Bind(static x => Result<int, string>.Success(x + 1)), "ValueTask<Result>.Bind");
GC.KeepAlive(sink);
Console.WriteLine("AOT combinator allocation gate OK");

Console.WriteLine("AOT smoke: PASS");
return 0;

static int Fail(string message)
{
    Console.Error.WriteLine($"AOT smoke: FAIL — {message}");
    return 1;
}

// The async combinators must not suspend when every awaited ValueTask is already complete.
static T Completed<T>(ValueTask<T> task) =>
    task.IsCompletedSuccessfully
        ? task.Result
        : throw new InvalidOperationException("async combinator did not complete synchronously");

static bool FailureVia<TResult, TError>(TError error, out TResult result)
{
    if (default(TResult) is IFailureFactory<TResult, TError> factory)
    {
        result = factory.CreateFailure(error);
        return true;
    }
    result = default!;
    return false;
}
