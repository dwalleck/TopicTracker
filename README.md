# Tethys.Results

[![NuGet](https://img.shields.io/nuget/v/Tethys.Results.svg)](https://www.nuget.org/packages/Tethys.Results/)
[![Build Status](https://github.com/dwalleck/Tethys.Results/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/dwalleck/Tethys.Results/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A lightweight, thread-safe Result pattern implementation for .NET that provides a clean, functional approach to error handling without exceptions.

## Features

- ✅ **Simple and Intuitive API** - Easy to understand and use
- ✅ **Thread-Safe** - Immutable design ensures thread safety
- ✅ **No Dependencies** - Lightweight with zero external dependencies
- ✅ **Async Support** - First-class support for async/await patterns
- ✅ **Functional Composition** - Chain operations with `Then` and `When`
- ✅ **Type-Safe** - Generic `Result<T>` for operations that return values
- ✅ **Error Aggregation** - Combine multiple results and aggregate errors
- ✅ **Implicit Conversions** - Seamless conversion between values and Results

## Installation

```bash
dotnet add package Tethys.Results
```

## Quick Start

### Basic Usage

```csharp
using Tethys.Results;

// Simple success/failure results
Result successResult = Result.Ok("Success message");
Result failureResult = Result.Fail("Something went wrong");

// Results with values
Result<string> valueResult = Result<string>.Ok("Hello World", "Operation completed");
Result<int> errorResult = Result<int>.Fail("Not found");

// Results with exceptions
var exception = new Exception("Test exception");
Result failWithException = Result.Fail("Error message", exception);
```

### Chaining Operations

```csharp
// Chain multiple operations that depend on each other
var result = Result.Ok("Start")
    .Then(() => 
    {
        // Do some work
        return Result.Ok("First operation completed");
    })
    .Then(() => 
    {
        // Do more work
        return Result.Ok("Second operation completed");
    })
    .Then(() => 
    {
        // Final operation
        return Result.Ok("All operations completed");
    });

if (result.Success)
{
    Console.WriteLine($"Success: {result.Message}");
}
else
{
    Console.WriteLine($"Error: {result.Message}");
}
```

### Working with Data

```csharp
// Transform data through a pipeline
var result = Result<int>.Ok(42, "Initial value")
    .Then(value => 
    {
        // Transform the value
        return Result<string>.Ok(value.ToString(), "Converted to string");
    })
    .Then(str => 
    {
        // Further transformation
        return Result<string>.Ok($"The answer is: {str}", "Formatted result");
    });

// Extract the final value
string finalValue = result.GetValueOrDefault("No answer available");
```

### Async Support

```csharp
// Chain async operations seamlessly
var result = await Result.Ok("Start")
    .ThenAsync(async () =>
    {
        await Task.Delay(100); // Simulate async work
        return Result.Ok("Async operation completed");
    })
    .ThenAsync(async () =>
    {
        await Task.Delay(100); // More async work
        return Result<string>.Ok("Final result", "All async operations done");
    });

// Work with Task<Result> directly
Task<Result<int>> asyncResult = Task.FromResult(Result<int>.Ok(42, "From async"));
var processed = await asyncResult.ThenAsync(async value =>
{
    await Task.Delay(100);
    return Result<string>.Ok($"Processed: {value}", "Transformation complete");
});
```

### Conditional Execution

```csharp
// Execute operations conditionally
var result = Result.Ok("Initial state")
    .When(true, () => Result.Ok("Condition was true"))
    .When(false, () => Result.Ok("This won't execute"));

// Conditional execution with data
var dataResult = Result<int>.Ok(10)
    .When(true, () => Result<int>.Ok(20))
    .Then(value => Result<int>.Ok(value * 2)); // Results in 40
```

### Error Aggregation

```csharp
// Combine multiple validation results
var results = new List<Result>
{
    ValidateEmail("user@example.com"),
    ValidatePassword("SecurePass123!"),
    ValidateUsername("johndoe")
};

var combined = Result.Combine(results);
if (!combined.Success)
{
    // Access aggregated errors
    var aggregateError = combined.Exception as AggregateError;
    foreach (var error in aggregateError.ErrorMessages)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Combine results with data
var dataResults = new List<Result<int>>
{
    Result<int>.Ok(1),
    Result<int>.Ok(2),
    Result<int>.Ok(3)
};

var combinedData = Result<int>.Combine(dataResults);
if (combinedData.Success)
{
    var sum = combinedData.Data.Sum(); // Sum is 6
}
```

### Value Extraction

```csharp
Result<User> userResult = GetUser(userId);

// Get value or default
User user = userResult.GetValueOrDefault(new User { Name = "Guest" });

// Try pattern
if (userResult.TryGetValue(out User foundUser))
{
    Console.WriteLine($"Found user: {foundUser.Name}");
}
else
{
    Console.WriteLine("User not found");
}

// Get value or throw (use sparingly)
try
{
    User user = userResult.GetValueOrThrow();
    // Use the user
}
catch (InvalidOperationException ex)
{
    // Handle the error
    Console.WriteLine($"Failed to get user: {ex.Message}");
}
```

### Implicit Conversions

```csharp
// Implicitly convert values to Results
Result<int> implicitResult = 42; // Creates Result<int>.Ok(42)
Result<string> stringResult = "Hello"; // Creates Result<string>.Ok("Hello")

// Implicitly convert successful Results to values (throws if failed)
Result<int> successResult = Result<int>.Ok(42);
int value = successResult; // Gets 42

// Use in expressions
Result<int> result1 = Result<int>.Ok(10);
Result<int> result2 = Result<int>.Ok(20);
int sum = result1 + result2; // Implicit conversion, sum is 30
```

## API Reference

### Result Class

- `Result.Ok()` - Creates a successful result
- `Result.Ok(string message)` - Creates a successful result with a message
- `Result.Fail(string message)` - Creates a failed result with an error message
- `Result.Fail(string message, Exception exception)` - Creates a failed result with message and exception
- `Result.Fail(Exception exception)` - Creates a failed result from an exception
- `Result.Combine(IEnumerable<Result> results)` - Combines multiple results into one

### Result<T> Class

- `Result<T>.Ok(T value)` - Creates a successful result with a value
- `Result<T>.Ok(T value, string message)` - Creates a successful result with a value and message
- `Result<T>.Fail(string message)` - Creates a failed result with an error message
- `Result<T>.Fail(string message, Exception exception)` - Creates a failed result with message and exception
- `Result<T>.Fail(Exception exception)` - Creates a failed result from an exception
- `Result<T>.Combine(IEnumerable<Result<T>> results)` - Combines multiple results with values

### Extension Methods

- `Then(Func<Result> operation)` - Chains operations on successful results
- `Then<T>(Func<Result<T>> operation)` - Chains operations that return values
- `ThenAsync(Func<Task<Result>> operation)` - Chains async operations
- `ThenAsync<T>(Func<Task<Result<T>>> operation)` - Chains async operations that return values
- `When(bool condition, Func<Result> operation)` - Conditionally executes operations
- `GetValueOrDefault(T defaultValue = default)` - Gets the value or a default
- `TryGetValue(out T value)` - Tries to get the value using the Try pattern
- `GetValueOrThrow()` - Gets the value or throws an exception

## Advanced Usage

### Real-World Example: Order Processing

```csharp
public async Task<Result<Order>> ProcessOrderAsync(OrderRequest request)
{
    return await ValidateOrderRequest(request)
        .ThenAsync(async validRequest => await CreateOrder(validRequest))
        .ThenAsync(async order => await ApplyDiscounts(order))
        .ThenAsync(async order => await CalculateTaxes(order))
        .ThenAsync(async order => await ChargePayment(order))
        .ThenAsync(async order => await SendConfirmationEmail(order))
        .ThenAsync(async order =>
        {
            await LogOrderProcessed(order);
            return Result<Order>.Ok(order, "Order processed successfully");
        });
}

// Usage
var result = await ProcessOrderAsync(orderRequest);
if (result.Success)
{
    return Ok(result.Value);
}
else
{
    _logger.LogError(result.Exception, "Order processing failed: {Message}", result.Message);
    return BadRequest(result.Message);
}
```

### Integration with ASP.NET Core

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    
    return result.Success
        ? Ok(result.Value)
        : NotFound(result.Message);
}

[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var validationResult = ValidateRequest(request);
    if (!validationResult.Success)
    {
        return BadRequest(validationResult.Message);
    }

    var result = await _userService.CreateUserAsync(request);
    
    return result.Success
        ? CreatedAtAction(nameof(GetUser), new { id = result.Value.Id }, result.Value)
        : BadRequest(result.Message);
}
```

### Error Handling Patterns

```csharp
// Centralized error handling
public Result<T> ExecuteWithErrorHandling<T>(Func<T> operation, string operationName)
{
    try
    {
        var result = operation();
        return Result<T>.Ok(result, $"{operationName} completed successfully");
    }
    catch (ValidationException ex)
    {
        return Result<T>.Fail($"Validation failed in {operationName}: {ex.Message}", ex);
    }
    catch (NotFoundException ex)
    {
        return Result<T>.Fail($"Resource not found in {operationName}: {ex.Message}", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in {OperationName}", operationName);
        return Result<T>.Fail($"An unexpected error occurred in {operationName}", ex);
    }
}
```

## Best Practices

1. **Use Result for Expected Failures** - Reserve exceptions for truly exceptional cases
2. **Chain Operations** - Leverage `Then` and `ThenAsync` for clean, readable code
3. **Avoid Nested Results** - Use `Then` instead of manual checking
4. **Consistent Error Messages** - Provide clear, actionable error messages
5. **Leverage Implicit Conversions** - Simplify code with implicit conversions where appropriate
6. **Prefer TryGetValue** - Use `TryGetValue` over `GetValueOrThrow` for safer value extraction
7. **Aggregate Validation Errors** - Use `Result.Combine` for multiple validation checks

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 📧 Email: support@tethys.dev
- 🐛 Issues: [GitHub Issues](https://github.com/dwalleck/Tethys.Results/issues)
- 📖 Documentation: [Full Documentation](https://github.com/dwalleck/Tethys.Results/wiki)

## Acknowledgments

Inspired by functional programming patterns and the Railway Oriented Programming approach.