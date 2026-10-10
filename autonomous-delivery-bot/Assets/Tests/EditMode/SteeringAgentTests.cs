using NUnit.Framework;
using UnityEngine;

public class SteeringAgentTests
{
    [Test]
    public void CalculateArriveVelocity_InsideStopRadius_ReturnsZero()
    {
        Vector3 result = SteeringAgent.CalculateArriveVelocity(
            Vector3.zero, new Vector3(0.5f, 0f, 0f), 4f, 5f, 1f);

        Assert.That(result, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void CalculateArriveVelocity_InSlowRadius_ScalesSpeedByDistance()
    {
        Vector3 result = SteeringAgent.CalculateArriveVelocity(
            Vector3.zero, new Vector3(3f, 0f, 0f), 4f, 6f, 1f);

        Assert.That(result.magnitude, Is.EqualTo(2f).Within(0.001f));
        Assert.That(result.normalized, Is.EqualTo(Vector3.right));
    }

    [Test]
    public void CalculateArriveVelocity_InvalidRadiusOrder_ReturnsFiniteBoundedResult()
    {
        Vector3 result = SteeringAgent.CalculateArriveVelocity(
            Vector3.zero, new Vector3(2f, 0f, 0f), 4f, 1f, 1f);

        Assert.That(float.IsFinite(result.x), Is.True);
        Assert.That(result.magnitude, Is.LessThanOrEqualTo(4f));
    }

    [Test]
    public void CalculateAcceleration_OverLimit_ClampsMagnitude()
    {
        Vector3 result = SteeringAgent.CalculateAcceleration(
            Vector3.zero, Vector3.right * 10f, 3f);

        Assert.That(result.magnitude, Is.EqualTo(3f).Within(0.001f));
    }
}
