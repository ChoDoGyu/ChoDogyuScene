using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneControllerAdditiveTests
    {
        [Test]
        public void LoadAsync_WithValidAdditiveScene_StartsAdditiveLoadAndReturnsOperation()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = false,
                LoadAdditiveOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            var result = controller.LoadAsync(scene, SceneLoadMode.Additive);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(runtime.LoadAdditiveCallCount, Is.EqualTo(1));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(0));
            Assert.That(runtime.LastAdditiveLoadScene, Is.EqualTo(scene));

            SceneOperation operation = result.Value;

            Assert.That(operation.Scene, Is.EqualTo(scene));
            Assert.That(operation.Kind, Is.EqualTo(SceneOperationKind.AdditiveLoad));
            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.SameAs(operation));
            Assert.That(controller.TargetScene, Is.EqualTo(scene));
            Assert.That(runtime.SetActiveSceneCallCount, Is.EqualTo(0));
        }

        [Test]
        public void LoadAsync_WithAlreadyLoadedAdditiveScene_ReturnsAlreadyLoadedWithoutStartingLoad()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = true,
                LoadAdditiveOperation = new FakeSceneAsyncOperation()
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            var result = controller.LoadAsync(scene, SceneLoadMode.Additive);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.AlreadyLoaded));
            Assert.That(runtime.LoadAdditiveCallCount, Is.EqualTo(0));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(0));
            Assert.That(controller.IsBusy, Is.False);
        }

        [Test]
        public void LoadAsync_WhenAdditiveRuntimeFailsToStart_ReturnsLoadStartFailed()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = false,
                LoadAdditiveOperation = null
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            var result = controller.LoadAsync(scene, SceneLoadMode.Additive);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.LoadStartFailed));
            Assert.That(runtime.LoadAdditiveCallCount, Is.EqualTo(1));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(0));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        [Test]
        public void LoadAsync_WhenAdditiveOperationCompletes_ClearsStateAndRaisesCompleted()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = false,
                LoadAdditiveOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            SceneOperation completedOperation = null;
            controller.OperationCompleted += operation => completedOperation = operation;

            var result = controller.LoadAsync(scene, SceneLoadMode.Additive);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(controller.IsBusy, Is.True);

            asyncOperation.Complete();

            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
            Assert.That(controller.TargetScene, Is.EqualTo(default(SceneReference)));
            Assert.That(completedOperation, Is.SameAs(result.Value));
        }

        private sealed class FakeSceneRuntime : ISceneRuntime
        {
            public bool IsInBuild { get; set; }

            public bool IsLoaded { get; set; }

            public ISceneAsyncOperation LoadAdditiveOperation { get; set; }

            public SceneReference LastAdditiveLoadScene { get; private set; }

            public int LoadSingleCallCount { get; private set; }

            public int LoadAdditiveCallCount { get; private set; }

            public int SetActiveSceneCallCount { get; private set; }

            public bool IsSceneInBuild(SceneReference scene)
            {
                return IsInBuild;
            }

            public bool IsSceneLoaded(SceneReference scene)
            {
                return IsLoaded;
            }

            public SceneReference GetActiveScene()
            {
                return default;
            }

            public ISceneAsyncOperation LoadSingleAsync(SceneReference scene)
            {
                LoadSingleCallCount++;
                return null;
            }

            public ISceneAsyncOperation LoadAdditiveAsync(SceneReference scene)
            {
                LoadAdditiveCallCount++;
                LastAdditiveLoadScene = scene;

                return LoadAdditiveOperation;
            }

            public ISceneAsyncOperation UnloadAsync(SceneReference scene)
            {
                return null;
            }

            public bool SetActiveScene(SceneReference scene)
            {
                SetActiveSceneCallCount++;
                return true;
            }
        }

        private sealed class FakeSceneAsyncOperation : ISceneAsyncOperation
        {
            public float Progress { get; set; }

            public bool IsDone { get; set; }

            public event Action Completed;

            public void Complete()
            {
                IsDone = true;
                Completed?.Invoke();
            }
        }
    }
}