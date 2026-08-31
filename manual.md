# 1. Setup

Proper setup is the foundation of this tutorial. Follow these steps exactly to avoid common pitfalls like missing SplashKit references or conflicting entry points.

## 1.1 Prerequisites

Before you start, make sure you have the following installed:

- **.NET SDK 8.0 or later** – Download from https://dotnet.microsoft.com/download
- **SplashKit** – Install the command-line tools:
  - Windows: `winget install SplashKit`
  - macOS/Linux: follow the https://splashkit.io/articles/installation/

Verify both are available in your terminal:

    dotnet --version
    skm --version

## 1.2 Create the Solution and Projects

We will create a solution with two projects: a game application (using SplashKit) and a separate NUnit test project. The separation prevents the “more than one entry point” error.

Open a terminal in the folder where you want the project to live (e.g., `~/deakin/sit771/1.5d/`).

### Step 1: Create the solution file

    dotnet new sln -n SplashKitNUnit

### Step 2: Create the game project using the SplashKit template

    mkdir -p game
    cd game
    skm dotnet new
    cd ..

This generates a ready‑to‑run SplashKit console application with the correct `.csproj`, `Program.cs`, and the required `lib/SplashKit.cs` file. The project file will have `<OutputType>Exe</OutputType>`.

Add it to the solution:

    dotnet sln add game/game.csproj

### Step 3: Create the NUnit test project

    dotnet new nunit -n game.test -o game.test

This creates a class library (no `Main` method) with NUnit and the test SDK already referenced.

Add it to the solution:

    dotnet sln add game.test/game.test.csproj

### Step 4: Link the test project to the game project

The test project needs to access the classes in the game project (like `ScoreCalculator`, `Physics`, etc.). Add a project reference:

    dotnet add game.test/game.test.csproj reference game/game.csproj

## 1.3 Optional: Add Resource Folders

If your game uses images, sounds, or fonts, generate the required folders:

    cd game
    skm resources
    cd ..

This creates `Resources/images`, `Resources/sounds`, and `Resources/fonts` inside the `game/` folder. Place your assets in the appropriate subfolder.

## 1.4 Restore, Build, and Test

Now restore all NuGet packages and compile the entire solution:

    dotnet restore
    dotnet build

If the build succeeds, run the tests:

    dotnet test

You should see output similar to:

    Starting test execution, please wait...
    A total of 1 test files matched the specified pattern.

    Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 15 ms - game.test.dll (net8.0)

If you see zero tests, make sure your test files (`.cs` files with `[TestFixture]` and `[Test]` attributes) are inside the `game.test/` folder.

## 1.5 Running the Game (Optional)

To launch the SplashKit window and play the game:

    dotnet run --project game

Press the spacebar to jump (if implemented) and close the window to exit.

## 1.6 Understanding the Structure

After completing the steps, your project tree should look like this:

    SplashKitNUnit/
    ├── SplashKitNUnit.sln
    ├── game/
    │   ├── Program.cs
    │   ├── game.csproj
    │   ├── Models/
    │   │   ├── ScoreCalculator.cs
    │   │   ├── Physics.cs
    │   │   └── CollisionMath.cs
    │   ├── lib/
    │   │   └── SplashKit.cs
    │   └── Resources/          (if you ran skm resources)
    │       ├── images/
    │       ├── sounds/
    │       └── fonts/
    └── game.test/
        ├── game.test.csproj
        ├── ScoreCalculatorTests.cs
        ├── PhysicsTests.cs
        ├── CollisionMathTests.cs
        └── PlayerTests.cs

## 1.7 Common Setup Mistakes

| Mistake | Consequence | Fix |
|---|---|---|
| Using `dotnet new console` instead of `skm dotnet new` | Missing `lib/SplashKit.cs`; SplashKit API calls will fail. | Delete the project and re‑run `skm dotnet new`. |
| Placing test files inside the `game/` project | Multiple entry points error (`CS0017`). | Move all test files to `game.test/`. |
| Forgetting to add the project reference | Tests cannot see `Models.ScoreCalculator` etc. | Run `dotnet add game.test/game.test.csproj reference game/game.csproj`. |
| Not restoring after cloning | NuGet packages missing; build fails. | Always run `dotnet restore` first. |

## 1.8 Next Steps

With the setup verified, you are ready to explore the tutorial content. The following sections will explain how to write pure‑logic classes, create NUnit tests, and refactor a tightly‑coupled game object.

---

You can save this as `manual.md` in the root of your repository and link to it from `README.md` with:

    See manual.md for the full tutorial including setup.