# Copilot Instructions for Blazor Server App with MudBlazor and Unit Testing

This document provides guidance to GitHub Copilot for generating consistent, idiomatic, and testable code in this project.

## Project Summary

- **Framework**: .NET 8 (Blazor Server)
- **UI Framework**: MudBlazor
- **Testing**: xUnit and bUnit (for Blazor component testing)
- **Design Pattern**: Component-based with separation of concerns (logic in services or code-behind files)

---

## General Coding Guidelines

- Use `@inject` for dependency injection in `.razor` components.
- Separate business logic into services or `*.razor.cs` code-behind files when complexity grows.
- Use MudBlazor components such as `MudButton`, `MudCard`, `MudTable`, etc., with proper `@bind` or `OnClick` event handling.
- Prefer `Task`-based asynchronous methods.
- Validate inputs using `MudForm`, `MudTextField`, and `FluentValidation` or similar.

---

## File & Folder Structure

```
/BlazorMudApp
  /Pages           → Razor pages
  /Shared          → Layouts and reusable components
  /Services        → Application logic and APIs
  /Models          → DTOs and data models
  /Tests
    /UnitTests     → xUnit tests for services and logic
    /ComponentTests→ bUnit tests for UI components
```

---

## Razor Component Conventions

- Use MudBlazor UI components with clear labels and actions.
- Name component files with `.razor` and optional `.razor.cs` for logic.
- Example component pattern:

```razor
<!-- Counter.razor -->
@inject ICounterService CounterService

<MudPaper Class="pa-4">
    <MudText Typo="Typo.h6">Current Count: @_count</MudText>
    <MudButton Variant="Filled" OnClick="Increment">Increment</MudButton>
</MudPaper>

@code {
    private int _count;

    private void Increment()
    {
        _count = CounterService.Increment(_count);
    }
}
```

---

## Unit Testing Guidelines

### Unit Test Framework

- Use **xUnit** for services and logic.
- Use **bUnit** for Razor component testing.

### Service Test Example

```csharp
public class CounterServiceTests
{
    [Fact]
    public void Increment_ShouldIncreaseByOne()
    {
        var service = new CounterService();
        var result = service.Increment(3);
        Assert.Equal(4, result);
    }
}
```

### Component Test Example (bUnit)

```csharp
public class CounterComponentTests : TestContext
{
    [Fact]
    public void ButtonClick_IncrementsCount()
    {
        // Arrange
        var cut = RenderComponent<Counter>();

        // Act
        cut.Find("button").Click();

        // Assert
        cut.MarkupMatches(@"<div>Current Count: 1</div>");
    }
}
```

---

## Preferred Libraries

- **MudBlazor** for UI components.
- **xUnit** for unit testing logic.
- **bUnit** for Blazor component tests.
- **Moq** for mocking dependencies in unit tests.
- **FluentAssertions** (optional) for more readable assertions.

---

## Style Guide for Copilot

- Use PascalCase for class and method names.
- Use private fields prefixed with underscore (e.g., `_myService`).
- Use `async/await` pattern for I/O or delay operations.
- Prefer dependency injection over static classes.
- Add XML comments for public methods and classes.

---

## Example Services

```csharp
public interface ICounterService
{
    int Increment(int value);
}

public class CounterService : ICounterService
{
    public int Increment(int value) => value + 1;
}
```

---

## Notes for Copilot

- Generate components that are reusable and testable.
- Keep rendering logic and business logic separate.
- Suggest test code alongside components and services.
- If generating a form or interactive UI, include validation and submission logic.
- Follow SOLID principles where possible.
