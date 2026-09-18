using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneControllerTests
    {
        [Test]
        public void Constructor_WithNullRuntime_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SceneController(null));
        }

        [Test]
        public void InitialState_HasNoCurrentOperation()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime();
            SceneController controller = new SceneController(runtime);

            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
            Assert.That(controller.TargetScene, Is.EqualTo(default(SceneReference)));
        }

        [Test]
        public void ActiveScene_ReturnsCurrentRuntimeActiveScene()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                ActiveScene = new SceneReference("Assets/Scenes/MainMenu.unity")
            };

            SceneController controller = new SceneController(runtime);

            Assert.That(controller.ActiveScene, Is.EqualTo(new SceneReference("Assets/Scenes/MainMenu.unity")));

            runtime.ActiveScene = new SceneReference("Assets/Scenes/Gameplay.unity");

            Assert.That(controller.ActiveScene, Is.EqualTo(new SceneReference("Assets/Scenes/Gameplay.unity")));
        }

        [Test]
        public void TrackOperation_WithNullOperation_ThrowsArgumentNullException()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime();
            SceneController controller = new SceneController(runtime);

            Assert.Throws<ArgumentNullException>(() => controller.TrackOperation(null));
        }

        [Test]
        public void TrackOperation_RegistersOperationAndRaisesStartedEvent()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime();
            SceneController controller = new SceneController(runtime);
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");
            SceneOperation operation = new SceneOperation(scene, SceneOperationKind.SingleLoad, asyncOperation);

            SceneOperation startedOperation = null;
            controller.OperationStarted += value => startedOperation = value;

            controller.TrackOperation(operation);

            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.SameAs(operation));
            Assert.That(controller.TargetScene, Is.EqualTo(scene));
            Assert.That(startedOperation, Is.SameAs(operation));
        }

        [Test]
        public void OperationCompletion_ClearsStateBeforeCompletedEvent()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime();
            SceneController controller = new SceneController(runtime);
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();
            SceneOperation operation = new SceneOperation(default, SceneOperationKind.Unload, asyncOperation);

            bool? isBusyDuringCompletedEvent = null;
            SceneOperation currentOperationDuringCompletedEvent = operation;
            SceneOperation completedOperation = null;

            controller.OperationCompleted += value =>
            {
                isBusyDuringCompletedEvent = controller.IsBusy;
                currentOperationDuringCompletedEvent = controller.CurrentOperation;
                completedOperation = value;
            };

            controller.TrackOperation(operation);
            asyncOperation.Complete();

            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
            Assert.That(controller.TargetScene, Is.EqualTo(default(SceneReference)));
            Assert.That(isBusyDuringCompletedEvent, Is.False);
            Assert.That(currentOperationDuringCompletedEvent, Is.Null);
            Assert.That(completedOperation, Is.SameAs(operation));
        }

        [Test]
        public void TrackOperation_WithAlreadyCompletedOperation_RaisesStartedThenCompleted()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime();
            SceneController controller = new SceneController(runtime);
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation
            {
                IsDone = true
            };

            SceneOperation operation = new SceneOperation(default, SceneOperationKind.SingleLoad, asyncOperation);

            int sequence = 0;
            int startedSequence = 0;
            int completedSequence = 0;

            controller.OperationStarted += value => startedSequence = ++sequence;
            controller.OperationCompleted += value => completedSequence = ++sequence;

            controller.TrackOperation(operation);

            Assert.That(startedSequence, Is.EqualTo(1));
            Assert.That(completedSequence, Is.EqualTo(2));
            Assert.That(controller.IsBusy, Is.False);
            Assert.That(controller.CurrentOperation, Is.Null);
        }

        [Test]
        public void LoadAsync_WithEmptyScene_ReturnsInvalidReference()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true
            };

            SceneController controller = new SceneController(runtime);

            var result = controller.LoadAsync(default);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.InvalidReference));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(0));
        }

        [Test]
        public void LoadAsync_WithAdditiveMode_ReturnsInvalidLoadMode()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            var result = controller.LoadAsync(scene, SceneLoadMode.Additive);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.InvalidLoadMode));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(0));
        }

        [Test]
        public void LoadAsync_WithSceneNotInBuild_ReturnsNotInBuild()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = false
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            var result = controller.LoadAsync(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.NotInBuild));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(0));
        }

        [Test]
        public void LoadAsync_WithAlreadyLoadedScene_ReturnsAlreadyLoaded()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = true
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            var result = controller.LoadAsync(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.AlreadyLoaded));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(0));
        }

        [Test]
        public void LoadAsync_WhenRuntimeFailsToStart_ReturnsLoadStartFailed()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = false,
                LoadSingleOperation = null
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            var result = controller.LoadAsync(scene);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(SceneErrorCodes.LoadStartFailed));
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(1));
            Assert.That(controller.IsBusy, Is.False);
        }

        [Test]
        public void LoadAsync_WithValidScene_StartsSingleLoadAndReturnsOperation()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                IsLoaded = false,
                LoadSingleOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            var result = controller.LoadAsync(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(runtime.LoadSingleCallCount, Is.EqualTo(1));
            Assert.That(runtime.LastSingleLoadScene, Is.EqualTo(scene));

            SceneOperation operation = result.Value;

            Assert.That(operation.Scene, Is.EqualTo(scene));
            Assert.That(operation.Kind, Is.EqualTo(SceneOperationKind.SingleLoad));
            Assert.That(controller.IsBusy, Is.True);
            Assert.That(controller.CurrentOperation, Is.SameAs(operation));
            Assert.That(controller.TargetScene, Is.EqualTo(scene));
        }

        [Test]
        public void LoadAsync_WithValidScene_RaisesOperationStarted()
        {
            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                LoadSingleOperation = new FakeSceneAsyncOperation()
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            SceneOperation startedOperation = null;
            controller.OperationStarted += operation => startedOperation = operation;

            var result = controller.LoadAsync(scene);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(startedOperation, Is.SameAs(result.Value));
        }

        [Test]
        public void LoadAsync_WhenOperationCompletes_ClearsControllerState()
        {
            FakeSceneAsyncOperation asyncOperation = new FakeSceneAsyncOperation();

            FakeSceneRuntime runtime = new FakeSceneRuntime
            {
                IsInBuild = true,
                LoadSingleOperation = asyncOperation
            };

            SceneController controller = new SceneController(runtime);
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            SceneOperation completedOperation = null;
            controller.OperationCompleted += operation => completedOperation = operation;

            var result = controller.LoadAsync(scene);

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
            public SceneReference ActiveScene { get; set; }

            public bool IsInBuild { get; set; }

            public bool IsLoaded { get; set; }

            public ISceneAsyncOperation LoadSingleOperation { get; set; }

            public SceneReference LastSingleLoadScene { get; private set; }

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
                LastSingleLoadScene = scene;

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
        }
    }
}