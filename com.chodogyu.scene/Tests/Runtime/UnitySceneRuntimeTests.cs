using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class UnitySceneRuntimeTests
    {
        [Test]
        public void IsSceneInBuild_WithEmptyReference_ReturnsFalse()
        {
            UnitySceneRuntime runtime = new UnitySceneRuntime();

            bool result = runtime.IsSceneInBuild(default);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsSceneLoaded_WithEmptyReference_ReturnsFalse()
        {
            UnitySceneRuntime runtime = new UnitySceneRuntime();

            bool result = runtime.IsSceneLoaded(default);

            Assert.That(result, Is.False);
        }

        [Test]
        public void UnloadAsync_WithEmptyReference_ReturnsNull()
        {
            UnitySceneRuntime runtime = new UnitySceneRuntime();

            ISceneAsyncOperation result = runtime.UnloadAsync(default);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void SetActiveScene_WithEmptyReference_ReturnsFalse()
        {
            UnitySceneRuntime runtime = new UnitySceneRuntime();

            bool result = runtime.SetActiveScene(default);

            Assert.That(result, Is.False);
        }
    }
}