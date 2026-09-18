using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneOperationCompletionTests
    {
        [TestCase(SceneOperationKind.SingleLoad)]
        [TestCase(SceneOperationKind.AdditiveLoad)]
        public void LoadProgressAtOne_DoesNotCompleteOperation(SceneOperationKind kind)
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.9f,
                IsDone = false
            };

            SceneOperation operation = new SceneOperation(default, kind, asyncOperation);
            int completedCount = 0;

            operation.Completed += () => completedCount++;

            Assert.That(operation.Progress, Is.EqualTo(1f));
            Assert.That(operation.IsDone, Is.False);
            Assert.That(completedCount, Is.EqualTo(0));
        }

        [Test]
        public void CompletedEvent_WhenUnderlyingOperationCompletes_ObservesCompletedState()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.4f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.SingleLoad, asyncOperation);

            bool? isDoneDuringEvent = null;
            float? progressDuringEvent = null;

            operation.Completed += () =>
            {
                isDoneDuringEvent = operation.IsDone;
                progressDuringEvent = operation.Progress;
            };

            asyncOperation.Complete();

            Assert.That(isDoneDuringEvent, Is.True);
            Assert.That(progressDuringEvent, Is.EqualTo(1f));
        }

        [Test]
        public void CompletedEvent_WhenCompletionSignalIsRaisedTwice_FiresOnce()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();
            SceneOperation operation = new SceneOperation(default, SceneOperationKind.Unload, asyncOperation);

            int completedCount = 0;
            operation.Completed += () => completedCount++;

            asyncOperation.Complete();
            asyncOperation.RaiseCompletedAgain();

            Assert.That(completedCount, Is.EqualTo(1));
            Assert.That(operation.IsDone, Is.True);
        }

        [Test]
        public void Controller_WithLoadProgressAtOne_RemainsBusyUntilActualCompletion()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.9f,
                IsDone = false
            };

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                LoadSingleOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);

            var result = controller.LoadAsync(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Progress, Is.EqualTo(1f));
            Assert.That(result.Value.IsDone, Is.False);
            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.SameAs(result.Value));

            asyncOperation.Complete();

            Assert.That(result.Value.IsDone, Is.True);
            Assert.That(result.Value.Progress, Is.EqualTo(1f));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        [Test]
        public void OperationCompletedEvent_ObservesClearedControllerAndCompletedOperation()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                LoadSingleOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);

            bool? isBusyDuringEvent = null;
            bool? isDoneDuringEvent = null;
            float? progressDuringEvent = null;
            SceneOperation completedOperation = null;

            controller.OperationCompleted += operation =>
            {
                isBusyDuringEvent = controller.IsBusy;
                isDoneDuringEvent = operation.IsDone;
                progressDuringEvent = operation.Progress;
                completedOperation = operation;
            };

            var result = controller.LoadAsync(scene);

            asyncOperation.Complete();

            Assert.That(isBusyDuringEvent, Is.False);
            Assert.That(isDoneDuringEvent, Is.True);
            Assert.That(progressDuringEvent, Is.EqualTo(1f));
            Assert.That(completedOperation, Is.SameAs(result.Value));
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        private sealed class FakeSceneRuntime : ISceneRuntime
        {
            public bool IsInBuild { get; set; }

            public ISceneAsyncOperation LoadSingleOperation { get; set; }

            public bool IsSceneInBuild(SceneReference scene)
            {
                return IsInBuild;
            }

            public bool IsSceneLoaded(SceneReference scene)
            {
                return false;
            }

            public SceneReference GetActiveScene()
            {
                return default;
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
                return null;
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

            public void RaiseCompletedAgain()
            {
                Completed?.Invoke();
            }
        }
    }
}