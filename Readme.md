# SplashKitNUnit

[<https://img.shields.io/badge/GitHub-Bronardo%2FSplashKitNUnit-blue>](<https://github.com/Bronardo/SplashKitNUnit>)

A demonstration of writing NUnit unit tests for SplashKit game logic, separating pure logic from SplashKit side effects.

This repository contains two projects:

- **game/** – A console application that uses SplashKit to run a simple game.
- **game.test/** – An NUnit test project that tests the pure logic classes (score calculation, physics, collision detection).

The accompanying tutorial (`tutorial.md`) guides you through setting up the project, writing basic tests (Part 2), and applying advanced NUnit techniques (Part 3). Each part lives on its own branch so you can follow along without confusion.

## Repository

<https://github.com/Bronardo/SplashKitNUnit>

## Prerequisites

Before setting up this project, ensure you have the following installed:

1. **<https://dotnet.microsoft.com/download>** (version 8.0 or later)
2. **<https://splashkit.io/>** – Install the SplashKit command-line tools:
   - Windows: `winget install SplashKit`
   - macOS/Linux: follow the <https://splashkit.io/articles/installation/>

Verify installations:

```bash
dotnet --version
skm --version
```

## Setup Instructions

Proper setup is critical for this project to compile and run correctly. Follow these steps exactly.

### 1. Clone the repository

```bash
git clone https://github.com/Bronardo/SplashKitNUnit.git
cd SplashKitNUnit
```

### 2. Restore dependencies

```bash
dotnet restore
```

This restores all NuGet packages (NUnit, test adapter, etc.) and sets up project references.

### 3. Build the solution

```bash
dotnet build
```

This compiles both the game project and the test project. If you see any errors, double-check that you have the correct .NET SDK version and that SplashKit is properly installed.

### 4. Run the tests

```bash
dotnet test
```

All tests should pass. Expected output:

```
Passed! - Failed: 0, Passed: 33, Skipped: 0, Total: 33
```

### 5. Run the game (optional)

```bash
dotnet run --project game
```

This launches the SplashKit window with the game loop. Press the spacebar to jump, close the window to exit.

## Tutorial

The main tutorial content is in **./tutorial.md**. It walks you through:

- Part 1: Best practice of setting up a SplashKit project with NUnit.
- Part 2: Writing basic NUnit tests for the game logic.
- Part 3: Advanced techniques such as parameterised tests, `[TestCaseSource]`, `[SetUp]`, `Assert.Multiple`, and custom assertion helpers.

Each part corresponds to a Git branch (see below) so you can follow the tutorial step by step.

## Branch Guide – Reading Order

The tutorial is split into parts, each preserved on a separate branch. Choose the branch that matches your learning stage.

| Branch | Content | Test Count |
| --- | --- | --- |
| `part2-complete` | Basic NUnit tests (setup, assertions, edge cases) | 26 |
| `part3-advanced` | Advanced techniques (parameterised tests, `[TestCaseSource]`, `[SetUp]`, `Assert.Multiple`, custom helpers) | 33 |
| `main` | Latest stable version (currently Part 3) | 33 |

**How to switch branches:**

```bash
# Start with Part 2
git checkout part2-complete

# When ready for Part 3, create a new branch from part2-complete
git checkout -b my-part3 part2-complete

# Or jump straight to the final version
git checkout main
```

If you want to experiment with advanced techniques without losing the simple Part 2 code, always branch off `part2-complete` before making changes.

## Project Structure

```
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
├── README.md
└── tutorial.md                 # Main tutorial document
```

## Troubleshooting

| Problem | Solution |
| --- | --- |
| `error CS0017: Program has more than one entry point` | Ensure the test project is a separate library (it already is). Run `dotnet clean` and rebuild. |
| `SplashKit.dll not found` | Reinstall SplashKit with `skm dotnet install` and restart your terminal. |
| `dotnet test` finds no tests | Make sure test files are inside `game.test/` and the `.csproj` includes `Microsoft.NET.Test.Sdk`. |
| `Assert.That` ambiguity | Use `Action` variable or `Assert.Throws<T>` as shown in the tutorial. |

## Contributing

This project is part of Deakin University's SIT771 unit. Feel free to fork and adapt for your own learning.

## License

MIT
