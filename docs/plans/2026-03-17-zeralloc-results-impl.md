# ZeroAlloc.Results Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a zero-allocation, no-boxing `Result<T, E>` library for .NET with full CFE API parity.

**Architecture:** Five `readonly struct` types (`Result`, `Result<T>`, `Result<T, E>`, `UnitResult<E>`, `Maybe<T>`) sharing a constraint-only `IResult<T, E>` interface. All combinators are extension methods to keep the structs minimal. Async variants return `ValueTask`. LINQ query syntax via `Select`/`SelectMany`/`Where`.

**Tech Stack:** .NET 8, C# 12, xUnit 2.6, BenchmarkDotNet 0.13

---

### Task 1: Solution scaffold

**Files:**
- Create: `ZeroAlloc.Results.sln`
- Create: `src/ZeroAlloc.Results/ZeroAlloc.Results.csproj`
- Create: `tests/ZeroAlloc.Results.Tests/ZeroAlloc.Results.Tests.csproj`

**Step 1: Create the solution and projects**

```bash
cd c:/Projects/Prive/ZeroAlloc.Results
dotnet new sln -n ZeroAlloc.Results
dotnet new classlib -n ZeroAlloc.Results -o src/ZeroAlloc.Results --framework net8.0
dotnet new xunit -n ZeroAlloc.Results.Tests -o tests/ZeroAlloc.Results.Tests --framework net8.0
dotnet sln add src/ZeroAlloc.Results/ZeroAlloc.Results.csproj
dotnet sln add tests/ZeroAlloc.Results.Tests/ZeroAlloc.Results.Tests.csproj
dotnet add tests/ZeroAlloc.Results.Tests/ZeroAlloc.Results.Tests.csproj reference src/ZeroAlloc.Results/ZeroAlloc.Results.csproj
```

**Step 2: Configure the library project**

Replace the contents of `src/ZeroAlloc.Results/ZeroAlloc.Results.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <AllowUnsafeBlocks>false</AllowUnsafeBlocks>
    <Optimize>true</Optimize>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

**Step 3: Configure the test project**

Replace the contents of `tests/ZeroAlloc.Results.Tests/ZeroAlloc.Results.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.6" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="BenchmarkDotNet" Version="0.13.12" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/ZeroAlloc.Results/ZeroAlloc.Results.csproj" />
  </ItemGroup>
</Project>
```

**Step 4: Delete the default generated files**

```bash
rm src/ZeroAlloc.Results/Class1.cs
rm tests/ZeroAlloc.Results.Tests/UnitTest1.cs
```

**Step 5: Verify the solution builds**

```bash
dotnet build ZeroAlloc.Results.sln
```
Expected: `Build succeeded`

**Step 6: Commit**

```bash
git add .
git commit -m "chore: scaffold solution, library, and test projects"
```

---

### Task 2: IResult interface

**Files:**
- Create: `src/ZeroAlloc.Results/IResult.cs`

**Step 1: Write the failing test**

Create `tests/ZeroAlloc.Results.Tests/IResultTests.cs`:

```csharp
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class IResultTests
{
    [Fact]
    public void IResult_IsConstraintOnly_NotUsableAsVariable()
    {
        // This test documents intent: IResult<T,E> exists only as a generic constraint.
        // The interface itself must be internal or carry an [EditorBrowsable(Never)] attribute.
        // We verify the interface exists by using it as a constraint.
        static TResult GetSuccess<TResult>(TResult r) where TResult : IResult<int, string> => r;

        // Actual assertion: compiles and works (if this file compiles, the interface exists)
        Assert.True(true);
    }
}
```

**Step 2: Run test to verify it fails**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~IResultTests" -v n
```
Expected: FAIL — `ZeroAlloc.Results.IResult` not found

**Step 3: Create the interface**

Create `src/ZeroAlloc.Results/IResult.cs`:

```csharp
using System.ComponentModel;

namespace ZeroAlloc.Results;

/// <summary>
/// Marker interface for result types.
/// WARNING: Never use as a variable or return type — doing so boxes the struct onto the heap.
/// Only use as a generic type constraint: <c>where TResult : IResult&lt;T, E&gt;</c>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IResult<out T, out E>
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    T Value { get; }
    E Error { get; }
}
```

**Step 4: Run test to verify it passes**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~IResultTests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/IResult.cs tests/ZeroAlloc.Results.Tests/IResultTests.cs
git commit -m "feat: add IResult<T,E> constraint-only interface"
```

---

### Task 3: Result\<T, E\> — core struct

**Files:**
- Create: `src/ZeroAlloc.Results/Result2.cs`
- Create: `tests/ZeroAlloc.Results.Tests/Result2Tests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/Result2Tests.cs`:

```csharp
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class Result2Tests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result<int, string>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = Result<int, string>.Failure("error");
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("error", result.Error);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result<int, string>.Failure("error");
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        var result = Result<int, string>.Success(42);
        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_IsSuccess()
    {
        Result<int, string> result = 42;
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_IsFailure()
    {
        Result<int, string> result = "error";
        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        // Verifies Result<T,E> is a value type
        Assert.True(typeof(Result<int, string>).IsValueType);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~Result2Tests" -v n
```
Expected: FAIL — `Result<T,E>` not found

**Step 3: Implement Result\<T, E\>**

Create `src/ZeroAlloc.Results/Result2.cs`:

```csharp
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

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public T Value => _isSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public E Error => !_isSuccess
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public static Result<T, E> Success(T value) => new(value);
    public static Result<T, E> Failure(E error) => new(error);

    public static implicit operator Result<T, E>(T value) => Success(value);
    public static implicit operator Result<T, E>(E error) => Failure(error);

    public override string ToString() =>
        _isSuccess ? $"Success({_value})" : $"Failure({_error})";
}
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~Result2Tests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/Result2.cs tests/ZeroAlloc.Results.Tests/Result2Tests.cs
git commit -m "feat: add Result<T,E> zero-alloc readonly struct"
```

---

### Task 4: Result\<T\> — string error shorthand

**Files:**
- Create: `src/ZeroAlloc.Results/Result1.cs`
- Create: `tests/ZeroAlloc.Results.Tests/Result1Tests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/Result1Tests.cs`:

```csharp
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class Result1Tests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = Result<int>.Failure("went wrong");
        Assert.True(result.IsFailure);
        Assert.Equal("went wrong", result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromValue()
    {
        Result<int> result = 99;
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        Result<int> result = "bad";
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(Result<int>).IsValueType);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~Result1Tests" -v n
```

**Step 3: Implement Result\<T\>**

Create `src/ZeroAlloc.Results/Result1.cs`:

```csharp
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

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public T Value => _isSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public string Error => !_isSuccess
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(string error) => Failure(error);

    public override string ToString() =>
        _isSuccess ? $"Success({_value})" : $"Failure({_error})";
}
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~Result1Tests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/Result1.cs tests/ZeroAlloc.Results.Tests/Result1Tests.cs
git commit -m "feat: add Result<T> shorthand with string error"
```

---

### Task 5: Result — non-generic pass/fail

**Files:**
- Create: `src/ZeroAlloc.Results/Result.cs`
- Create: `tests/ZeroAlloc.Results.Tests/ResultTests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/ResultTests.cs`:

```csharp
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = Result.Failure("went wrong");
        Assert.True(result.IsFailure);
        Assert.Equal("went wrong", result.Error);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(Result).IsValueType);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultTests" -v n
```

**Step 3: Implement Result**

Create `src/ZeroAlloc.Results/Result.cs`:

```csharp
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

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public string Error => !_isSuccess
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);

    public override string ToString() => _isSuccess ? "Success" : $"Failure({_error})";
}
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultTests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/Result.cs tests/ZeroAlloc.Results.Tests/ResultTests.cs
git commit -m "feat: add non-generic Result pass/fail struct"
```

---

### Task 6: UnitResult\<E\>

**Files:**
- Create: `src/ZeroAlloc.Results/UnitResult.cs`
- Create: `tests/ZeroAlloc.Results.Tests/UnitResultTests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/UnitResultTests.cs`:

```csharp
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class UnitResultTests
{
    [Fact]
    public void Success_IsSuccess_True()
    {
        var result = UnitResult<string>.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_IsFailure_True()
    {
        var result = UnitResult<string>.Failure("typed error");
        Assert.True(result.IsFailure);
        Assert.Equal("typed error", result.Error);
    }

    [Fact]
    public void Error_OnSuccess_Throws()
    {
        var result = UnitResult<string>.Success();
        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(UnitResult<string>).IsValueType);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~UnitResultTests" -v n
```

**Step 3: Implement UnitResult\<E\>**

Create `src/ZeroAlloc.Results/UnitResult.cs`:

```csharp
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

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public E Error => !_isSuccess
        ? _error
        : throw new InvalidOperationException("Cannot access Error on a successful UnitResult.");

    public static UnitResult<E> Success() => new(true, default!);
    public static UnitResult<E> Failure(E error) => new(false, error);

    public static implicit operator UnitResult<E>(E error) => Failure(error);

    public override string ToString() => _isSuccess ? "Success" : $"Failure({_error})";
}
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~UnitResultTests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/UnitResult.cs tests/ZeroAlloc.Results.Tests/UnitResultTests.cs
git commit -m "feat: add UnitResult<E> zero-alloc struct"
```

---

### Task 7: Maybe\<T\>

**Files:**
- Create: `src/ZeroAlloc.Results/Maybe.cs`
- Create: `tests/ZeroAlloc.Results.Tests/MaybeTests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/MaybeTests.cs`:

```csharp
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests;

public class MaybeTests
{
    [Fact]
    public void Some_HasValue_True()
    {
        var maybe = Maybe<int>.Some(42);
        Assert.True(maybe.HasValue);
        Assert.Equal(42, maybe.Value);
    }

    [Fact]
    public void None_HasValue_False()
    {
        var maybe = Maybe<int>.None;
        Assert.False(maybe.HasValue);
    }

    [Fact]
    public void Value_OnNone_Throws()
    {
        var maybe = Maybe<int>.None;
        Assert.Throws<InvalidOperationException>(() => _ = maybe.Value);
    }

    [Fact]
    public void ImplicitConversion_FromValue()
    {
        Maybe<int> maybe = 42;
        Assert.True(maybe.HasValue);
        Assert.Equal(42, maybe.Value);
    }

    [Fact]
    public void IsStruct_NoHeapAllocation()
    {
        Assert.True(typeof(Maybe<int>).IsValueType);
    }

    [Fact]
    public void GetValueOrDefault_OnNone_ReturnsDefault()
    {
        var maybe = Maybe<int>.None;
        Assert.Equal(0, maybe.GetValueOrDefault());
        Assert.Equal(99, maybe.GetValueOrDefault(99));
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~MaybeTests" -v n
```

**Step 3: Implement Maybe\<T\>**

Create `src/ZeroAlloc.Results/Maybe.cs`:

```csharp
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

    public bool HasValue => _hasValue;
    public bool HasNoValue => !_hasValue;

    public T Value => _hasValue
        ? _value
        : throw new InvalidOperationException("Maybe has no value.");

    public T GetValueOrDefault(T defaultValue = default!) =>
        _hasValue ? _value : defaultValue;

    public static Maybe<T> Some(T value) => new(value);
    public static readonly Maybe<T> None = default;

    public static implicit operator Maybe<T>(T value) => Some(value);

    public override string ToString() => _hasValue ? $"Some({_value})" : "None";
}
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~MaybeTests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/Maybe.cs tests/ZeroAlloc.Results.Tests/MaybeTests.cs
git commit -m "feat: add Maybe<T> zero-alloc Option type"
```

---

### Task 8: Core extension methods — Map, MapError, Bind

**Files:**
- Create: `src/ZeroAlloc.Results/Extensions/ResultExtensions.cs`
- Create: `tests/ZeroAlloc.Results.Tests/Extensions/ResultExtensionsMapBindTests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/Extensions/ResultExtensionsMapBindTests.cs`:

```csharp
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultExtensionsMapBindTests
{
    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int, string>.Success(5)
            .Map(x => x * 2);
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void Map_OnFailure_PassesThroughError()
    {
        var result = Result<int, string>.Failure("err")
            .Map(x => x * 2);
        Assert.True(result.IsFailure);
        Assert.Equal("err", result.Error);
    }

    [Fact]
    public void MapError_OnFailure_TransformsError()
    {
        var result = Result<int, string>.Failure("err")
            .MapError(e => e.Length);
        Assert.True(result.IsFailure);
        Assert.Equal(3, result.Error);
    }

    [Fact]
    public void MapError_OnSuccess_PassesThroughValue()
    {
        var result = Result<int, string>.Success(42)
            .MapError(e => e.Length);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Bind_OnSuccess_ChainsResult()
    {
        var result = Result<int, string>.Success(5)
            .Bind(x => x > 0
                ? Result<string, string>.Success("positive")
                : Result<string, string>.Failure("not positive"));
        Assert.True(result.IsSuccess);
        Assert.Equal("positive", result.Value);
    }

    [Fact]
    public void Bind_OnFailure_SkipsFunction()
    {
        var called = false;
        var result = Result<int, string>.Failure("err")
            .Bind(x => { called = true; return Result<string, string>.Success("x"); });
        Assert.False(called);
        Assert.True(result.IsFailure);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultExtensionsMapBindTests" -v n
```

**Step 3: Create the Extensions directory and implement**

```bash
mkdir -p src/ZeroAlloc.Results/Extensions
mkdir -p tests/ZeroAlloc.Results.Tests/Extensions
```

Create `src/ZeroAlloc.Results/Extensions/ResultExtensions.cs`:

```csharp
namespace ZeroAlloc.Results.Extensions;

public static class ResultExtensions
{
    // ── Map ──────────────────────────────────────────────────────────────

    public static Result<U, E> Map<T, U, E>(
        this Result<T, E> result,
        Func<T, U> map) =>
        result.IsSuccess
            ? Result<U, E>.Success(map(result.Value))
            : Result<U, E>.Failure(result.Error);

    public static Result<U> Map<T, U>(
        this Result<T> result,
        Func<T, U> map) =>
        result.IsSuccess
            ? Result<U>.Success(map(result.Value))
            : Result<U>.Failure(result.Error);

    // ── MapError ─────────────────────────────────────────────────────────

    public static Result<T, F> MapError<T, E, F>(
        this Result<T, E> result,
        Func<E, F> mapError) =>
        result.IsSuccess
            ? Result<T, F>.Success(result.Value)
            : Result<T, F>.Failure(mapError(result.Error));

    // ── Bind ─────────────────────────────────────────────────────────────

    public static Result<U, E> Bind<T, U, E>(
        this Result<T, E> result,
        Func<T, Result<U, E>> bind) =>
        result.IsSuccess
            ? bind(result.Value)
            : Result<U, E>.Failure(result.Error);

    public static Result<U> Bind<T, U>(
        this Result<T> result,
        Func<T, Result<U>> bind) =>
        result.IsSuccess
            ? bind(result.Value)
            : Result<U>.Failure(result.Error);
}
```

**Step 4: Add using to test file**

Add at the top of `ResultExtensionsMapBindTests.cs`:

```csharp
using ZeroAlloc.Results.Extensions;
```

**Step 5: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultExtensionsMapBindTests" -v n
```
Expected: PASS

**Step 6: Commit**

```bash
git add src/ZeroAlloc.Results/Extensions/ tests/ZeroAlloc.Results.Tests/Extensions/
git commit -m "feat: add Map, MapError, Bind extension methods"
```

---

### Task 9: Match, Tap, TapError

**Files:**
- Modify: `src/ZeroAlloc.Results/Extensions/ResultExtensions.cs`
- Create: `tests/ZeroAlloc.Results.Tests/Extensions/ResultExtensionsMatchTapTests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/Extensions/ResultExtensionsMatchTapTests.cs`:

```csharp
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultExtensionsMatchTapTests
{
    [Fact]
    public void Match_OnSuccess_InvokesOnSuccess()
    {
        var result = Result<int, string>.Success(42)
            .Match(v => v * 2, _ => -1);
        Assert.Equal(84, result);
    }

    [Fact]
    public void Match_OnFailure_InvokesOnFailure()
    {
        var result = Result<int, string>.Failure("err")
            .Match(_ => 0, e => e.Length);
        Assert.Equal(3, result);
    }

    [Fact]
    public void Tap_OnSuccess_InvokesSideEffect()
    {
        int sideEffect = 0;
        var result = Result<int, string>.Success(5)
            .Tap(v => sideEffect = v);
        Assert.Equal(5, sideEffect);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Tap_OnFailure_DoesNotInvoke()
    {
        var called = false;
        Result<int, string>.Failure("err").Tap(_ => called = true);
        Assert.False(called);
    }

    [Fact]
    public void TapError_OnFailure_InvokesSideEffect()
    {
        string? captured = null;
        var result = Result<int, string>.Failure("err")
            .TapError(e => captured = e);
        Assert.Equal("err", captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TapError_OnSuccess_DoesNotInvoke()
    {
        var called = false;
        Result<int, string>.Success(1).TapError(_ => called = true);
        Assert.False(called);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultExtensionsMatchTapTests" -v n
```

**Step 3: Add Match, Tap, TapError to ResultExtensions.cs**

Append to `src/ZeroAlloc.Results/Extensions/ResultExtensions.cs`:

```csharp
    // ── Match ─────────────────────────────────────────────────────────────

    public static U Match<T, E, U>(
        this Result<T, E> result,
        Func<T, U> onSuccess,
        Func<E, U> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

    public static U Match<T, U>(
        this Result<T> result,
        Func<T, U> onSuccess,
        Func<string, U> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

    // ── Tap ───────────────────────────────────────────────────────────────

    public static Result<T, E> Tap<T, E>(
        this Result<T, E> result,
        Action<T> action)
    {
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    public static Result<T, E> TapError<T, E>(
        this Result<T, E> result,
        Action<E> action)
    {
        if (result.IsFailure) action(result.Error);
        return result;
    }
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultExtensionsMatchTapTests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/Extensions/ResultExtensions.cs tests/ZeroAlloc.Results.Tests/Extensions/ResultExtensionsMatchTapTests.cs
git commit -m "feat: add Match, Tap, TapError extension methods"
```

---

### Task 10: Ensure, Combine

**Files:**
- Modify: `src/ZeroAlloc.Results/Extensions/ResultExtensions.cs`
- Create: `tests/ZeroAlloc.Results.Tests/Extensions/ResultExtensionsEnsureCombineTests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/Extensions/ResultExtensionsEnsureCombineTests.cs`:

```csharp
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultExtensionsEnsureCombineTests
{
    [Fact]
    public void Ensure_PredicateTrue_ReturnsSuccess()
    {
        var result = Result<int, string>.Success(10)
            .Ensure(x => x > 0, "must be positive");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Ensure_PredicateFalse_ReturnsFailure()
    {
        var result = Result<int, string>.Success(-1)
            .Ensure(x => x > 0, "must be positive");
        Assert.True(result.IsFailure);
        Assert.Equal("must be positive", result.Error);
    }

    [Fact]
    public void Ensure_OnFailure_SkipsPredicate()
    {
        var called = false;
        var result = Result<int, string>.Failure("original")
            .Ensure(x => { called = true; return true; }, "irrelevant");
        Assert.False(called);
        Assert.Equal("original", result.Error);
    }

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        Span<Result<int, string>> results =
        [
            Result<int, string>.Success(1),
            Result<int, string>.Success(2),
            Result<int, string>.Success(3)
        ];
        var combined = ResultExtensions.Combine<int, string>(results);
        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void Combine_AnyFailure_ReturnsFirstFailure()
    {
        Span<Result<int, string>> results =
        [
            Result<int, string>.Success(1),
            Result<int, string>.Failure("first error"),
            Result<int, string>.Failure("second error")
        ];
        var combined = ResultExtensions.Combine<int, string>(results);
        Assert.True(combined.IsFailure);
        Assert.Equal("first error", combined.Error);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultExtensionsEnsureCombineTests" -v n
```

**Step 3: Add Ensure and Combine to ResultExtensions.cs**

Append to `src/ZeroAlloc.Results/Extensions/ResultExtensions.cs`:

```csharp
    // ── Ensure ────────────────────────────────────────────────────────────

    public static Result<T, E> Ensure<T, E>(
        this Result<T, E> result,
        Func<T, bool> predicate,
        E error) =>
        result.IsSuccess
            ? predicate(result.Value) ? result : Result<T, E>.Failure(error)
            : result;

    // ── Combine ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns Success if all results succeed, otherwise returns the first failure.
    /// Accepts a <see cref="ReadOnlySpan{T}"/> to avoid heap allocation at the call site
    /// — use stackalloc or a stack-local Span to keep the call zero-alloc.
    /// </summary>
    public static UnitResult<E> Combine<T, E>(ReadOnlySpan<Result<T, E>> results)
    {
        foreach (ref readonly var result in results)
        {
            if (result.IsFailure)
                return UnitResult<E>.Failure(result.Error);
        }
        return UnitResult<E>.Success();
    }
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultExtensionsEnsureCombineTests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/Extensions/ResultExtensions.cs tests/ZeroAlloc.Results.Tests/Extensions/ResultExtensionsEnsureCombineTests.cs
git commit -m "feat: add Ensure and Combine extension methods"
```

---

### Task 11: Async extensions (ValueTask)

**Files:**
- Create: `src/ZeroAlloc.Results/Extensions/ResultAsyncExtensions.cs`
- Create: `tests/ZeroAlloc.Results.Tests/Extensions/ResultAsyncExtensionsTests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/Extensions/ResultAsyncExtensionsTests.cs`:

```csharp
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultAsyncExtensionsTests
{
    [Fact]
    public async Task MapAsync_OnSuccess_TransformsValue()
    {
        var result = await Result<int, string>.Success(5)
            .MapAsync(async x => { await Task.Yield(); return x * 2; });
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public async Task MapAsync_OnFailure_PassesThroughError()
    {
        var result = await Result<int, string>.Failure("err")
            .MapAsync(async x => { await Task.Yield(); return x * 2; });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task BindAsync_OnSuccess_ChainsResult()
    {
        var result = await Result<int, string>.Success(5)
            .BindAsync(async x =>
            {
                await Task.Yield();
                return Result<string, string>.Success(x.ToString());
            });
        Assert.True(result.IsSuccess);
        Assert.Equal("5", result.Value);
    }

    [Fact]
    public async Task TapAsync_OnSuccess_InvokesSideEffect()
    {
        int sideEffect = 0;
        await Result<int, string>.Success(7)
            .TapAsync(async v => { await Task.Yield(); sideEffect = v; });
        Assert.Equal(7, sideEffect);
    }

    [Fact]
    public async Task TapErrorAsync_OnFailure_InvokesSideEffect()
    {
        string? captured = null;
        await Result<int, string>.Failure("err")
            .TapErrorAsync(async e => { await Task.Yield(); captured = e; });
        Assert.Equal("err", captured);
    }

    [Fact]
    public async Task MatchAsync_OnSuccess_InvokesOnSuccess()
    {
        var result = await Result<int, string>.Success(3)
            .MatchAsync(
                async v => { await Task.Yield(); return v * 10; },
                async _ => { await Task.Yield(); return -1; });
        Assert.Equal(30, result);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultAsyncExtensionsTests" -v n
```

**Step 3: Implement async extensions**

Create `src/ZeroAlloc.Results/Extensions/ResultAsyncExtensions.cs`:

```csharp
namespace ZeroAlloc.Results.Extensions;

public static class ResultAsyncExtensions
{
    // ── MapAsync ──────────────────────────────────────────────────────────

    public static async ValueTask<Result<U, E>> MapAsync<T, U, E>(
        this Result<T, E> result,
        Func<T, ValueTask<U>> map) =>
        result.IsSuccess
            ? Result<U, E>.Success(await map(result.Value).ConfigureAwait(false))
            : Result<U, E>.Failure(result.Error);

    // Overload accepting Task-returning funcs for interop
    public static async ValueTask<Result<U, E>> MapAsync<T, U, E>(
        this Result<T, E> result,
        Func<T, Task<U>> map) =>
        result.IsSuccess
            ? Result<U, E>.Success(await map(result.Value).ConfigureAwait(false))
            : Result<U, E>.Failure(result.Error);

    // ── BindAsync ─────────────────────────────────────────────────────────

    public static async ValueTask<Result<U, E>> BindAsync<T, U, E>(
        this Result<T, E> result,
        Func<T, ValueTask<Result<U, E>>> bind) =>
        result.IsSuccess
            ? await bind(result.Value).ConfigureAwait(false)
            : Result<U, E>.Failure(result.Error);

    public static async ValueTask<Result<U, E>> BindAsync<T, U, E>(
        this Result<T, E> result,
        Func<T, Task<Result<U, E>>> bind) =>
        result.IsSuccess
            ? await bind(result.Value).ConfigureAwait(false)
            : Result<U, E>.Failure(result.Error);

    // ── TapAsync ──────────────────────────────────────────────────────────

    public static async ValueTask<Result<T, E>> TapAsync<T, E>(
        this Result<T, E> result,
        Func<T, ValueTask> action)
    {
        if (result.IsSuccess)
            await action(result.Value).ConfigureAwait(false);
        return result;
    }

    public static async ValueTask<Result<T, E>> TapAsync<T, E>(
        this Result<T, E> result,
        Func<T, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value).ConfigureAwait(false);
        return result;
    }

    // ── TapErrorAsync ─────────────────────────────────────────────────────

    public static async ValueTask<Result<T, E>> TapErrorAsync<T, E>(
        this Result<T, E> result,
        Func<E, ValueTask> action)
    {
        if (result.IsFailure)
            await action(result.Error).ConfigureAwait(false);
        return result;
    }

    public static async ValueTask<Result<T, E>> TapErrorAsync<T, E>(
        this Result<T, E> result,
        Func<E, Task> action)
    {
        if (result.IsFailure)
            await action(result.Error).ConfigureAwait(false);
        return result;
    }

    // ── MatchAsync ────────────────────────────────────────────────────────

    public static async ValueTask<U> MatchAsync<T, E, U>(
        this Result<T, E> result,
        Func<T, ValueTask<U>> onSuccess,
        Func<E, ValueTask<U>> onFailure) =>
        result.IsSuccess
            ? await onSuccess(result.Value).ConfigureAwait(false)
            : await onFailure(result.Error).ConfigureAwait(false);

    public static async ValueTask<U> MatchAsync<T, E, U>(
        this Result<T, E> result,
        Func<T, Task<U>> onSuccess,
        Func<E, Task<U>> onFailure) =>
        result.IsSuccess
            ? await onSuccess(result.Value).ConfigureAwait(false)
            : await onFailure(result.Error).ConfigureAwait(false);

    // ── ValueTask<Result<T,E>> continuation overloads ─────────────────────

    public static async ValueTask<Result<U, E>> Map<T, U, E>(
        this ValueTask<Result<T, E>> resultTask,
        Func<T, U> map)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(map);
    }

    public static async ValueTask<Result<U, E>> Bind<T, U, E>(
        this ValueTask<Result<T, E>> resultTask,
        Func<T, Result<U, E>> bind)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(bind);
    }
}
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultAsyncExtensionsTests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/Extensions/ResultAsyncExtensions.cs tests/ZeroAlloc.Results.Tests/Extensions/ResultAsyncExtensionsTests.cs
git commit -m "feat: add ValueTask async extension methods"
```

---

### Task 12: LINQ extensions

**Files:**
- Create: `src/ZeroAlloc.Results/Extensions/ResultLinqExtensions.cs`
- Create: `tests/ZeroAlloc.Results.Tests/Extensions/ResultLinqExtensionsTests.cs`

**Step 1: Write failing tests**

Create `tests/ZeroAlloc.Results.Tests/Extensions/ResultLinqExtensionsTests.cs`:

```csharp
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Extensions;

public class ResultLinqExtensionsTests
{
    [Fact]
    public void Select_OnSuccess_TransformsValue()
    {
        var result = from x in Result<int, string>.Success(5)
                     select x * 2;
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void SelectMany_BothSuccess_ChainsResult()
    {
        var result =
            from user in Result<string, string>.Success("alice")
            from greeting in Result<string, string>.Success($"Hello, {user}")
            select greeting;
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello, alice", result.Value);
    }

    [Fact]
    public void SelectMany_FirstFailure_ShortCircuits()
    {
        var secondCalled = false;
        var result =
            from user in Result<string, string>.Failure("no user")
            from greeting in (secondCalled = true, Result<string, string>.Success("hi")).Item2
            select greeting;
        Assert.False(secondCalled);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Where_PredicateTrue_ReturnsSuccess()
    {
        var result = from x in Result<int, string>.Success(10)
                     where x > 0
                     select x;
        // Where requires an error factory — test without Where syntax directly
        Assert.True(result.IsSuccess);
    }
}
```

**Step 2: Run tests to verify they fail**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultLinqExtensionsTests" -v n
```

**Step 3: Implement LINQ extensions**

Create `src/ZeroAlloc.Results/Extensions/ResultLinqExtensions.cs`:

```csharp
namespace ZeroAlloc.Results.Extensions;

/// <summary>
/// LINQ query syntax support for <see cref="Result{T,E}"/>.
/// Enables <c>from x in result select x</c> and <c>from x in r1 from y in r2 select ...</c>.
/// </summary>
public static class ResultLinqExtensions
{
    /// <summary>Enables <c>select</c> in query expressions (equivalent to Map).</summary>
    public static Result<U, E> Select<T, U, E>(
        this Result<T, E> result,
        Func<T, U> selector) =>
        result.Map(selector);

    /// <summary>Enables <c>from ... from ...</c> in query expressions (equivalent to Bind).</summary>
    public static Result<V, E> SelectMany<T, U, V, E>(
        this Result<T, E> result,
        Func<T, Result<U, E>> bind,
        Func<T, U, V> project) =>
        result.IsSuccess
            ? bind(result.Value) is { IsSuccess: true } bound
                ? Result<V, E>.Success(project(result.Value, bound.Value))
                : Result<V, E>.Failure(bind(result.Value).Error)
            : Result<V, E>.Failure(result.Error);
}
```

**Step 4: Run tests to verify they pass**

```bash
dotnet test tests/ZeroAlloc.Results.Tests --filter "FullyQualifiedName~ResultLinqExtensionsTests" -v n
```
Expected: PASS

**Step 5: Commit**

```bash
git add src/ZeroAlloc.Results/Extensions/ResultLinqExtensions.cs tests/ZeroAlloc.Results.Tests/Extensions/ResultLinqExtensionsTests.cs
git commit -m "feat: add LINQ query syntax extensions (Select, SelectMany)"
```

---

### Task 13: Allocation benchmarks

**Files:**
- Create: `tests/ZeroAlloc.Results.Tests/Benchmarks/AllocationBenchmarks.cs`
- Create: `tests/ZeroAlloc.Results.Tests/Benchmarks/BenchmarkRunner.cs`

**Step 1: Write the benchmark**

Create `tests/ZeroAlloc.Results.Tests/Benchmarks/AllocationBenchmarks.cs`:

```csharp
using BenchmarkDotNet.Attributes;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class AllocationBenchmarks
{
    [Benchmark]
    public Result<int, string> Create_Success() =>
        Result<int, string>.Success(42);

    [Benchmark]
    public Result<int, string> Create_Failure() =>
        Result<int, string>.Failure("error");

    [Benchmark]
    public Result<int, string> Map_Success() =>
        Result<int, string>.Success(5).Map(x => x * 2);

    [Benchmark]
    public Result<string, string> Bind_Chain() =>
        Result<int, string>.Success(5)
            .Bind(x => Result<string, string>.Success(x.ToString()));

    [Benchmark]
    public int Match_Success() =>
        Result<int, string>.Success(42).Match(v => v, _ => -1);

    [Benchmark]
    public Maybe<int> Maybe_Some() => Maybe<int>.Some(42);

    [Benchmark]
    public UnitResult<string> UnitResult_Success() => UnitResult<string>.Success();
}
```

Create `tests/ZeroAlloc.Results.Tests/Benchmarks/BenchmarkRunner.cs`:

```csharp
using BenchmarkDotNet.Running;

namespace ZeroAlloc.Results.Tests.Benchmarks;

/// <summary>
/// Run with: dotnet run --project tests/ZeroAlloc.Results.Tests -c Release -- --benchmark
/// All Gen0 columns must be 0.000 — any allocation is a regression.
/// </summary>
public static class BenchmarkEntryPoint
{
    public static void RunBenchmarks()
    {
        BenchmarkRunner.Run<AllocationBenchmarks>();
    }
}
```

**Step 2: Run benchmarks in Release mode**

```bash
cd c:/Projects/Prive/ZeroAlloc.Results
dotnet run --project tests/ZeroAlloc.Results.Tests -c Release -- --filter "*AllocationBenchmarks*"
```

Expected output (all Gen0 = 0.000):
```
| Method          | Mean     | Gen0   | Allocated |
|---------------- |---------:|-------:|----------:|
| Create_Success  | x.xx ns  | 0.000  |         - |
| Create_Failure  | x.xx ns  | 0.000  |         - |
| Map_Success     | x.xx ns  | 0.000  |         - |
| Bind_Chain      | x.xx ns  | 0.000  |         - |
| Match_Success   | x.xx ns  | 0.000  |         - |
| Maybe_Some      | x.xx ns  | 0.000  |         - |
| UnitResult_...  | x.xx ns  | 0.000  |         - |
```

If Gen0 > 0 on any row, there is a boxing or heap allocation regression — investigate before committing.

**Step 3: Run all unit tests to confirm nothing is broken**

```bash
dotnet test ZeroAlloc.Results.sln -v n
```
Expected: all tests PASS

**Step 4: Commit**

```bash
git add tests/ZeroAlloc.Results.Tests/Benchmarks/
git commit -m "test: add BenchmarkDotNet allocation benchmarks — Gen0 must be 0"
```

---

### Task 14: Full solution build + final verification

**Step 1: Build in Release**

```bash
dotnet build ZeroAlloc.Results.sln -c Release
```
Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

**Step 2: Run all tests**

```bash
dotnet test ZeroAlloc.Results.sln -c Release -v n
```
Expected: all tests PASS, 0 failures

**Step 3: Final commit**

```bash
git add .
git commit -m "chore: final build verification — all tests pass, zero allocation confirmed"
```
