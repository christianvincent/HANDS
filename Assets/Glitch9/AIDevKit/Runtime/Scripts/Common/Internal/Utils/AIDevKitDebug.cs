using UnityEngine;

namespace Glitch9.AIDevKit
{
    internal static class AIDevKitDebug
    {
        internal readonly static Prefs<bool> kDebugMode = new("AIDevKit.DebugMode", false);

        internal static void Mark(params int[] num)
        {
            if (!kDebugMode.Value) return;
            Debug.Log($"<color=magenta>-----------------{string.Join("-", num)}-----------------</color>");
        }

        internal static void Mark(string message)
        {
            if (!kDebugMode.Value) return;
            Debug.Log($"<color=magenta>-----------------{message}-----------------</color>");
        }

        internal static void Log(string message)
        {
            if (!kDebugMode.Value) return;
            Debug.Log($"<color=magenta>[Debug] {message}</color>");
        }

        internal static void Green(string message)
        {
            if (!kDebugMode.Value) return;
            Debug.Log($"<color=#3cff4b>[Debug] {message}</color>");
        }

        internal static void Blue(string message)
        {
            if (!kDebugMode.Value) return;
            Debug.Log($"<color=cyan>[Debug] {message}</color>");
        }

        internal static void LogWarning(string message)
        {
            if (!kDebugMode.Value) return;
            Debug.LogWarning(message);
        }

        internal static void LogError(string message)
        {
            if (!kDebugMode.Value) return;
            Debug.LogError(message);
        }
    }
}