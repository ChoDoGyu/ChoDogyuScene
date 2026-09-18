using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneControllerActiveSceneTests
    {
        [Test]
        public void IsSceneLoaded_WithEmptyScene_ReturnsFalseWithoutRuntimeLookup()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true
            };

            SceneController controller = new SceneController(runtime);

            bool result = controller.IsSceneLoaded(default);

            Assert.That(result, Is.False);
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(0));
        }

        [Test]
        public void IsSceneLoaded_WithValidScene_ReturnsRuntimeResult()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            bool result = controller.IsSceneLoaded(scene);

            Assert.That(result, Is.True);
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(1));
            Assert.That(runtime.LastLoadedCheckScene, Is.EqualTo(scene));
        }

        [Test]
        public void SetActiveScene_WithEmptyScene_ReturnsInvalidReference()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                SetActiveResult = true
            };

            SceneController controller = new SceneController(runtime);

            var result = controller.SetActiveScene(default);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.InvalidReference));
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(0));
            Assert.That(runtime.SetActiveSceneCallCount, Is.EqualTo(0));
        }

        [Test]
        public void SetActiveScene_WithUnloadedScene_ReturnsNotLoaded()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = false,
                SetActiveResult = true
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            var result = controller.SetActiveScene(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.NotLoaded));
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(1));
            Assert.That(runtime.SetActiveSceneCallCount, Is.EqualTo(0));
        }

        [Test]
        public void SetActiveScene_WhenRuntimeFails_ReturnsSetActiveFailed()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                SetActiveResult = false
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            var result = controller.SetActiveScene(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.SetActiveFailed));
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(1));
            Assert.That(runtime.SetActiveSceneCallCount, Is.EqualTo(1));
            Assert.That(runtime.LastSetActiveScene, Is.EqualTo(scene));
        }

        [Test]
        public void SetActiveScene_WithLoadedScene_SucceedsAndUpdatesActiveScene()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                SetActiveResult = true,
                ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity")
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            var result = controller.SetActiveScene(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(1));
            Assert.That(runtime.SetActiveSceneCallCount, Is.EqualTo(1));
            Assert.That(runtime.LastSetActiveScene, Is.EqualTo(scene));
            Assert.That(controller.ActiveScene, Is.EqualTo(scene));
        }

        private sealed class FakeSceneRuntime : ISceneRuntime
        {
            public bool IsLoaded { get; set; }

            public bool SetActiveResult { get; set; }

            public SceneReference ActiveScene { get; set; }

            public SceneReference LastLoadedCheckScene { get; private set; }

            public SceneReference LastSetActiveScene { get; private set; }

            public int IsSceneLoadedCallCount { get; private set; }

            public int SetActiveSceneCallCount { get; private set; }

            public bool IsSceneInBuild(SceneReference scene)
            {
                return false;
            }

            public bool IsSceneLoaded(SceneReference scene)
            {
                IsSceneLoadedCallCount++;
                LastLoadedCheckScene = scene;

                return IsLoaded;
            }

            public SceneReference GetActiveScene()
            {
                return ActiveScene;
            }

            public ISceneAsyncOperation LoadSingleAsync(SceneReference scene)
            {
                return null;
            }

            public ISceneAsyncOperation LoadAdditiveAsync(SceneReference scene)
            {
                return null;
            }

            public ISceneAsyncOperation UnloadAsync(SceneReference scene)
            {
                return null;
            }

            public bool SetActiveScene(SceneReference scene)
            {
                SetActiveSceneCallCount++;
                LastSetActiveScene = scene;

                if (!SetActiveResult)
                {
                    return false;
                }

                ActiveScene = scene;
                return true;
            }
        }
    }
}