using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneControllerIntegrationTests
    {
        [Test]
        public void LoadAsync_AfterPreviousOperationCompletes_StartsNextOperation()
        {
            FakeSceneAsyncOperation firstAsyncOperation = new FakeSceneAsyncOperation();
            FakeSceneAsyncOperation secondAsyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = false,
                LoadSingleOperation = firstAsyncOperation
            };

            SceneController controller = new SceneController(runtime);
            SceneReference firstScene = new SceneReference("Assets/Scenes/First.unity");
            SceneReference secondScene = new SceneReference("Assets/Scenes/Second.unity");

            var firstResult = controller.LoadAsync(firstScene);

            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(controller.IsBusy, Is.True);

            firstAsyncOperation.Complete();

            Assert.That(controller.IsBusy, Is.False);

            runtime.LoadSingleOperation = secondAsyncOperation;

            var secondResult = controller.LoadAsync(secondScene);

            Assert.That(secondResult.IsSuccess, Is.True);
            Assert.That(secondResult.Value, Is.Not.SameAs(firstResult.Value));
            Assert.That(secondResult.Value.Scene, Is.EqualTo(secondScene));
            Assert.That(controller.CurrentOperation, Is.SameAs(secondResult.Value));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(2));
        }

        [Test]
        public void LoadAsync_WhileUnloadInProgress_ReturnsOperationInProgress()
        {
            SceneReference unloadScene = new SceneReference("Assets/Scenes/Overlay.unity");
            SceneReference loadScene = new SceneReference("Assets/Scenes/Next.unity");

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = true,
                ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity"),
                UnloadOperation = new FakeSceneAsyncOperation(),
                LoadSingleOperation = new FakeSceneAsyncOperation()
            };

            SceneController controller = new SceneController(runtime);

            var unloadResult = controller.UnloadAsync(unloadScene);
            var loadResult = controller.LoadAsync(loadScene);

            Assert.That(unloadResult.IsSuccess, Is.True);
            Assert.That(loadResult.IsFailure, Is.True);
            Assert.That(loadResult.Error.Code, Is.EqualTo(SceneErrorCodes.OperationInProgress));
            Assert.That(controller.CurrentOperation, Is.SameAs(unloadResult.Value));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(1));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(0));
        }

        [Test]
        public void UnloadAsync_WithEmptySceneWhileOperationInProgress_ReturnsInvalidReference()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = false,
                LoadSingleOperation = new FakeSceneAsyncOperation()
            };

            SceneController controller = new SceneController(runtime);
            SceneReference loadingScene = new SceneReference("Assets/Scenes/Gameplay.unity");

            var loadResult = controller.LoadAsync(loadingScene);
            var unloadResult = controller.UnloadAsync(default);

            Assert.That(loadResult.IsSuccess, Is.True);
            Assert.That(unloadResult.IsFailure, Is.True);
            Assert.That(unloadResult.Error.Code, Is.EqualTo(SceneErrorCodes.InvalidReference));
            Assert.That(controller.CurrentOperation, Is.SameAs(loadResult.Value));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(0));
        }

        [Test]
        public void LoadAsync_AfterFailedRequest_CanStartValidOperation()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = false,
                IsLoaded = false
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            int startedCount = 0;
            controller.OperationStarted += operation => startedCount++;

            var failedResult = controller.LoadAsync(scene);

            Assert.That(failedResult.IsFailure, Is.True);
            Assert.That(failedResult.Error.Code, Is.EqualTo(SceneErrorCodes.NotInBuild));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
            Assert.That(startedCount, Is.EqualTo(0));

            runtime.IsInBuild = true;
            runtime.LoadSingleOperation = new FakeSceneAsyncOperation();

            var successResult = controller.LoadAsync(scene);

            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.SameAs(successResult.Value));
            Assert.That(startedCount, Is.EqualTo(1));
        }

        [Test]
        public void UnloadAsync_AfterActiveSceneFailure_CanRetryAfterChangingActiveScene()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                ActiveScene = scene,
                UnloadOperation = new FakeSceneAsyncOperation()
            };

            SceneController controller = new SceneController(runtime);

            var failedResult = controller.UnloadAsync(scene);

            Assert.That(failedResult.IsFailure, Is.True);
            Assert.That(failedResult.Error.Code, Is.EqualTo(SceneErrorCodes.CannotUnloadActive));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(0));

            runtime.ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity");

            var successResult = controller.UnloadAsync(scene);

            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(successResult.Value.Kind, Is.EqualTo(SceneOperationKind.Unload));
            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.SameAs(successResult.Value));
            Assert.That(runtime.UnloadCallCount, Is.EqualTo(1));
        }

        private sealed class FakeSceneRuntime : ISceneRuntime
        {
            public bool IsInBuild { get; set; }

            public bool IsLoaded { get; set; }

            public SceneReference ActiveScene { get; set; }

            public ISceneAsyncOperation LoadSingleOperation { get; set; }

            public ISceneAsyncOperation UnloadOperation { get; set; }

            public int LoadSingleCallCount { get; private set; }

            public int UnloadCallCount { get; private set; }

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
                return ActiveScene;
            }

            public ISceneAsyncOperation LoadSingleAsync(SceneReference scene)
            {
                LoadSingleCallCount++;
                return LoadSingleOperation;
            }

            public ISceneAsyncOperation LoadAdditiveAsync(SceneReference scene)
            {
                return null;
            }

            public ISceneAsyncOperation UnloadAsync(SceneReference scene)
            {
                UnloadCallCount++;
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