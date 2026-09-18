using System;
using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class UnitySceneAsyncOperationTests
    {
        [Test]
        public void Constructor_WithNullOperation_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new UnitySceneAsyncOperation(null));
        }
    }
}