using Xunit;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;
using ZeroAlloc.TestHelpers;

namespace ZeroAlloc.Results.Tests.Extensions;

/// <summary>
/// Asserts the README / <see cref="ResultExtensions"/> claim that the combinators are 0 B per call.
/// Every lambda here is non-capturing, so the compiler caches the delegate and any byte the gate
/// counts comes from the library, not the call site.
/// </summary>
public class CombinatorAllocationTests
{
    private const int Iterations = 1000;

    private static readonly Result<int, string> Ok = Result<int, string>.Success(5);
    private static readonly Result<int, string> Err = Result<int, string>.Failure("err");
    private static readonly Result<int> OkSingle = Result<int>.Success(5);

    private static int _sink;

    [Fact]
    public void Map_IsZeroAlloc()
    {
        AllocationGate.AssertBudget(0, Iterations, () => _sink = Ok.Map(static x => x * 2).Value, "Map (success)");
        AllocationGate.AssertBudget(0, Iterations, () => _ = Err.Map(static x => x * 2), "Map (failure)");
        AllocationGate.AssertBudget(0, Iterations, () => _sink = OkSingle.Map(static x => x * 2).Value, "Result<T>.Map");
        AllocationGate.AssertBudget(0, Iterations, () => _ = Err.MapError(static e => e.Length), "MapError");
    }

    [Fact]
    public void Bind_IsZeroAlloc()
    {
        AllocationGate.AssertBudget(0, Iterations,
            () => _sink = Ok.Bind(static x => Result<int, string>.Success(x + 1)).Value, "Bind (success)");
        AllocationGate.AssertBudget(0, Iterations,
            () => _ = Err.Bind(static x => Result<int, string>.Success(x + 1)), "Bind (failure)");
        AllocationGate.AssertBudget(0, Iterations,
            () => _sink = OkSingle.Bind(static x => Result<int>.Success(x + 1)).Value, "Result<T>.Bind");
    }

    [Fact]
    public void Match_IsZeroAlloc()
    {
        AllocationGate.AssertBudget(0, Iterations,
            () => _sink = Ok.Match(static v => v, static e => e.Length), "Match (success)");
        AllocationGate.AssertBudget(0, Iterations,
            () => _sink = Err.Match(static v => v, static e => e.Length), "Match (failure)");
        AllocationGate.AssertBudget(0, Iterations,
            () => _sink = OkSingle.Match(static v => v, static e => e.Length), "Result<T>.Match");
    }

    [Fact]
    public void Tap_IsZeroAlloc()
    {
        AllocationGate.AssertBudget(0, Iterations, () => _ = Ok.Tap(static v => _sink = v), "Tap");
        AllocationGate.AssertBudget(0, Iterations, () => _ = Err.TapError(static e => _sink = e.Length), "TapError");
    }

    [Fact]
    public void Ensure_IsZeroAlloc()
    {
        AllocationGate.AssertBudget(0, Iterations, () => _ = Ok.Ensure(static v => v > 0, "neg"), "Ensure (pass)");
        AllocationGate.AssertBudget(0, Iterations, () => _ = Ok.Ensure(static v => v < 0, "neg"), "Ensure (fail)");
    }

    [Fact]
    public void Combine_OverStackSpan_IsZeroAlloc()
    {
        AllocationGate.AssertBudget(0, Iterations, () =>
        {
            ReadOnlySpan<Result<int, string>> all = [Ok, Ok, Ok];
            _ = ResultExtensions.Combine(all);
        }, "Combine (all success)");
        AllocationGate.AssertBudget(0, Iterations, () =>
        {
            ReadOnlySpan<Result<int, string>> all = [Ok, Err, Ok];
            _ = ResultExtensions.Combine(all);
        }, "Combine (first failure)");
    }

    [Fact]
    public void Linq_SelectAndSelectMany_AreZeroAlloc()
    {
        AllocationGate.AssertBudget(0, Iterations, () =>
        {
            var r = from x in Ok select x * 2;
            _sink = r.Value;
        }, "LINQ select");
        AllocationGate.AssertBudget(0, Iterations, () =>
        {
            var r = from x in Ok
                    from y in Result<int, string>.Success(x + 1)
                    select x + y;
            _sink = r.Value;
        }, "LINQ from-from-select");
        AllocationGate.AssertBudget(0, Iterations, () =>
        {
            var r = from x in Err
                    from y in Result<int, string>.Success(x + 1)
                    select x + y;
            _ = r;
        }, "LINQ from-from-select (failure)");
    }

    // The async combinators are only 0 B when every awaited ValueTask has already completed:
    // an async ValueTask method that never suspends returns its result without boxing a state
    // machine. A suspending continuation allocates by design; see ResultAsyncExtensions.

    [Fact]
    public void MapAsync_SyncCompletion_IsZeroAlloc() =>
        AllocationGate.AssertBudgetValueTask(0, Iterations,
            () => Ok.MapAsync(static x => new ValueTask<int>(x * 2)), "MapAsync");

    [Fact]
    public void BindAsync_SyncCompletion_IsZeroAlloc() =>
        AllocationGate.AssertBudgetValueTask(0, Iterations,
            () => Ok.BindAsync(static x => new ValueTask<Result<int, string>>(Result<int, string>.Success(x + 1))),
            "BindAsync");

    [Fact]
    public void TapAsync_SyncCompletion_IsZeroAlloc()
    {
        AllocationGate.AssertBudgetValueTask(0, Iterations,
            () => Ok.TapAsync(static _ => ValueTask.CompletedTask), "TapAsync");
        AllocationGate.AssertBudgetValueTask(0, Iterations,
            () => Err.TapErrorAsync(static _ => ValueTask.CompletedTask), "TapErrorAsync");
    }

    [Fact]
    public void MatchAsync_SyncCompletion_IsZeroAlloc() =>
        AllocationGate.AssertBudgetValueTask(0, Iterations,
            () => Ok.MatchAsync(static v => new ValueTask<int>(v), static e => new ValueTask<int>(e.Length)),
            "MatchAsync");

    [Fact]
    public void AsyncContinuations_SyncCompletion_AreZeroAlloc()
    {
        AllocationGate.AssertBudgetValueTask(0, Iterations,
            () => new ValueTask<Result<int, string>>(Ok).Map(static x => x * 2), "ValueTask<Result>.Map");
        AllocationGate.AssertBudgetValueTask(0, Iterations,
            () => new ValueTask<Result<int, string>>(Ok).Bind(static x => Result<int, string>.Success(x + 1)),
            "ValueTask<Result>.Bind");
    }
}
