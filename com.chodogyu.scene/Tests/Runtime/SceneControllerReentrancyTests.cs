using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneControllerReentrancyTests
    {
        [Test]
        public void LoadAsync_WithAlreadyCompletedOperation_RaisesStartedThenCompletedAndLeavesControllerIdle()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                IsDone = true,
                Progress = 1f
            };

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                LoadSingleOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            int sequence = 0;
            int startedSequence = 0;
            int completedSequence = 0;

            controller.OperationStarted += operation => startedSequence = ++sequence;
            controller.OperationCompleted += operation => completedSequence = ++sequence;

            var result = controller.LoadAsync(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.IsDone, Is.True);
            Assert.That(result.Value.Progress, Is.EqualTo(1f));
            Assert.That(startedSequence, Is.EqualTo(1));
            Assert.That(completedSequence, Is.EqualTo(2));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        [Test]
        public void UnloadAsync_WithAlreadyCompletedOperation_RaisesStartedThenCompletedAndLeavesControllerIdle()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Overlay.unity");

            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                IsDone = true,
                Progress = 1f
            };

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsLoaded = true,
                ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity"),
                UnloadOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);

            int startedCount = 0;
            int completedCount = 0;

            controller.OperationStarted += operation => startedCount++;
            controller.OperationCompleted += operation => completedCount++;

            var result = controller.UnloadAsync(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Kind, Is.EqualTo(SceneOperationKind.Unload));
            Assert.That(result.Value.IsDone, Is.True);
            Assert.That(startedCount, Is.EqualTo(1));
            Assert.That(completedCount, Is.EqualTo(1));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        [Test]
        public void OperationStarted_WhenOperationCompletesDuringHandler_DoesNotThrowAndLeavesControllerIdle()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                LoadSingleOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            int completedCount = 0;

            controller.OperationStarted += operation => asyncOperation.Complete();
            controller.OperationCompleted += operation => completedCount++;

            Assert.DoesNotThrow(() =>
            {
                var result = controller.LoadAsync(scene);

                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.IsDone, Is.True);
            });

            Assert.That(completedCount, Is.EqualTo(1));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        [Test]
        public void OperationCompleted_HandlerCanStartNextOperation()
        {
            SceneReference firstScene = new SceneReference("Assets/Scenes/First.unity");
            SceneReference secondScene = new SceneReference("Assets/Scenes/Second.unity");

            FakeSceneAsyncOperation firstAsyncOperation = new FakeSceneAsyncOperation();
            FakeSceneAsyncOperation secondAsyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                LoadSingleOperation = firstAsyncOperation
            };

            SceneController controller = new SceneController(runtime);

            SceneOperation secondOperation = null;

            controller.OperationCompleted += completedOperation =>
            {
                if (completedOperation.Scene != firstScene)
                {
                    return;
                }

                runtime.LoadSingleOperation = secondAsyncOperation;

                var secondResult = controller.LoadAsync(secondScene);

                Assert.That(secondResult.IsSuccess, Is.True);
                secondOperation = secondResult.Value;
            };

            var firstResult = controller.LoadAsync(firstScene);

            Assert.That(firstResult.IsSuccess, Is.True);

            firstAsyncOperation.Complete();

            Assert.That(secondOperation, Is.Not.Null);
            Assert.That(secondOperation.Scene, Is.EqualTo(secondScene));
            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.SameAs(secondOperation));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(2));
        }

        [Test]
        public void OperationCompleted_WhenNextOperationStarts_DoesNotClearNextOperation()
        {
            SceneReference firstScene = new SceneReference("Assets/Scenes/First.unity");
            SceneReference secondScene = new SceneReference("Assets/Scenes/Second.unity");

            FakeSceneAsyncOperation firstAsyncOperation = new FakeSceneAsyncOperation();
            FakeSceneAsyncOperation secondAsyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                LoadSingleOperation = firstAsyncOperation
            };

            SceneController controller = new SceneController(runtime);

            controller.OperationCompleted += completedOperation =>
            {
                if (completedOperation.Scene != firstScene)
                {
                    return;
                }

                runtime.LoadSingleOperation = secondAsyncOperation;
                controller.LoadAsync(secondScene);
            };

            var firstResult = controller.LoadAsync(firstScene);

            firstAsyncOperation.Complete();

            Assert.That(firstResult.Value.IsDone, Is.True);
            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.Not.Null);
            Assert.That(controller.CurrentOperation.Scene, Is.EqualTo(secondScene));

            secondAsyncOperation.Complete();

            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        private sealed class FakeSceneRuntime : ISceneRuntime
        {
            public bool IsInBuild { get; set; }

            public bool IsLoaded { get; set; }

            public SceneReference ActiveScene { get; set; }

            public ISceneAsyncOperation LoadSingleOperation { get; set; }

            public ISceneAsyncOperation UnloadOperation { get; set; }

            public int LoadSingleCallCount { get; private set; }

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