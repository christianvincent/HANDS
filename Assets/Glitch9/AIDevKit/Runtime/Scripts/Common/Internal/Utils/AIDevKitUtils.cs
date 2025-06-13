
namespace Glitch9.AIDevKit
{
    /// <summary>
    /// AIDevKit 내부에서 사용하는 다목적 유틸리티 클래스
    /// </summary>
    internal class AIDevKitUtils
    {
        internal static string ReturnDefaultIfEmpty(string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        internal static bool IsImageSizeSupported(ImageSize size, Model model)
        {
            if (model == null) return true; // If no model is selected, enable all sizes
            AIDevKitConfig.ImageSizeOptions.TryGetValue(model.Id, out ImageSize[] supportedSizes);

            if (supportedSizes == null || supportedSizes.Length == 0)
            {
                return true; // If no specific sizes are defined for the model, enable all sizes
            }

            foreach (var supportedSize in supportedSizes)
            {
                if (supportedSize == size)
                {
                    return true; // The size is supported by the model
                }
            }

            return false; // The size is not supported by the model
        }

        internal static bool IsImageQualitySupported(ImageQuality quality, Model model)
        {
            if (model == null) return true; // If no model is selected, enable all qualities
            AIDevKitConfig.ImageQualityOptions.TryGetValue(model.Id, out ImageQuality[] supportedQualities);

            if (supportedQualities == null || supportedQualities.Length == 0)
            {
                return true; // If no specific qualities are defined for the model, enable all qualities
            }

            foreach (var supportedQuality in supportedQualities)
            {
                if (supportedQuality == quality)
                {
                    return true; // The quality is supported by the model
                }
            }

            return false; // The quality is not supported by the model
        }
    }
}