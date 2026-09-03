# SplashKit & NUnit Tutorial

## 1. Setup

Proper setup is the foundation of this tutorial. Follow these steps exactly to avoid common pitfalls like missing SplashKit references or conflicting entry points.

### 1.1 Prerequisites

Before you start, make sure you have the following installed:

- **.NET SDK 8.0 or later** – Download from <https://dotnet.microsoft.com/download>
- **SplashKit** – Install the command-line tools:
  - Windows: `winget install SplashKit`
  - macOS/Linux: follow the <https://splashkit.io/articles/installation/>

Verify both are available in your terminal:

    dotnet --version
    skm --version

### 1.2 Create the Solution and Projects

We will create a solution with two projects: a game application (using SplashKit) and a separate NUnit test project. The separation prevents the “more than one entry point” error.

Open a terminal in the folder where you want the project to live (e.g., `~/deakin/sit771/1.5d/`).

#### Step 1: Create the solution file

    dotnet new sln -n SplashKitNUnit

#### Step 2: Create the game project using the SplashKit template

    mkdir -p game
    cd game
    skm dotnet new
    cd ..

This generates a ready‑to‑run SplashKit console application with the correct `.csproj`, `Program.cs`, and the required `lib/SplashKit.cs` file. The project file will have `<OutputType>Exe</OutputType>`.

Add it to the solution:

    dotnet sln add game/game.csproj

#### Step 3: Create the NUnit test project

    dotnet new nunit -n game.test -o game.test

This creates a class library (no `Main` method) with NUnit and the test SDK already referenced.

Add it to the solution:

    dotnet sln add game.test/game.test.csproj

#### Step 4: Link the test project to the game project

The test project needs to access the classes in the game project (like `ScoreCalculator`, `Physics`, etc.). Add a project reference:

    dotnet add game.test/game.test.csproj reference game/game.csproj

### 1.3 Optional: Add Resource Folders

If your game uses images, sounds, or fonts, generate the required folders:

    cd game
    skm resources
    cd ..

This creates `Resources/images`, `Resources/sounds`, and `Resources/fonts` inside the `game/` folder. Place your assets in the appropriate subfolder.

### 1.4 Restore, Build, and Test

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

### 1.5 Running the Game (Optional)

To launch the SplashKit window and play the game:

    dotnet run --project game

Press the spacebar to jump (if implemented) and close the window to exit.

### 1.6 Understanding the Structure

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

### 1.7 Common Setup Mistakes

| Mistake | Consequence | Fix |
| --- | --- | --- |
| Using `dotnet new console` instead of `skm dotnet new` | Missing `lib/SplashKit.cs`; SplashKit API calls will fail. | Delete the project and re‑run `skm dotnet new`. |
| Placing test files inside the `game/` project | Multiple entry points error (`CS0017`). | Move all test files to `game.test/`. |
| Forgetting to add the project reference | Tests cannot see `Models.ScoreCalculator` etc. | Run `dotnet add game.test/game.test.csproj reference game/game.csproj`. |
| Not restoring after cloning | NuGet packages missing; build fails. | Always run `dotnet restore` first. |

### 1.8 Next Steps

With the setup verified, you are ready to explore the tutorial content. The following sections will explain how to write pure‑logic classes, create NUnit tests, and refactor a tightly‑coupled game object.

---

## Part 2: Separating Pure Logic from SplashKit Side Effects

### 2.1 Why Separate?

SplashKit is a multimedia library that handles windows, input, drawing, and sound. When you mix game logic directly with SplashKit calls, testing becomes difficult. You cannot easily simulate a key press or verify that a score increased without launching a full graphical window. By extracting pure logic into plain C# classes, you gain the ability to test everything quickly and reliably with `dotnet test`.

Pure logic means methods that:

- Depend only on their inputs (parameters and fields).
- Produce predictable outputs (return values or state changes).
- Do not call external systems (file I/O, graphics, audio, network).

In our project, the classes `ScoreCalculator`, `Physics`, `CollisionMath`, and `Player` contain no SplashKit calls. They work entirely with floats, integers, and booleans. This makes them perfect candidates for unit testing.

### 2.2 Overview of the Logic Classes

| Class | Responsibility |
| --- | --- |
| `ScoreCalculator` | Tracks a numeric score, validates coin values, resets. |
| `Physics` | Static constants and methods for gravity and jump velocity. |
| `CollisionMath` | Static methods for circle-rect and rect-rect collision. |
| `Player` | Manages position, velocity, jumping, ground detection, coin count. |

Each class lives in the `SplashKitNUnit.Game.Models` namespace inside the `game/Models/` folder.

### 2.3 Writing Tests for ScoreCalculator

Start with the simplest class: `ScoreCalculator`. Its contract is:

- A new calculator has score 0.
- `CollectCoin(positive)` increases score by that amount.
- `CollectCoin(0)` or `CollectCoin(negative)` throws `ArgumentException`.
- `Reset()` sets score back to 0.

Here is the complete test file `game.test/ScoreCalculatorTests.cs`:

    using NUnit.Framework;
    using SplashKitNUnit.Game.Models;

    namespace SplashKitNUnit.Game.Test
    {
        [TestFixture]
        public class ScoreCalculatorTests
        {
            [Test]
            public void NewCalculator_HasZeroScore()
            {
                var calc = new ScoreCalculator();
                Assert.That(calc.Score, Is.Zero);
            }

            [Test]
            public void CollectCoin_WithPositiveValue_IncreasesScore()
            {
                var calc = new ScoreCalculator();
                calc.CollectCoin(10);
                Assert.That(calc.Score, Is.EqualTo(10));
            }

            [Test]
            public void CollectCoin_MultipleTimes_AccumulatesScore()
            {
                var calc = new ScoreCalculator();
                calc.CollectCoin(5);
                calc.CollectCoin(15);
                Assert.That(calc.Score, Is.EqualTo(20));
            }

            [Test]
            public void CollectCoin_WithZeroValue_ThrowsArgumentException()
            {
                var calc = new ScoreCalculator();
                Assert.Throws<ArgumentException>(() => calc.CollectCoin(0));
            }

            [Test]
            public void CollectCoin_WithNegativeValue_ThrowsArgumentException()
            {
                var calc = new ScoreCalculator();
                Assert.Throws<ArgumentException>(() => calc.CollectCoin(-5));
            }

            [Test]
            public void Reset_SetsScoreToZero()
            {
                var calc = new ScoreCalculator();
                calc.CollectCoin(100);
                calc.Reset();
                Assert.That(calc.Score, Is.Zero);
            }
        }
    }

Notice that each test follows the Arrange-Act-Assert pattern. We never touch SplashKit. The tests run instantly.

### 2.4 Testing Physics Constants and Methods

`Physics` is a static class with constants and one method. Because it has no mutable state, we simply verify that the values are correct.

File `game.test/PhysicsTests.cs`:

    using NUnit.Framework;
    using SplashKitNUnit.Game.Models;

    namespace SplashKitNUnit.Game.Test
    {
        [TestFixture]
        public class PhysicsTests
        {
            [Test]
            public void Gravity_IsPositiveConstant()
            {
                Assert.That(Physics.Gravity, Is.GreaterThan(0));
            }

            [Test]
            public void ApplyGravity_WithZeroDelta_ReturnsSameVelocity()
            {
                float result = Physics.ApplyGravity(100f, 0f);
                Assert.That(result, Is.EqualTo(100f));
            }

            [Test]
            public void ApplyGravity_WithPositiveDelta_IncreasesVelocityDownward()
            {
                float result = Physics.ApplyGravity(0f, 1f);
                Assert.That(result, Is.EqualTo(980f).Within(0.001f));
            }

            [Test]
            public void GetJumpVelocity_ReturnsExpectedConstant()
            {
                Assert.That(Physics.GetJumpVelocity(), Is.EqualTo(-400f));
            }
        }
    }

Floating-point comparisons use `Within(tolerance)` to avoid precision issues.

### 2.5 Testing Collision Detection

`CollisionMath` provides two static methods. We test several scenarios: overlapping, touching edges, far apart.

File `game.test/CollisionMathTests.cs` (excerpt showing key cases):

    [Test]
    public void CircleRectCollision_CenterInsideRect_ReturnsTrue()
    {
        bool result = CollisionMath.CircleRectCollision(150, 150, 10, 100, 100, 80, 120);
        Assert.That(result, Is.True);
    }

    [Test]
    public void CircleRectCollision_FarApart_ReturnsFalse()
    {
        bool result = CollisionMath.CircleRectCollision(50, 50, 10, 200, 200, 30, 30);
        Assert.That(result, Is.False);
    }

    [Test]
    public void RectRectCollision_OverlappingCenters_ReturnsTrue()
    {
        bool result = CollisionMath.RectRectCollision(100, 100, 50, 50, 115, 125, 32, 27);
        Assert.That(result, Is.True);
    }

    [Test]
    public void RectRectCollision_NotOverlapping_ReturnsFalse()
    {
        bool result = CollisionMath.RectRectCollision(10, 16, 24, 36, 95, 42, 19, 14);
        Assert.That(result, Is.False);
    }

The full file in our repository contains eight collision tests covering boundary conditions.

### 2.6 Testing the Player Class

`Player` has state that changes over time. We test construction, jumping rules, gravity updates, and landing.

File `game.test/PlayerTests.cs`:

    [Test]
    public void Constructor_SetsPositionAndDefaults()
    {
        var p = new Player(100, 300);
        Assert.Multiple(() =>
        {
            Assert.That(p.X, Is.EqualTo(100));
            Assert.That(p.Y, Is.EqualTo(300));
            Assert.That(p.VelocityY, Is.Zero);
            Assert.That(p.IsOnGround, Is.True);
            Assert.That(p.CoinsCollected, Is.Zero);
        });
    }

    [Test]
    public void Jump_WhenOnGround_SetsVelocityAndNotOnGround()
    {
        var p = new Player(0, 475);
        p.Jump();
        Assert.Multiple(() =>
        {
            Assert.That(p.VelocityY, Is.EqualTo(-400f));
            Assert.That(p.IsOnGround, Is.False);
        });
    }

    [Test]
    public void Jump_WhenAlreadyAirborne_DoesNothing()
    {
        var p = new Player(0, 465);
        p.Jump();
        float velAfterFirst = p.VelocityY;
        p.Jump(); // should be ignored
        Assert.That(p.VelocityY, Is.EqualTo(velAfterFirst));
    }

    [Test]
    public void Update_WithGravity_MovesPlayerDownward()
    {
        var p = new Player(0, 485);
        p.Update(0.04f);
        Assert.That(p.Y, Is.GreaterThan(485));
        Assert.That(p.VelocityY, Is.GreaterThan(0));
    }

    [Test]
    public void Update_FallsToGround_StopsAtGround()
    {
        var p = new Player(0, 440);
        p.Update(0.75f); // enough time to hit ground at Y=500
        Assert.Multiple(() =>
        {
            Assert.That(p.Y, Is.EqualTo(500));
            Assert.That(p.VelocityY, Is.Zero);
            Assert.That(p.IsOnGround, Is.True);
        });
    }

    [Test]
    public void CollectCoin_IncrementsCount()
    {
        var p = new Player(0, 260);
        p.CollectCoin();
        p.CollectCoin();
        Assert.That(p.CoinsCollected, Is.EqualTo(2));
    }

The key insight: we control `deltaTime` precisely, so we can predict exactly where the player should be after a known number of seconds. This deterministic behaviour is what makes unit testing possible.

### 2.7 What About the Game Loop?

The file `game/Program.cs` contains the SplashKit game loop. It instantiates the same logic classes and calls their methods, but adds drawing and input handling. We do not write unit tests for `Program.cs` because it is tightly coupled to SplashKit. Instead, we trust that if the underlying logic is correct, the visual behaviour will also be correct.

This separation of concerns is the central lesson of Task 1.5D.

### 2.8 Running All Tests

From the solution root, execute:

    dotnet test

You should see:

    Passed! - Failed: 0, Passed: 26, Skipped: 0, Total: 26

Every test covers a specific rule. If a future change breaks a rule, the corresponding test will fail immediately, giving you confidence to refactor safely.

### 2.9 Summary

By pulling pure logic out of SplashKit and into dedicated model classes, we achieved:

- Fast, repeatable unit tests (under 0.2 seconds for 26 tests).
- Clear separation between “what the game does” and “how it draws”.
- Easy verification of edge cases (negative coins, touching collisions, airborne jumps).
- A design that scales well as the game grows.

In the next part (Part 3) we will explore advanced NUnit features such as parameterised tests and custom assertions, and discuss how to extend this pattern to larger projects.

### **Save Your Progress**

Congratulations! You have finished writing the basic tests. To keep your current achievements safe while exploring advanced techniques later without affecting what you've already built, run the following commands to create a backup branch:

    git add .
    git commit -m "Part 2 complete: 26 passing tests"
    git branch part2-complete

You are currently on the `master` (or `main`) branch, and your Part 2 code is now permanently saved in the `part2-complete` branch. When you move on to Part 3, we will create a new branch based on this one.

## Part 3: Advanced NUnit Techniques and Best Practices

In Part 2 we wrote straightforward tests for each logic class. Now we will explore more powerful NUnit features that reduce repetition, improve readability, and catch subtle bugs. These techniques are especially valuable as your test suite grows beyond 26 tests.

### **reparation**

This part will refactor the test code, introducing techniques such as parameterized tests, `[SetUp]`, and `[TestCaseSource]`. These modifications will alter the test files from Part 2, so we will use an independent branch for experimentation.

First, ensure you have completed Part 2 and saved the `part2-complete` branch. Then execute:

    git checkout part2-complete
    git checkout -b part3-advanced

You are now on the `part3-advanced` branch and can safely modify the test files. If you ever want to return to the Part 2 state, simply run `git checkout part2-complete`.

### 3.1 Parameterised Tests with `[TestCase]`

Many tests differ only by input values. Instead of writing five separate methods, you can write one parameterised test.

**Example: Testing `CollectCoin` with various values**

Replace the three separate tests for positive, zero, and negative values with a single parameterised test:

```csharp
[Test]
[TestCase(10, 10)]
[TestCase(1, 1)]
[TestCase(999, 999)]
public void CollectCoin_PositiveValues_IncreasesScoreCorrectly(int coinValue, int expectedScore)
{
    var calc = new ScoreCalculator();
    calc.CollectCoin(coinValue);
    Assert.That(calc.Score, Is.EqualTo(expectedScore));
}

[Test]
[TestCase(0)]
[TestCase(-1)]
[TestCase(-100)]
public void CollectCoin_NonPositiveValues_ThrowsArgumentException(int invalidValue)
{
    var calc = new ScoreCalculator();
    Assert.Throws<ArgumentException>(() => calc.CollectCoin(invalidValue));
}
```

This reduces boilerplate and makes it easy to add new test cases later.

### **Example: Collision detection with many scenarios**

```csharp
private static IEnumerable<TestCaseData> CircleRectCollisionCases()
{
    yield return new TestCaseData(150f, 150f, 10f, 100f, 100f, 80f, 120f, true)
        .SetName("CenterInsideRect");
    yield return new TestCaseData(50f, 50f, 10f, 200f, 200f, 30f, 30f, false)
        .SetName("FarApart");
    yield return new TestCaseData(220f, 217f, 12f, 206f, 199f, 35f, 41f, true)
        .SetName("CornerOverlap");
    yield return new TestCaseData(104f, 172f, 9f, 111f, 157f, 65f, 76f, true)
        .SetName("TouchingEdge");
}

[Test, TestCaseSource(nameof(CircleRectCollisionCases))]
public void CircleRectCollision_VariousScenarios_ReturnsExpected(
    float cx, float cy, float radius,
    float rx, float ry, float rw, float rh,
    bool expected)
{
    bool result = CollisionMath.CircleRectCollision(cx, cy, radius, rx, ry, rw, rh);
    Assert.That(result, Is.EqualTo(expected));
}
```

Each case gets a descriptive name, making failure messages easier to understand.

### 3.2 Using `[TestCaseSource]` for Data-Driven Tests

When you have many test cases or complex data, `[TestCase]` becomes unwieldy. Use `[TestCaseSource]` to load data from a separate method, field, or property.

### **Example: Loading collision test data from a method**

We already saw this above. The same pattern applies to rectangle-rectangle collisions:

```csharp
private static IEnumerable<TestCaseData> RectRectCollisionCases()
{
    yield return new TestCaseData(100f, 103f, 54f, 49f, 119f, 128f, 32f, 22f, true)
        .SetName("OverlappingCenters");
    yield return new TestCaseData(0f, 56f, 47f, 58f, 45f, 70f, 50f, 28f, true)
        .SetName("TouchingEdges");
    yield return new TestCaseData(94f, 51f, 23f, 14f, 185f, 44f, 19f, 11f, false)
        .SetName("NotOverlapping");
    yield return new TestCaseData(70f, 69f, 86f, 73f, 84f, 88f, 23f, 21f, true)
        .SetName("OneContainsOther");
}

[Test, TestCaseSource(nameof(RectRectCollisionCases))]
public void RectRectCollision_VariousScenarios_ReturnsExpected(
    float x1, float y1, float w1, float h1,
    float x2, float y2, float w2, float h2,
    bool expected)
{
    bool result = CollisionMath.RectRectCollision(x1, y1, w1, h1, x2, y2, w2, h2);
    Assert.That(result, Is.EqualTo(expected));
}
```

### 3.3 Floating-Point Comparisons with `Within`

Physics calculations produce floating-point results that may differ slightly due to rounding. Always use `Within` for equality checks on floats.

```csharp
Assert.That(result, Is.EqualTo(expected).Within(0.0001f));
```

You can also use `Percent`:

```csharp
Assert.That(result, Is.EqualTo(expected).Within(1.0).Percent);
```

In our `PhysicsTests`, we already used `Within(0.001f)`. This is good practice for any float-based logic.

### 3.4 Testing State Changes with `Assert.Multiple`

When a method changes multiple properties, use `Assert.Multiple` to report all failures at once rather than stopping at the first one.

We already used this in `PlayerTests`. Another example:

```csharp
[Test]
public void Player_AfterFullJumpCycle_ReturnsToGround()
{
    var p = new Player(0, 494);
    p.Jump();
    p.Update(0.025f);  // initial upward movement
    p.Update(0.835f);  // enough time to go up and come back down
    Assert.Multiple(() =>
    {
        Assert.That(p.Y, Is.EqualTo(500));
        Assert.That(p.VelocityY, Is.Zero);
        Assert.That(p.IsOnGround, Is.True);
    });
}
```

Without `Assert.Multiple`, if Y was wrong, you would never see that VelocityY was also wrong until fixing Y first.

### 3.5 Custom Assertions with Helper Methods

If you repeatedly check the same combination of conditions, extract a helper method.

```csharp
private static void AssertPlayerState(Player player, float expectedY, float expectedVelY, bool expectedOnGround)
{
    Assert.Multiple(() =>
    {
        Assert.That(player.Y, Is.EqualTo(expectedY));
        Assert.That(player.VelocityY, Is.EqualTo(expectedVelY).Within(0.001f));
        Assert.That(player.IsOnGround, Is.EqualTo(expectedOnGround));
    });
}
```

Then your tests become cleaner:

```csharp
[Test]
public void Update_AfterLanding_StateIsCorrect()
{
    var p = new Player(0, 448);
    p.Update(0.702f);
    AssertPlayerState(p, 500f, 0f, true);
}
```

### 3.6 Test Lifecycle: `[SetUp]` and `[TearDown]`

When many tests share the same arrangement, use `[SetUp]` to reduce duplication.

```csharp
public class ScoreCalculatorTests
{
    private ScoreCalculator _calc;

    [SetUp]
    public void SetUp()
    {
        _calc = new ScoreCalculator();
    }

    [Test]
    public void NewCalculator_HasZeroScore()
    {
        Assert.That(_calc.Score, Is.Zero);
    }

    [Test]
    [TestCase(10, 10)]
    [TestCase(1, 1)]
    [TestCase(999, 999)]
    public void CollectCoin_PositiveValues_IncreasesScoreCorrectly(int coinValue, int expectedScore)
    {
        _calc.CollectCoin(coinValue);
        Assert.That(_calc.Score, Is.EqualTo(expectedScore));
    }

    // ... more tests using _calc ...
}
```

Be careful: `[SetUp]` runs before **each** test, ensuring tests are isolated. Do not put shared state that accumulates across tests.

### 3.7 Ignoring Tests Temporarily

If a test is failing and you want to commit your code without breaking the build, use `[Ignore]` with a reason:

```csharp
[Test]
[Ignore("Refactoring collision logic – will fix next commit")]
public void CircleRectCollision_EdgeCase_ReturnsTrue()
{
    // ...
}
```

This documents known issues and prevents the test from blocking CI pipelines.

### 3.8 Testing Private Methods – Should You?

Generally, test only public behaviour. If a private method is complex, consider extracting it into a separate class or making it internal with `InternalsVisibleTo`. In our project, all logic is already public, so this is not an issue.

### 3.9 Organising Large Test Suites

As your project grows, organise tests by feature:

```text
game.test/
├── Models/
│   ├── ScoreCalculatorTests.cs
│   ├── PhysicsTests.cs
│   ├── CollisionMathTests.cs
│   └── PlayerTests.cs
├── Services/          (future: tests for service classes)
└── Integration/       (future: tests that span multiple classes)
```

Use namespaces consistently: `SplashKitNUnit.Game.Test.Models`, `SplashKitNUnit.Game.Test.Services`, etc.

### 3.10 Continuous Integration

Consider adding a GitHub Actions workflow to run tests automatically on every push:

```yaml
# .github/workflows/dotnet.yml
name: .NET
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal
```

This ensures your tests always pass before merging.

### 3.11 Final Verification

After applying all Part 3 changes, run:

```bash
dotnet test
```

You should see:

```
Passed! - Failed: 0, Passed: 33, Skipped: 0, Total: 33
```

The test count increased from 26 to 33 because we added new parameterised scenarios (extra collision cases and player landing cases). All tests pass with the corrected test data.

### 3.12 Summary of Part 3

| Technique | Benefit |
| --- | --- |
| `[TestCase]` | Write one test method for many inputs |
| `[TestCaseSource]` | Load complex or numerous test cases from code |
| `Within` tolerance | Safe float comparisons |
| `Assert.Multiple` | See all failures at once |
| Helper assertion methods | Reduce duplication, improve readability |
| `[SetUp]` | Share arrangement code across tests |
| `[Ignore]` | Temporarily skip broken tests |
| CI pipeline | Automatically verify tests on each commit |

These patterns turn a basic test suite into a robust safety net. In Part 4 we will look at refactoring real game code using tests as a guide, demonstrating how TDD (Test-Driven Development) helps you design cleaner abstractions.

---

### Note on Branch Strategy

Part 3 modifies the test files from Part 2. To keep both versions clean, we recommend branching:

- After Part 2: `git checkout -b part2-complete`
- Before Part 3: `git checkout part2-complete && git checkout -b part3-advanced`

This preserves the original 26-test suite and allows independent exploration of advanced techniques.

---

## **Now switch to main branch.**
