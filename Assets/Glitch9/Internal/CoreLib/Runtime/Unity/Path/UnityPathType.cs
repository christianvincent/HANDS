using UnityEngine;

namespace Glitch9
{
    /// <summary>
    /// Specifies different types of paths used in Unity projects.
    /// </summary>
    public enum UnityPathType
    {
        /// <summary>
        /// The path is not set.
        /// Error will be thrown if you try to use this path.
        /// </summary>
        Unknown,

        /// <summary>
        /// The path is an absolute path on the local file system.
        /// This is the only path that <see cref="System.IO.File"/> can use to load files.
        /// </summary>
        Absolute,

        /// <summary>
        /// The path is relative to the project's <see cref="Application.dataPath"/>.
        /// This is the only path that <see cref="UnityEditor.AssetDatabase"/> can use to load assets.
        /// </summary>
        Assets,

        /// <summary>
        /// The path is relative to the project's <see cref="Application.dataPath"/> + <c>"/Resources"</c>.
        /// This is the only path that <see cref="Resources.Load"/> can use to load assets.
        /// </summary>
        Resources,

        /// <summary>
        /// The path is relative to the project's <see cref="Application.streamingAssetsPath"/>.
        /// If a file is in this folder, you will have to use <see cref="UnityEngine.Networking.UnityWebRequest"/> to load it.
        /// </summary>
        StreamingAssets,

        /// <summary>
        /// The path is relative to the project's <see cref="Application.persistentDataPath"/>.
        /// This path is used to store data that should persist between app launches.
        /// If a file is in this folder, you will have to use <see cref="UnityEngine.Networking.UnityWebRequest"/> to load it.
        /// </summary>
        PersistentData,

        /// <summary>
        /// The path is a URL to a resource on the internet.
        /// Use <see cref="UnityEngine.Networking.UnityWebRequest"/> to load the files.
        /// </summary>
        Url,

        /// <summary>
        /// The path is relative to the project's <see cref="Application.temporaryCachePath"/>.
        /// This path is used to store temporary data that does not need to persist between app launches.
        /// If a file is in this folder, you will have to use <see cref="UnityEngine.Networking.UnityWebRequest"/> to load it.
        /// </summary>
        TemporaryCache,

        /// <summary>
        /// The path is relative to the project's <see cref="Application.consoleLogPath"/>.
        /// This path is used to store log files that are generated by Unity.
        /// If a file is in this folder, you will have to use <see cref="UnityEngine.Networking.UnityWebRequest"/> to load it.
        /// </summary>
        ConsoleLog
    }
}