using NUnit.Framework;

namespace CDG.Scene.Tests.Runtime
{
    public sealed class SceneReferenceTests
    {
        [Test]
        public void Default_HasEmptyPath()
        {
            SceneReference scene = default;

            Assert.That(scene.Path, Is.EqualTo(string.Empty));
            Assert.That(scene.IsEmpty, Is.True);
        }

        [Test]
        public void Constructor_StoresOriginalPath()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            Assert.That(scene.Path, Is.EqualTo("Assets/Scenes/Gameplay.unity"));
            Assert.That(scene.IsEmpty, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        public void IsEmpty_WithEmptyOrWhitespacePath_ReturnsTrue(string path)
        {
            SceneReference scene = new SceneReference(path);

            Assert.That(scene.IsEmpty, Is.True);
        }

        [Test]
        public void Equals_WithSamePath_ReturnsTrue()
        {
            SceneReference first = new SceneReference("Assets/Scenes/Gameplay.unity");
            SceneReference second = new SceneReference("Assets/Scenes/Gameplay.unity");

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);
        }

        [Test]
        public void Equals_WithDifferentPath_ReturnsFalse()
        {
            SceneReference first = new SceneReference("Assets/Scenes/Gameplay.unity");
            SceneReference second = new SceneReference("Assets/Scenes/MainMenu.unity");

            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first == second, Is.False);
            Assert.That(first != second, Is.True);
        }

        [Test]
        public void Equals_WithDifferentCase_ReturnsFalse()
        {
            SceneReference first = new SceneReference("Assets/Scenes/Gameplay.unity");
            SceneReference second = new SceneReference("Assets/Scenes/gameplay.unity");

            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void SamePath_HasSameHashCode()
        {
            SceneReference first = new SceneReference("Assets/Scenes/Gameplay.unity");
            SceneReference second = new SceneReference("Assets/Scenes/Gameplay.unity");

            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void ToString_ReturnsPath()
        {
            SceneReference scene = new SceneReference("Assets/Scenes/Gameplay.unity");

            Assert.That(scene.ToString(), Is.EqualTo("Assets/Scenes/Gameplay.unity"));
        }
    }
}