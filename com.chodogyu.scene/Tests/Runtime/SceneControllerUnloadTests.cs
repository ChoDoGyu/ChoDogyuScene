using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneControllerUnloadTests
    {
        [Test]
        public void UnloadAsync_WithEmptyScene_ReturnsInvalidReference()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true
            };

            SceneController controller = new SceneController(runtime);

            var result = controller.UnloadAsync(default);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.InvalidReference));
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(0));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(0));
        }

        [Test]
        public void UnloadAsync_WithUnloadedScene_ReturnsNotLoaded()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = false
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            var result = controller.UnloadAsync(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.NotLoaded));
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(1));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(0));
        }

        [Test]
        public void UnloadAsync_WithActiveScene_ReturnsCannotUnloadActive()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                ActiveScene = scene
            };

            SceneController controller = new SceneController(runtime);

            var result = controller.UnloadAsync(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.CannotUnloadActive));
            Assert.That(runtime.IsSceneLoadedCallCount, Is.EqualTo(1));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(0));
        }

        [Test]
        public void UnloadAsync_WithSameRequestInProgress_ReturnsExistingOperationWithoutStartingAnotherUnload()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity"),
                UnloadOperation = new FakeSceneAsyncOperation()
            };

            SceneController controller = new SceneController(runtime);

            int startedCount = 0;
            controller.OperationStarted += operation => startedCount++;

            var firstResult = controller.UnloadAsync(scene);
            var secondResult = controller.UnloadAsync(scene);

            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(secondResult.IsSuccess, Is.True);
            Assert.That(secondResult.Value, Is.SameAs(firstResult.Value));
            Assert.That(controller.CurrentOperation, Is.SameAs(firstResult.Value));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(1));
            Assert.That(startedCount, Is.EqualTo(1));
        }

        [Test]
        public void UnloadAsync_WhileDifferentOperationInProgress_ReturnsOperationInProgress()
        {
            SceneReference loadingScene = new SceneReference("Assets/Scenes/Gameplay.unity");
            SceneReference unloadScene = new SceneReference("Assets/Scenes/Overlay.unity");

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = false,
                ActiveScene = new SceneReference("Assets/Scenes/MainMenu.unity"),
                LoadSingleOperation = new FakeSceneAsyncOperation(),
                UnloadOperation = new FakeSceneAsyncOperation()
            };

            SceneController controller = new SceneController(runtime);

            var loadResult = controller.LoadAsync(loadingScene);
            var unloadResult = controller.UnloadAsync(unloadScene);

            Assert.That(loadResult.IsSuccess, Is.True);
            Assert.That(unloadResult.IsFailure, Is.True);
            Assert.That(unloadResult.Error.Code, Is.EqualTo(SceneErrorCodes.OperationInProgress));
            Assert.That(controller.CurrentOperation, Is.SameAs(loadResult.Value));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(0));
        }

        [Test]
        public void UnloadAsync_WhenRuntimeFailsToStart_ReturnsUnloadStartFailed()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity"),
                UnloadOperation = null
            };

            SceneController controller = new SceneController(runtime);

            var result = controller.UnloadAsync(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.UnloadStartFailed));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(1));
            Assert.That(runtime.LastUnloadScene, Is.EqualTo(scene));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        [Test]
        public void UnloadAsync_WithValidScene_StartsUnloadAndReturnsOperation()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity"),
                UnloadOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);

            var result = controller.UnloadAsync(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(1));
            Assert.That(runtime.LastUnloadScene, Is.EqualTo(scene));

            SceneOperation operation = result.Value;

            Assert.That(operation.Scene, Is.EqualTo(scene));
            Assert.That(operation.Kind, Is.EqualTo(SceneOperationKind.Unload));
            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.SameAs(operation));
            Assert.That(controller.TargetScene, Is.EqualTo(scene));
        }

        [Test]
        public void UnloadAsync_WithValidScene_RaisesOperationStarted()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity"),
                UnloadOperation = new FakeSceneAsyncOperation()
            };

            SceneController controller = new SceneController(runtime);

            SceneOperation startedOperation = null;
            controller.OperationStarted += operation => startedOperation = operation;

            var result = controller.UnloadAsync(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(startedOperation, Is.SameAs(result.Value));
        }

        [Test]
        public void UnloadAsync_WhenOperationCompletes_ClearsStateAndRaisesCompleted()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity"),
                UnloadOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);

            SceneOperation completedOperation = null;
            controller.OperationCompleted += operation => completedOperation = operation;

            var result = controller.UnloadAsync(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(controller.IsBusy, Is.True);

            asyncOperation.Complete();

            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
            Assert.That(controller.TargetScene, Is.EqualTo(default(SceneReference)));
            Assert.That(completedOperation, Is.SameAs(result.Value));
            Assert.That(result.Value.IsDone, Is.True);
            Assert.That(result.Value.Progress, Is.EqualTo(1f));
        }

        private sealed class FakeSceneRuntime : ISceneRuntime
        {
            public bool IsInBuild { get; set; }

            public bool IsLoaded { get; set; }

            public SceneReference ActiveScene { get; set; }

            public ISceneAsyncOperation LoadSingleOperation { get; set; }

            public ISceneAsyncOperation UnloadOperation { get; set; }

            public SceneReference LastUnloadScene { get; private set; }

            public int IsSceneLoadedCallCount { get; private set; }

            public int UnloadCallCount { get; private set; }

            public bool IsSceneInBuild(SceneReference scene)
            {
                return IsInBuild;
            }

            public bool IsSceneLoaded(SceneReference scene)
            {
                IsSceneLoadedCallCount++;
                return IsLoaded;
            }

            public SceneReference GetActiveScene()
            {
                return ActiveScene;
            }

            public ISceneAsyncOperation LoadSingleAsync(SceneReference scene)
            {
                return LoadSingleOperation;
            }

            public ISceneAsyncOperation LoadAdditiveAsync(SceneReference scene)
            {
                return null;
            }

            public ISceneAsyncOperation UnloadAsync(SceneReference scene)
            {
                UnloadCallCount++;
                LastUnloadScene = scene;

                return UnloadOperation;
            }

            public bool SetActiveScene(SceneReference scene)
            {
                return false;
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