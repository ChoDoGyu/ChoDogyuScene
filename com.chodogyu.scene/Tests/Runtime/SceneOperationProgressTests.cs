using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneOperationProgressTests
    {
        [TestCase(SceneOperationKind.SingleLoad)]
        [TestCase(SceneOperationKind.AdditiveLoad)]
        public void Progress_WithLoadOperation_NormalizesZeroToPointNineIntoZeroToOne(SceneOperationKind kind)
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.45f
            };

            SceneOperation operation = new SceneOperation(default, kind, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(operation.IsDone, Is.False);
        }

        [TestCase(SceneOperationKind.SingleLoad)]
        [TestCase(SceneOperationKind.AdditiveLoad)]
        public void Progress_WithLoadAtReadyPoint_ReturnsOneWithoutMarkingOperationDone(SceneOperationKind kind)
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.9f,
                IsDone = false
            };

            SceneOperation operation = new SceneOperation(default, kind, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(1f));
            Assert.That(operation.IsDone, Is.False);
        }

        [Test]
        public void Progress_WithUnloadOperation_UsesRawProgress()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.65f
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.Unload, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(0.65f).Within(0.0001f));
            Assert.That(operation.IsDone, Is.False);
        }

        [TestCase(SceneOperationKind.SingleLoad)]
        [TestCase(SceneOperationKind.AdditiveLoad)]
        [TestCase(SceneOperationKind.Unload)]
        public void Progress_WithNegativeRawProgress_ReturnsZero(SceneOperationKind kind)
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = -0.5f
            };

            SceneOperation operation = new SceneOperation(default, kind, asyncOperation);

            Assert.That(operation.Progress, Is.EqualTo(0f));
        }

        [TestCase(SceneOperationKind.SingleLoad)]
        [TestCase(SceneOperationKind.AdditiveLoad)]
        [TestCase(SceneOperationKind.Unload)]
        public void Progress_AfterCompletion_ReturnsOne(SceneOperationKind kind)
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                Progress = 0.25f
            };

            SceneOperation operation = new SceneOperation(default, kind, asyncOperation);

            asyncOperation.Complete();

            Assert.That(operation.IsDone, Is.True);
            Assert.That(operation.Progress, Is.EqualTo(1f));
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