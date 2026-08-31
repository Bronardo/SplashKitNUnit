# Writing NUnit Unit Tests for SplashKit Game Logic: From Coupling to Testability

> For SIT771 students who already know how to build small games with SplashKit but struggle with code that breaks everywhere when you change one thing.

## Have You Ever Had This Frustration?

Your `Player.Update()` calls both `SplashKit.KeyDown()` and `SplashKit.Draw()` — which means to test whether your jump logic works, you must first open a window, press a real key, and look at the screen. That's not testing. That's playing the game.

**Root cause**: Game logic (scoring, collision, physics) is tightly coupled with SplashKit I/O operations (window, keyboard, drawing).

**Solution**: Separate "pure logic" from SplashKit side effects, then write NUnit unit tests for the pure logic.

**After reading this tutorial you will be able to:**

1. Identify which parts of your game code are "pure logic" (testable) vs "SplashKit side effects" (not testable / not worth testing)
2. Use NUnit's `[Test]`, `[TestCase]`, and `Assert.That` to write tests for pure logic
3. Refactor a tightly-coupled GameObject so it can run both in the game and under `dotnet test`

---

## 1. Concept: What Is "Testable Game Logic"?

| Code Type | Example | Can You Test It in NUnit? |
| --- | --- | --- |
| Pure logic | Scoring, collision detection, state machines, physics calculations | ✅ Yes, and you should |
| SplashKit side effects | `OpenWindow`, `KeyDown`, `Draw`, `LoadBitmap`, `PlaySoundEffect` | ❌ No, don't test these in NUnit |

Why can't SplashKit side effects be tested in NUnit?

- `KeyDown` depends on the `ProcessEvents` event loop — NUnit has no event loop
- `LoadBitmap` depends on files in `Resources/images/` — the test environment might not have them
- `PlaySoundEffect` needs an audio backend — headless environments cannot initialise it
- `OpenWindow` needs a graphics context — CI servers usually have no display

**Core principle**: Classes inside the `Models/` folder should **not** have `using SplashKitSDK;` (except for pure geometry functions — see Example C).

---

## 2. Project Setup

### Create a SplashKit project skeleton

```bash
skm dotnet new
```

### Add NUnit packages

```bash
cd MyGame
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk
```

### Recommended directory structure

```cli
MyGame/
├── Game.cs              # Game entry point, calls SplashKit API (only file with side effects)
├── Models/              # Pure logic layer, does NOT reference SplashKitSDK
│   ├── ScoreCalculator.cs
│   ├── Physics.cs
│   └── CollisionMath.cs
├── Entities/            # Entity layer, dependencies injected via constructor
│   └── Player.cs
└── Tests/               # NUnit test project
    ├── ScoreCalculatorTests.cs
    ├── PhysicsTests.cs
    ├── CollisionMathTests.cs
    └── PlayerTests.cs
```

> 💡 **Best practice**: Classes in `Models/` and `Entities/` should not contain any `using SplashKitSDK;` statement. SplashKit only appears in `Game.cs`.

---

## 3. Hands-On Examples: Three Levels of Difficulty

### Example A: ScoreCalculator — Simplest Pure Logic

```csharp
// Models/ScoreCalculator.cs
using System;

namespace MyGame.Models
{
    public class ScoreCalculator
    {
        public int Score { get; private set; }

        public void CollectCoin(int value = 10)
        {
            if (value <= 0)
                throw new ArgumentException("Coin value must be positive");
            Score += value;
        }

        public void HitObstacle(int penalty = 15)
        {
            Score = Math.Max(0, Score - penalty);
        }
    }
}
```

Corresponding NUnit tests:

```csharp
// Tests/ScoreCalculatorTests.cs
using NUnit.Framework;
using MyGame.Models;

[TestFixture]
public class ScoreCalculatorTests
{
    [Test]
    public void CollectCoin_DefaultValue_ScoreIncreasesBy10()
    {
        var calc = new ScoreCalculator();
        calc.CollectCoin();
        Assert.That(calc.Score, Is.EqualTo(10));
    }

    [TestCase(5, 5)]
    [TestCase(20, 20)]
    [TestCase(100, 100)]
    public void CollectCoin_CustomValue_ScoreMatches(int value, int expected)
    {
        var calc = new ScoreCalculator();
        calc.CollectCoin(value);
        Assert.That(calc.Score, Is.EqualTo(expected));
    }

    [Test]
    public void HitObstacle_ScoreNeverNegative()
    {
        var calc = new ScoreCalculator();
        calc.HitObstacle(15);  // 0 - 15 would be negative
        Assert.That(calc.Score, Is.EqualTo(0));  // clamped to 0
    }

    [Test]
    public void CollectCoin_NegativeValue_ThrowsException()
    {
        var calc = new ScoreCalculator();
        Assert.That(() => calc.CollectCoin(-5), Throws.ArgumentException);
    }
}
```

Run the tests:

```bash
dotnet test
```

All tests pass. No window, no keyboard, no images — just pure arithmetic.

---

### Example B: Physics — Gravity and Jumping

Based on typical parameters from the SIT771 Doodle Jump tutorial: `Gravity = 0.5f`, `JumpStrength = -15f`.

```csharp
// Models/Physics.cs
namespace MyGame.Models
{
    public static class Physics
    {
        public const float Gravity = 0.5f;
        public const float JumpStrength = -15f;

        /// <summary>
        /// Apply gravity: velocity increases (towards positive, i.e. downwards)
        /// </summary>
        public static float ApplyGravity(float currentVelocity)
        {
            return currentVelocity + Gravity;
        }

        /// <summary>
        /// Jump: returns upward initial velocity
        /// </summary>
        public static float Jump()
        {
            return JumpStrength;
        }

        /// <summary>
        /// Calculate vertical displacement given initial velocity and elapsed time (in ms)
        /// s = v0 * t + 0.5 * g * t^2
        /// </summary>
        public static float CalculateDisplacement(float initialVelocity, float timeMs)
        {
            float timeSec = timeMs / 1000f;
            return initialVelocity * timeSec + 0.5f * Gravity * timeSec * timeSec;
        }
    }
}
```

NUnit tests:

```csharp
// Tests/PhysicsTests.cs
using NUnit.Framework;
using MyGame.Models;

[TestFixture]
public class PhysicsTests
{
    [Test]
    public void Jump_InitialVelocity_IsNegative()
    {
        float v = Physics.Jump();
        Assert.That(v, Is.LessThan(0));
        Assert.That(v, Is.EqualTo(Physics.JumpStrength));
    }

    [Test]
    public void ApplyGravity_VelocityIncreases()
    {
        float v0 = -10f;       // moving upward
        float v1 = Physics.ApplyGravity(v0);
        Assert.That(v1, Is.GreaterThan(v0));  // gravity pulls velocity towards positive
    }

    [Test]
    public void ApplyGravity_AfterMultipleFrames_VelocityBecomesPositive()
    {
        float v = Physics.JumpStrength;  // -15
        for (int i = 0; i < 40; i++)     // about 40 frames later velocity turns positive
        {
            v = Physics.ApplyGravity(v);
        }
        Assert.That(v, Is.GreaterThan(0));  // eventually falling down
    }

    [TestCase(0, 0, 0)]           // stationary
    [TestCase(-15, 500, -7.4375)] // 0.5 seconds after jump
    [TestCase(-15, 1500, -21.9375)] // 1.5 seconds after jump
    public void CalculateDisplacement_ReturnsExpected(float v0, float t, float expected)
    {
        float d = Physics.CalculateDisplacement(v0, t);
        Assert.That(d, Is.EqualTo(expected).Within(0.001));
    }
}
```

> 💡 **Teaching point**: The parameterised tests for `CalculateDisplacement` cover three cases — stationary, rising phase, and falling phase. Students can intuitively see the physics formula working.

---

### Example C: CollisionMath — Using SplashKit Geometry APIs Without Opening a Window

SplashKit provides some pure geometry functions (e.g., `CirclesIntersect`, `RectangleFrom`) that do not depend on windows and can be safely used in tests.

```csharp
// Models/CollisionMath.cs
using SplashKitSDK;  // Note: only using geometry calculations, no window creation

namespace MyGame.Models
{
    public static class CollisionMath
    {
        /// <summary>
        /// Check if two rectangles overlap (AABB collision)
        /// </summary>
        public static bool RectanglesOverlap(Rectangle a, Rectangle b)
        {
            return !(a.X + a.Width < b.X ||
                     b.X + b.Width < a.X ||
                     a.Y + a.Height < b.Y ||
                     b.Y + b.Height < a.Y);
        }

        /// <summary>
        /// Check if two circles intersect (uses SplashKit built-in function)
        /// </summary>
        public static bool CirclesCollide(double x1, double y1, double r1,
                                           double x2, double y2, double r2)
        {
            return SplashKit.CirclesIntersect(x1, y1, r1, x2, y2, r2);
        }
    }
}
```

NUnit tests:

```csharp
// Tests/CollisionMathTests.cs
using NUnit.Framework;
using SplashKitSDK;
using MyGame.Models;

[TestFixture]
public class CollisionMathTests
{
    [Test]
    public void RectanglesOverlap_Overlapping_ReturnsTrue()
    {
        var a = SplashKit.RectangleFrom(0, 0, 50, 50);
        var b = SplashKit.RectangleFrom(25, 25, 50, 50);
        Assert.That(CollisionMath.RectanglesOverlap(a, b), Is.True);
    }

    [Test]
    public void RectanglesOverlap_NotOverlapping_ReturnsFalse()
    {
        var a = SplashKit.RectangleFrom(0, 0, 30, 30);
        var b = SplashKit.RectangleFrom(100, 200, 30, 30);
        Assert.That(CollisionMath.RectanglesOverlap(a, b), Is.False);
    }

    [Test]
    public void RectanglesOverlap_TouchingEdges_ReturnsFalse()
    {
        var a = SplashKit.RectangleFrom(0, 0, 50, 50);
        var b = SplashKit.RectangleFrom(50, 0, 50, 50);  // exactly touching right edge
        Assert.That(CollisionMath.RectanglesOverlap(a, b), Is.False);
    }

    [Test]
    public void CirclesCollide_Overlapping_ReturnsTrue()
    {
        Assert.That(CollisionMath.CirclesCollide(0, 0, 10, 5, 0, 10), Is.True);
    }

    [Test]
    public void CirclesCollide_NotOverlapping_ReturnsFalse()
    {
        Assert.That(CollisionMath.CirclesCollide(0, 0, 10, 100, 100, 10), Is.False);
    }
}
```

> ⚠️ **Important note**: Here we `using SplashKitSDK`, but we only call `CirclesIntersect` and `RectangleFrom` — both are **pure functions** that do not involve windows, input, or drawing. This is the only scenario where referencing SplashKit is allowed in the Models layer.

---

## 4. Refactoring: Turning a Tightly-Coupled GameObject into a Testable One

### ❌ Anti-pattern: Logic coupled with SplashKit

Below is typical "bad smell" code from the SIT771 Doodle Jump tutorial:

```csharp
// ❌ Anti-pattern: logic coupled with SplashKit, impossible to unit-test
public class Player
{
    public float Y { get; set; }
    private float _yVelocity;
    private const float JumpStrength = -15f;

    public void Update()
    {
        if (SplashKit.KeyDown(KeyCode.SpaceKey))  // depends on input
        {
            _yVelocity = JumpStrength;
        }
        _yVelocity += 0.5f;  // gravity
        Y += _yVelocity;
        SplashKit.DrawCircle(Color.Red, 400, Y, 20);  // depends on drawing
    }
}
```

Problems with this code:

- `KeyDown` only works inside the SplashKit event loop — you cannot simulate a key press in a test
- `DrawCircle` requires an open window — the test environment has none
- You cannot verify "does pressing Space set the correct velocity?" without opening a window and pressing a real key

### ✅ Correct pattern: Dependency injection decouples logic

```csharp
// Entities/Player.cs — pure logic, no SplashKit reference
namespace MyGame.Entities
{
    public class Player
    {
        public float Y { get; set; }
        public float Velocity { get; set; }

        /// <summary>
        /// Decide whether to jump based on external input
        /// </summary>
        /// <param name="isJumpPressed">Key state passed from outside</param>
        public void JumpIfInput(bool isJumpPressed)
        {
            if (isJumpPressed && Velocity >= 0)  // only jump when falling or stationary
            {
                Velocity = Models.Physics.JumpStrength;
            }
        }

        /// <summary>
        /// Update physical state
        /// </summary>
        /// <param name="timeMs">Elapsed time since last frame (milliseconds)</param>
        public void UpdatePhysics(float timeMs)
        {
            Velocity = Models.Physics.ApplyGravity(Velocity);
            Y += Models.Physics.CalculateDisplacement(Velocity, timeMs);
        }
    }
}
```

Corresponding NUnit tests:

```csharp
// Tests/PlayerTests.cs
using NUnit.Framework;
using MyGame.Entities;

[TestFixture]
public class PlayerTests
{
    [Test]
    public void JumpIfInput_JumpPressedAndStationary_SetsNegativeVelocity()
    {
        var player = new Player { Velocity = 0 };
        player.JumpIfInput(isJumpPressed: true);
        Assert.That(player.Velocity, Is.LessThan(0));
        Assert.That(player.Velocity, Is.EqualTo(-15f));  // JumpStrength
    }

    [Test]
    public void JumpIfInput_JumpPressedWhileRising_IgnoresInput()
    {
        var player = new Player { Velocity = -10 };  // currently rising
        player.JumpIfInput(isJumpPressed: true);
        Assert.That(player.Velocity, Is.EqualTo(-10));  // no double-jump
    }

    [Test]
    public void JumpIfInput_JumpNotPressed_VelocityUnchanged()
    {
        var player = new Player { Velocity = 5 };  // currently falling
        player.JumpIfInput(isJumpPressed: false);
        Assert.That(player.Velocity, Is.EqualTo(5));
    }

    [Test]
    public void UpdatePhysics_GravityApplied_VelocityIncreases()
    {
        var player = new Player { Velocity = 0, Y = 300 };
        player.UpdatePhysics(16);  // roughly one frame at 60 FPS
        Assert.That(player.Velocity, Is.EqualTo(0.5f));  // gravity applied
        Assert.That(player.Y, Is.EqualTo(300 + 0.000064f).Within(0.001));  // tiny displacement
    }

    [Test]
    public void UpdatePhysics_JumpThenFall_SimulatesArc()
    {
        var player = new Player { Velocity = 0, Y = 300 };
        
        // Frame 1: jump
        player.JumpIfInput(isJumpPressed: true);
        player.UpdatePhysics(16);
        Assert.That(player.Velocity, Is.LessThan(0));   // moving upward
        Assert.That(player.Y, Is.LessThan(300));        // Y decreased
        
        // Simulate 59 more frames (~1 second total)
        for (int i = 0; i < 59; i++)
        {
            player.UpdatePhysics(16);
        }
        Assert.That(player.Velocity, Is.GreaterThan(0));  // finally falling
        Assert.That(player.Y, Is.GreaterThan(300));       // back below starting point
    }
}
```

### Using the refactored Player in Game.cs

All SplashKit side effects are concentrated in the single entry-point file:

```csharp
// Game.cs — SplashKit side effects live here only
using SplashKitSDK;
using MyGame.Entities;

public class Program
{
    public static void Main()
    {
        Window window = new Window("My Game", 800, 600);
        Player player = new Player { Y = 350, Velocity = 0 };

        while (!SplashKit.QuitRequested())
        {
            SplashKit.ProcessEvents();

            // Sample input, pass to pure logic
            bool spacePressed = SplashKit.KeyDown(KeyCode.SpaceKey);
            player.JumpIfInput(spacePressed);

            // Update physics (assume 60 FPS, ~16ms per frame)
            player.UpdatePhysics(16);

            // Draw (the only place with side effects)
            SplashKit.ClearWindow(window, Color.White);
            SplashKit.DrawCircle(Color.Red, 400, player.Y, 20);
            SplashKit.RefreshWindow(window, 60);
        }
        window.Close();
    }
}
```

> 💡 **Key change**: `Game.cs` is responsible for "sample input → call pure logic → draw results"; `Player` no longer knows about SplashKit. In tests we simply pass `true/false` to simulate key presses.

---

## 5. Common Mistakes

These are pitfalls students often encounter. Knowing them in advance saves hours of debugging.

### ❌ Mistake 1: Calling `OpenWindow` inside NUnit tests

```csharp
// ❌ Never do this
[Test]
public void BadTest()
{
    Window w = new Window("Test", 800, 600);  // test runner has no graphics context
    // ...
}
```

**Consequence**: Test hangs or crashes; CI server cannot run it.

**Correct approach**: Pure logic does not depend on any Window.

### ❌ Mistake 2: Trying to test `KeyDown`

```csharp
// ❌ Never do this
[Test]
public void BadTest()
{
    bool pressed = SplashKit.KeyDown(KeyCode.SpaceKey);  // needs event loop
    Assert.That(pressed, Is.True);
}
```

**Correct approach**: Input is an external side effect. Sample it in the Game layer and pass the boolean to pure logic. In tests, directly pass `true` or `false`.

### ❌ Mistake 3: `LoadBitmap` fails because file not found

SplashKit resource path conventions:

- Images must be in `Resources/images/`
- Sounds must be in `Resources/sounds/`
- Fonts must be in `Resources/fonts/`

**Correct approach**: Pure logic tests never need to load bitmaps. If you must test resource-loading logic, consider passing the file path as a parameter.

### ❌ Mistake 4: Using outdated Assert syntax

```csharp
// ❌ Old style
Assert.AreEqual(10, calc.Score);

// ✅ New style (constraint model, better error messages)
Assert.That(calc.Score, Is.EqualTo(10));
```

Modern NUnit recommends `Assert.That(actual, constraint)`.

### ❌ Mistake 5: Timer-related logic hard to test

SplashKit's `CreateTimer`/`StartTimer` uses real system time. Do not wait for real time to pass in a test.

**Correct approach**: Pass time as a parameter (e.g., `UpdatePhysics(float timeMs)`), and supply fixed values in tests.

---

## 6. Try It Yourself (Exercises)

These exercises have no provided answers. They are designed for you to practise independently — consistent with the Task 1.5D spirit of "helping someone find the solution themselves".

### Exercise 1: Combo System

Extend `ScoreCalculator`: when coins are collected within 500ms of each other, the coin value doubles each time (first coin 10, second 20, third 40, ...). The combo resets if the gap exceeds 500ms.

- Hint: you'll need a `LastCoinTime` property and an `UpdateTime(float deltaMs)` method
- Write NUnit tests covering: consecutive fast collections, long gap resets, correct score accumulation

### Exercise 2: Triangle–Rectangle Collision

SplashKit provides `TriangleRectangleIntersect(Triangle tri, Rectangle rect)`. Wrap it into a method `CollisionMath.TriangleHitsRect(Triangle tri, Rectangle rect)` and write tests covering:

- Triangle completely inside rectangle
- Triangle partially overlapping rectangle
- Triangle completely outside rectangle
- Triangle vertex touching rectangle edge

### Exercise 3: Refactor Your Own Game

Take a SplashKit game you've written before. Identify at least 3 pieces of pure logic that can be extracted from SplashKit dependency. For example:

- Scoring system
- Health management
- Enemy AI decisions (without drawing)
- Level progress tracking

Write NUnit tests for each extracted class.

---

## Summary

| Principle | Explanation |
| --- | --- |
| Separation of concerns | Pure logic goes in `Models/`, SplashKit side effects stay in `Game.cs` |
| Dependency injection | Input (keys, time) are passed as parameters; do not call SplashKit from inside logic |
| Only test pure logic | Never attempt to test `KeyDown`, `Draw`, `OpenWindow` |
| Parameterised tests | Use `[TestCase]` to cover boundary conditions |
| Time as a parameter | Avoid using real clocks inside tests |

Next time you open the SIT771 Doodle Jump tutorial, try this thought experiment: **If I moved this class into the `Models/` folder, would it still compile?** If not, it's coupled to a SplashKit side effect — and that's a signal to refactor.

---

## Appendix: How to Use This Tutorial as Task 1.5D Evidence

1. **The Markdown file itself**: Format it according to the SplashKit contribution guidelines so it can be published on the SplashKit website
2. **Accompanying GitHub repository**: Contains a runnable `MyGame` project and `Tests` project — `dotnet test` shows all green
3. **Discussion board answers**: Actively answer 3–5 questions on the SIT771 forum about "how to organise SplashKit code / how to test it" — take screenshots
4. **Tutor support email**: If you helped classmates refactor their code using this approach at the Help Hub, ask the tutor to send a supporting email

---

*Happy testing! Remember: `dotnet test` first, then `skm dotnet run`.*
