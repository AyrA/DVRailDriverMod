using System.IO;

namespace DVRailDriverMod.Calibration
{
    /// <summary>
    /// Interface for classes that implement generic serialization
    /// </summary>
    /// <remarks>
    /// Serialization should only write as much data as needed.
    /// Deserialization must not read more data than originally serialized
    /// </remarks>
    internal interface IStreamSerializable
    {
        /// <summary>
        /// Deserializes calibration data
        /// </summary>
        /// <param name="source">Data source previously saved using <see cref="Serialize(Stream)"/></param>
        void Deserialize(Stream source);

        /// <summary>
        /// Serializes calibration data
        /// </summary>
        /// <param name="destination">Data destination that can later be used in <see cref="Deserialize(Stream)"/></param>
        void Serialize(Stream destination);
    }
}
