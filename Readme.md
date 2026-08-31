# SplashKitNUnit

A demonstration of writing NUnit unit tests for SplashKit game logic, separating pure logic from SplashKit side effects.

This repository contains two projects:

- **game/** – A console application that uses SplashKit to run a simple game.
- **game.test/** – An NUnit test project that tests the pure logic classes (score calculation, physics, collision detection).

## Prerequisites

Before setting up this project, ensure you have the following installed:

1. **<https://dotnet.microsoft.com/download>** (version 8.0 or later)
2. **<https://splashkit.io/>** – Install the SplashKit command-line tools:
   - Windows: `winget install SplashKit`
   - macOS/Linux: follow the <https://splashkit.io/articles/installation/>

Verify installations:

    dotnet --version
    skm --version

## Setup Instructions

Proper setup is critical for this project to compile and run correctly. Follow these steps exactly.

### 1. Clone the repository

    git clone <repository-url>
    cd SplashKitNUnit

### 2. Restore dependencies

    dotnet restore

This restores all NuGet packages (NUnit, test adapter, etc.) and sets up project references.

### 3. Build the solution

    dotnet build

This compiles both the game project and the test project. If you see any errors, double-check that you have the correct .NET SDK version and that SplashKit is properly installed.

### 4. Run the tests

    dotnet test

All tests should pass. Expected output:

    Passed! - Failed: 0, Passed: 12, Skipped: 0, Total: 12

### 5. Run the game (optional)

    dotnet run --project game

This launches the SplashKit window with the game loop. Press the spacebar to jump, close the window to exit.

## Troubleshooting

| Problem | Solution |
| --- | --- |
| `error CS0017: Program has more than one entry point` | Ensure the test project is a separate library (it already is). Run `dotnet clean` and rebuild. |
| `SplashKit.dll not found` | Reinstall SplashKit with `skm dotnet install` and restart your terminal. |
| `dotnet test` finds no tests | Make sure test files are inside `game.test/` and the `.csproj` includes `Microsoft.NET.Test.Sdk`. |
| `Assert.That` ambiguity | Use `Action` variable or `Assert.Throws<T>` as shown in the tutorial. |

## Project Structure

    SplashKitNUnit/
    ├── game/
    │   ├── Program.cs              # Entry point, SplashKit game loop
    │   ├── Models/
    │   │   ├── ScoreCalculator.cs  # Pure scoring logic
    │   │   ├── Physics.cs          # Gravity & jump calculations
    │   │   └── CollisionMath.cs    # Rectangle/circle collision
    │   ├── lib/
    │   │   └── SplashKit.cs        # SplashKit interop (auto-generated)
    │   └── game.csproj
    ├── game.test/
    │   ├── ScoreCalculatorTests.cs
    │   ├── PhysicsTests.cs
    │   ├── CollisionMathTests.cs
    │   ├── PlayerTests.cs
    │   └── game.test.csproj
    ├── SplashKitNUnit.sln
    ├── .gitignore
    └── README.md

## Contributing

This project is part of Deakin University's SIT771 unit. Feel free to fork and adapt for your own learning.

## License

MIT
