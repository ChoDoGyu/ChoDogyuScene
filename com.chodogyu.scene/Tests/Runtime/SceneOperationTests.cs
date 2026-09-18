using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneOperationTests
    {
        [Test]
        public void Constructor_StoresSceneAndKind()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            SceneOperation operation = new SceneOperation(scene, SceneOperationKind.SingleLoad, asyncOperation);

            Assert.That(operation.Scene, Is.EqualTo(scene));
            Assert.That(operation.Kind, Is.EqualTo(SceneOperationKind.SingleLoad));
        }

        [Test]
        public void Constructor_WithNullOperation_ThrowsArgumentNullException()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            Assert.Throws<ArgumentNullException>(() => new SceneOperation(scene, SceneOperationKind.SingleLoad, null));
        }

        [Test]
        public void Progress_WithSingleLoad_NormalizesUnityLoadProgress()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.45f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.SingleLoad, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void Progress_WithAdditiveLoad_NormalizesUnityLoadProgress()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.675f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.AdditiveLoad, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(0.75f).Within(0.0001f));
        }

        [Test]
        public void Progress_WithLoadProgressAtReadyPoint_ReturnsOneWhileNotDone()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.9f,
                IsDone = false
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.SingleLoad, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(1f));
            Assert.That(operation.IsDone, Is.False);
        }

        [Test]
        public void Progress_WithLoadProgressAboveReadyPoint_IsClampedToOne()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.95f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.SingleLoad, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void Progress_WithUnload_ReturnsRawProgress()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.5f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.Unload, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(0.5f));
        }

        [Test]
        public void Progress_WithNegativeValue_IsClampedToZero()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = -0.5f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.Unload, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(0f));
        }

        [Test]
        public void Progress_WithValueAboveOne_IsClampedToOne()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 1.5f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.Unload, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void IsDone_WhenUnderlyingOperationIsAlreadyDone_ReturnsTrue()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                IsDone = true
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.SingleLoad, asyncOperation);

            Assert.That(operation.IsDone, Is.True);
            Assert.That(operation.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void Completed_WhenUnderlyingOperationCompletes_SetsDoneAndRaisesEvent()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.4f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.Unload, asyncOperation);
            int completedCount = 0;

            operation.Completed += () => completedCount++;

            asyncOperation.Complete();

            Assert.That(operation.IsDone, Is.True);
            Assert.That(operation.Progress, Is.EqualTo(1f));
            Assert.That(completedCount, Is.EqualTo(1));
        }

        [Test]
        public void Completed_WhenUnderlyingOperationRaisesCompletionTwice_RaisesEventOnce()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();
            SceneOperation operation = new SceneOperation(default, SceneOperationKind.SingleLoad, asyncOperation);
            int completedCount = 0;

            operation.Completed += () => completedCount++;

            asyncOperation.Complete();
            asyncOperation.RaiseCompletedAgain();

            Assert.That(completedCount, Is.EqualTo(1));
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