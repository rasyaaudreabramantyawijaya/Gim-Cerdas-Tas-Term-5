using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class DungeonFoundationTests
{
    [Test]
    public void DungeonHealthTypeExistsForSharedPlayerAndEnemyDamage()
    {
        var healthType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("DungeonHealth"))
            .FirstOrDefault(type => type != null);

        Assert.That(healthType, Is.Not.Null,
            "DungeonHealth should be available to represent player and enemy health.");
    }

    [Test]
    public void HealthIgnoresNegativeDamageAndSignalsDeathOnlyOnce()
    {
        var type = Find("DungeonHealth");
        var gameObject = new GameObject("health-test");
        try
        {
            var health = gameObject.AddComponent(type);
            type.GetMethod("Configure").Invoke(health, new object[] { 20f });
            int deaths = 0;
            type.GetEvent("Died").AddEventHandler(health, new System.Action(() => deaths++));
            type.GetMethod("TakeDamage").Invoke(health, new object[] { -5f });
            Assert.That((float)type.GetProperty("Current").GetValue(health), Is.EqualTo(20f));
            type.GetMethod("TakeDamage").Invoke(health, new object[] { 50f });
            type.GetMethod("TakeDamage").Invoke(health, new object[] { 5f });
            Assert.That((float)type.GetProperty("Current").GetValue(health), Is.Zero);
            Assert.That(deaths, Is.EqualTo(1));
        }
        finally { UnityEngine.Object.DestroyImmediate(gameObject); }
    }

    [Test]
    public void ArtifactCollectionIsIdempotentAndExitNeedsArtifact()
    {
        var type = Find("DungeonSession");
        var gameObject = new GameObject("session-test");
        try
        {
            var session = gameObject.AddComponent(type);
            Assert.That((bool)type.GetMethod("CompleteRun").Invoke(session, null), Is.False);
            Assert.That((bool)type.GetMethod("CollectArtifact").Invoke(session, null), Is.True);
            Assert.That((bool)type.GetMethod("CollectArtifact").Invoke(session, null), Is.False);
            Assert.That((bool)type.GetMethod("CompleteRun").Invoke(session, null), Is.True);
            Assert.That((bool)type.GetMethod("CompleteRun").Invoke(session, null), Is.False);
        }
        finally { UnityEngine.Object.DestroyImmediate(gameObject); }
    }

    [Test]
    public void ClosingDialogueRestoresPriorTimeScale()
    {
        var type = Find("DungeonSession");
        var gameObject = new GameObject("dialogue-test");
        float previous = Time.timeScale;
        try
        {
            Time.timeScale = 0.4f;
            var session = gameObject.AddComponent(type);
            type.GetMethod("OpenDialogue").Invoke(session, new object[] { "Keeper", "Find the key." });
            Assert.That(Time.timeScale, Is.Zero);
            type.GetMethod("SetDialogueOpen").Invoke(session, new object[] { false });
            Assert.That(Time.timeScale, Is.EqualTo(0.4f).Within(0.001f));
        }
        finally
        {
            Time.timeScale = previous;
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static System.Type Find(string fullName) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(assembly => assembly.GetType(fullName))
        .First(type => type != null);
}
