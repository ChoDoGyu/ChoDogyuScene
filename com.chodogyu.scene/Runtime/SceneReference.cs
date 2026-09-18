using System;
using UnityEngine;

namespace CDG.Scene
{
    /// <summary>
    /// Runtime과 Editor에서 Scene을 식별하기 위한 직렬화 가능한 Scene 참조 값입니다.
    /// Scene 이름이 아닌 전체 Asset Path를 저장하며 경로의 유효성 검사는 별도의 검증 계층에서 수행합니다.
    /// </summary>
    [Serializable]
    public struct SceneReference : IEquatable<SceneReference>
    {
        [SerializeField]
        private string path;

        /// <summary>
        /// Scene의 전체 Asset Path를 반환합니다.
        /// 값이 설정되지 않은 경우 빈 문자열을 반환합니다.
        /// </summary>
        public string Path => path ?? string.Empty;

        /// <summary>
        /// Scene 경로가 비어 있거나 공백만 포함하고 있는지 여부를 반환합니다.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(path);

        /// <summary>
        /// 지정된 전체 Asset Path로 Scene 참조를 생성합니다.
        /// 경로 정규화, 공백 제거 또는 유효성 검사는 수행하지 않습니다.
        /// </summary>
        public SceneReference(string path)
        {
            this.path = path;
        }

        /// <inheritdoc/>
        public bool Equals(SceneReference other)
        {
            return string.Equals(Path, other.Path, StringComparison.Ordinal);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is SceneReference other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Path);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Path;
        }

        public static bool operator ==(SceneReference left, SceneReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SceneReference left, SceneReference right)
        {
            return !left.Equals(right);
        }
    }
}