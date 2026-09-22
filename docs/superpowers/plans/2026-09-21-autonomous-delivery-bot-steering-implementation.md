# Autonomous Delivery Bot Steering Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Unity 6 training-yard scene where a yellow NPC uses Arrive, Wander, and SphereCast obstacle avoidance with inspectable parameters and visual debugging.

**Architecture:** `SteeringAgent` owns desired velocity, acceleration, velocity, movement, rotation, behaviour colour, and agent Gizmos. `SteeringSensor` owns local obstacle detection and returns an avoidance direction. A dedicated editor builder creates an open arena, Player, NPC, obstacle layer, U-shape limitation example, materials, and component wiring; it does not create a NavMesh or attach a NavMeshAgent.

**Tech Stack:** Unity 6, C#, Universal Render Pipeline, Unity Input System, Unity Test Framework/NUnit, Unity Editor APIs.

**Spec:** `docs/superpowers/specs/2026-09-21-autonomous-delivery-bot-steering-design.md`

## Global Constraints

- Preserve `Player` and `Obstacle` layer names.
- NPC movement must be calculated from velocity; do not use NavMesh, NavMeshAgent, or pathfinding.
- Every mandatory task parameter must be a serialized Inspector field.
- Player uses `CharacterController` and keyboard input.
- Obstacle detection uses `Physics.SphereCast` with an Inspector-configured mask.
- Gizmos show stop/slow radii, velocity, desired velocity, target direction, sensor sweep, and detection hit.
- Do not commit or push unless the user explicitly requests it after reviewing the changes.

## Review Focus

- A target inside Stop Radius must produce zero desired velocity, not jitter.
- Slow Radius less than or equal to Stop Radius must still produce finite, bounded velocity.
- Acceleration and movement must be delta-time based.
- A direct obstacle must produce a non-zero lateral avoidance direction.
- An obstacle behind or outside Sensor Distance must not trigger avoidance.

---

## File Structure

| File | Responsibility |
| --- | --- |
| `Assets/Scripts/SimplePlayerController.cs` | WASD/arrow Player movement through `CharacterController`. |
| `Assets/Scripts/SteeringSensor.cs` | SphereCast, avoidance direction, hit state, sensor Gizmos. |
| `Assets/Scripts/SteeringAgent.cs` | Arrive, Wander, integration, rotation, colour state, agent Gizmos. |
| `Assets/Editor/AutonomousSteeringSceneBuilder.cs` | Builds Player, NPC, training yard, materials, and scene. |
| `Assets/Tests/EditMode/SteeringAgentTests.cs` | Pure Arrive and acceleration calculations. |
| `Assets/Tests/PlayMode/SteeringSensorTests.cs` | SphereCast behavior against real colliders. |
| `Assets/Scenes/Praktikum03_AutonomousSteering.unity` | Generated practical-delivery scene. |

### Task 1: Implement and Test Arrive Math

**Files:**
- Create: `Assets/Scripts/SteeringAgent.cs`
- Create: `Assets/Tests/EditMode/SteeringAgentTests.cs`

**Interfaces:**
- Produces `public static Vector3 CalculateArriveVelocity(Vector3 agentPosition, Vector3 targetPosition, float maxSpeed, float slowRadius, float stopRadius)`.
- Produces `public static Vector3 CalculateAcceleration(Vector3 currentVelocity, Vector3 desiredVelocity, float maxAcceleration)`.
- Produces `public void Configure(Transform target, SteeringSensor steeringSensor, Renderer[] colourRenderers)`.

- [ ] **Step 1: Write the failing Edit Mode tests.**

```csharp
[Test]
public void CalculateArriveVelocity_InsideStopRadius_ReturnsZero()
{
    Vector3 result = SteeringAgent.CalculateArriveVelocity(Vector3.zero,
        new Vector3(0.5f, 0f, 0f), 4f, 5f, 1f);
    Assert.That(result, Is.EqualTo(Vector3.zero));
}

[Test]
public void CalculateArriveVelocity_InSlowRadius_ScalesSpeedByDistance()
{
    Vector3 result = SteeringAgent.CalculateArriveVelocity(Vector3.zero,
        new Vector3(3f, 0f, 0f), 4f, 6f, 1f);
    Assert.That(result.magnitude, Is.EqualTo(2f).Within(0.001f));
    Assert.That(result.normalized, Is.EqualTo(Vector3.right));
}
```

- [ ] **Step 2: Run Unity Test Runner > EditMode > `SteeringAgentTests`.**

Expected: compilation fails because `SteeringAgent` does not exist.

- [ ] **Step 3: Implement the pure math API.**

```csharp
public static Vector3 CalculateArriveVelocity(Vector3 agentPosition, Vector3 targetPosition,
    float maxSpeed, float slowRadius, float stopRadius)
{
    Vector3 offset = Vector3.ProjectOnPlane(targetPosition - agentPosition, Vector3.up);
    float distance = offset.magnitude;
    if (distance <= Mathf.Max(0f, stopRadius) || distance <= 0.0001f) return Vector3.zero;
    float safeSlowRadius = Mathf.Max(slowRadius, stopRadius + 0.0001f);
    float speed = distance >= safeSlowRadius ? maxSpeed : maxSpeed * distance / safeSlowRadius;
    return offset.normalized * Mathf.Max(0f, speed);
}

public static Vector3 CalculateAcceleration(Vector3 currentVelocity,
    Vector3 desiredVelocity, float maxAcceleration)
{
    return Vector3.ClampMagnitude(desiredVelocity - currentVelocity, Mathf.Max(0f, maxAcceleration));
}
```

Add serialized fields: target, useTarget, maxSpeed, maxAcceleration, turnSpeed,
slowRadius, stopRadius, wanderSpeed, wanderChangeInterval, wanderAngleChange,
avoidanceWeight, sensor, velocity, desiredVelocity.

- [ ] **Step 4: Add edge-case tests.**

```csharp
[Test]
public void CalculateArriveVelocity_InvalidRadiusOrder_ReturnsFiniteBoundedResult()
{
    Vector3 result = SteeringAgent.CalculateArriveVelocity(Vector3.zero,
        new Vector3(2f, 0f, 0f), 4f, 1f, 1f);
    Assert.That(float.IsFinite(result.x), Is.True);
    Assert.That(result.magnitude, Is.LessThanOrEqualTo(4f));
}

[Test]
public void CalculateAcceleration_OverLimit_ClampsMagnitude()
{
    Vector3 result = SteeringAgent.CalculateAcceleration(Vector3.zero, Vector3.right * 10f, 3f);
    Assert.That(result.magnitude, Is.EqualTo(3f).Within(0.001f));
}
```

- [ ] **Step 5: Run the Edit Mode suite.**

Expected: all four tests pass.

- [ ] **Step 6: Show the diff to the user; do not commit.**

### Task 2: Implement and Test SphereCast Avoidance

**Files:**
- Create: `Assets/Scripts/SteeringSensor.cs`
- Create: `Assets/Tests/PlayMode/SteeringSensorTests.cs`
- Modify: `Assets/Scripts/SteeringAgent.cs`

**Interfaces:**
- Produces `public bool TryGetAvoidance(out Vector3 avoidanceDirection)`.
- Produces `public bool IsAvoiding { get; }` and `public RaycastHit LastHit { get; }`.
- `SteeringAgent` calls `sensor.TryGetAvoidance(out Vector3 avoidance)`.

- [ ] **Step 1: Write failing Play Mode test with a real obstacle.**

```csharp
[UnityTest]
public IEnumerator TryGetAvoidance_ObstacleAhead_ReturnsLateralDirection()
{
    GameObject agent = new GameObject("Agent");
    SteeringSensor sensor = agent.AddComponent<SteeringSensor>();
    sensor.Configure(5f, 0.5f, 1 << LayerMask.NameToLayer("Obstacle"));
    GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
    obstacle.layer = LayerMask.NameToLayer("Obstacle");
    obstacle.transform.position = new Vector3(0f, 0f, 2f);
    Physics.SyncTransforms();
    yield return null;
    Assert.That(sensor.TryGetAvoidance(out Vector3 direction), Is.True);
    Assert.That(direction.sqrMagnitude, Is.GreaterThan(0.001f));
}
```

- [ ] **Step 2: Run Unity Test Runner > PlayMode > `SteeringSensorTests`.**

Expected: compilation fails because `SteeringSensor` does not exist.

- [ ] **Step 3: Implement `SteeringSensor`.**

```csharp
public bool TryGetAvoidance(out Vector3 avoidanceDirection)
{
    Vector3 origin = transform.position + Vector3.up * sensorHeight;
    if (!Physics.SphereCast(origin, sensorRadius, transform.forward, out lastHit,
            sensorDistance, obstacleMask, QueryTriggerInteraction.Ignore))
    {
        IsAvoiding = false;
        avoidanceDirection = Vector3.zero;
        return false;
    }
    Vector3 normal = Vector3.ProjectOnPlane(lastHit.normal, Vector3.up).normalized;
    Vector3 lateral = Vector3.Cross(Vector3.up, transform.forward).normalized;
    if (Vector3.Dot(normal, lateral) < 0f) lateral = -lateral;
    avoidanceDirection = (normal + lateral).normalized;
    IsAvoiding = true;
    return true;
}
```

Add serialized `sensorDistance`, `sensorRadius`, `obstacleMask`, and `sensorHeight`.
Draw yellow clear casts, red blocked casts, end wire sphere, and hit point in Gizmos.

- [ ] **Step 4: Add and run the outside-range test.**

```csharp
[UnityTest]
public IEnumerator TryGetAvoidance_ObstacleOutsideSensorDistance_ReturnsFalse()
{
    // Arrange as above, but set obstacle.transform.position = new Vector3(0f, 0f, 8f).
    yield return null;
    Assert.That(sensor.TryGetAvoidance(out Vector3 direction), Is.False);
    Assert.That(direction, Is.EqualTo(Vector3.zero));
}
```

Expected: both sensor tests pass.

- [ ] **Step 5: Blend avoidance into desired velocity.**

```csharp
bool avoiding = sensor != null && sensor.TryGetAvoidance(out Vector3 avoidance);
if (avoiding) desiredVelocity += avoidance * avoidanceWeight;
desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
```

- [ ] **Step 6: Show the diff to the user; do not commit.**

### Task 3: Add Wander, Rotation, Colours, and Agent Gizmos

**Files:**
- Modify: `Assets/Scripts/SteeringAgent.cs`

**Interfaces:**
- Produces `public Vector3 Velocity { get; }`, `public bool IsUsingTarget { get; }`, and `public bool IsAvoiding { get; }`.

- [ ] **Step 1: Add a zero-distance Arrive test and run all Edit Mode tests.**

```csharp
[Test]
public void CalculateArriveVelocity_AtTarget_ReturnsZero()
{
    Assert.That(SteeringAgent.CalculateArriveVelocity(new Vector3(2f, 0f, 3f),
        new Vector3(2f, 0f, 3f), 8f, 10f, 0.5f), Is.EqualTo(Vector3.zero));
}
```

- [ ] **Step 2: Implement Wander and rotation.**

```csharp
private Vector3 CalculateWanderVelocity()
{
    if (Time.time >= nextWanderChangeTime)
    {
        Vector3 baseDirection = velocity.sqrMagnitude > 0.01f ? velocity.normalized : transform.forward;
        wanderHeading = Quaternion.Euler(0f, Random.Range(-wanderAngleChange, wanderAngleChange), 0f) * baseDirection;
        nextWanderChangeTime = Time.time + Mathf.Max(0.05f, wanderChangeInterval);
    }
    return wanderHeading.normalized * Mathf.Min(wanderSpeed, maxSpeed);
}
```

Integrate with `velocity += acceleration * Time.deltaTime`, move with
`transform.position += velocity * Time.deltaTime`, and use `Quaternion.RotateTowards`
at `turnSpeed * Time.deltaTime` toward non-zero velocity.

- [ ] **Step 3: Implement visual states and Gizmos.**

Set renderers orange/red when avoiding, green when arriving, blue when wandering.
Draw green Stop Radius, yellow Slow Radius, cyan velocity, white desired velocity,
and magenta target direction.

- [ ] **Step 4: Verify in Play Mode.**

Toggle `Use Target`; verify heading changes during Wander, rotation follows velocity,
colours switch by behavior, and Gizmos remain readable when DeliveryBot is selected.

- [ ] **Step 5: Show the diff to the user; do not commit.**

### Task 4: Create Training Yard and Run Experiments

**Files:**
- Create: `Assets/Editor/AutonomousSteeringSceneBuilder.cs`
- Create: `Assets/Scripts/SimplePlayerController.cs`
- Create: `Assets/Scenes/Praktikum03_AutonomousSteering.unity`

**Interfaces:**
- Produces editor menu `Game Cerdas/Build Praktikum 03 Autonomous Steering Scene`.
- Produces objects `Player`, `DeliveryBot`, `ObstacleCourse`, and `UShapeLimitation`.

- [ ] **Step 1: Implement `SimplePlayerController`.**

Use the existing Input System keyboard pattern. In `Awake`, fetch or add a
`CharacterController`; in `Update`, map WASD/arrow keys to an XZ vector, call
`controller.Move(direction.normalized * moveSpeed * Time.deltaTime)`, and rotate
Player toward non-zero input.

- [ ] **Step 2: Build the arena without NavMesh.**

Create a plane at origin, Player at `(-8, 1, -6)`, DeliveryBot at `(0, 1, 0)`,
boundary walls, and standalone obstacles at `(0, 1, 5)`, `(4, 1, -1)`, and
`(-4, 1, 2)`. Put walls and obstacles on `Obstacle`. Create three cube walls in a
U-shape centered near `(7, 1, 6)` under `UShapeLimitation`.

- [ ] **Step 3: Wire NPC defaults.**

Reuse the existing yellow `CharacterVisual`, remove visual colliders, disable the
root capsule renderer, then configure:

```csharp
sensor.Configure(4.5f, 0.45f, LayerMask.GetMask("Obstacle"));
agent.Configure(player.transform, sensor, visual.GetComponentsInChildren<Renderer>());
```

Use defaults: Max Speed `4`, Max Acceleration `8`, Turn Speed `360`, Slow Radius `5`,
Stop Radius `1`, Wander Speed `2.5`, Wander Change Interval `1.2`, Wander Angle Change
`65`, Avoidance Weight `2`.

- [ ] **Step 4: Configure camera, save, and inspect hierarchy.**

Set camera near `(0, 18, -20)` looking downward. Save the named scene, select
DeliveryBot, and confirm its Inspector exposes every required parameter. Confirm no
`NavMeshAgent` or `Navigation` object exists.

- [ ] **Step 5: Run the required experiments in Play Mode.**

| Parameter | Values | Evidence |
| --- | --- | --- |
| Max Speed | 2, 4, 8 | Three screenshots/clips of travel responsiveness. |
| Slow Radius | 2, 5, 10 | Three screenshots with radius Gizmos while approaching Player. |
| Avoidance Weight | 0.5, 2, 5 | Three clips/screenshots at the same obstacle. |

- [ ] **Step 6: Run final acceptance test.**

Verify Arrive stops, Wander works with target disabled, avoidance works in both modes,
NPC faces velocity, Inspector values take effect, colour/Gizmos are visible, and the
U-shape demonstrates why local avoidance differs from pathfinding.

- [ ] **Step 7: Show complete diff and results to user; do not commit or push.**

## Plan Self-Review

**Spec coverage:** Tasks 1-3 implement Arrive, Wander, acceleration, velocity,
movement, rotation, SphereCast avoidance, colors, and Gizmos. Task 4 implements Player,
the open arena, U-shape limitation, Inspector wiring, three required experiments, and
Play Mode validation.

**Placeholder scan:** No TODO/TBD markers or undefined interfaces remain.

**Type consistency:** `SteeringAgent.Configure(Transform, SteeringSensor, Renderer[])`
and `SteeringSensor.TryGetAvoidance(out Vector3)` are defined before use.

**Review focus coverage:** Stop/invalid radius cases appear in Task 1 tests; acceleration
is integrated with `Time.deltaTime`; direct/out-of-range obstacle behavior appears in
Task 2 tests; Wander mode is checked in Task 3 Play Mode verification.
