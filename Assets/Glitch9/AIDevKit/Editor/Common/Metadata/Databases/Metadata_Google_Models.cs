using System.Collections.Generic;

namespace Glitch9.AIDevKit.Editor
{
    internal static class Metadata_Google_Models
    {
        internal static Modality ResolveInputModality(ModelFeature cap, string id, string family)
        {
            if (id.Contains("veo-")) return Modality.Text | Modality.Image;

            Modality modal = default;

            if (cap.HasFlag(ModelFeature.SpeechGeneration)) modal |= Modality.Text;
            if (cap.HasFlag(ModelFeature.SpeechRecognition)) modal |= Modality.Audio;
            if (cap.HasFlag(ModelFeature.SoundFXGeneration)) modal |= Modality.Audio;
            if (cap.HasFlag(ModelFeature.TextGeneration)) modal |= ResolveLLMInputModality(family);
            if (cap.HasFlag(ModelFeature.ImageGeneration)) modal |= Modality.Image;
            if (cap.HasFlag(ModelFeature.VideoGeneration)) modal |= Modality.Video;
            if (cap.HasFlag(ModelFeature.TextEmbedding)) modal |= Modality.TextEmbedding;
            if (cap.HasFlag(ModelFeature.Moderation)) modal |= Modality.Text;

            return modal;
        }

        private static Modality ResolveLLMInputModality(string family)
        {
            if (family == ModelFamily.Gemini) return Modality.Text | Modality.Image | Modality.Audio | Modality.Video;
            return Modality.Text;
        }

        internal static Modality ResolveOutputModality(ModelFeature cap, string id)
        {
            Modality modal = default;

            if (id.Contains("veo-")) modal |= Modality.Video;
            if (id.Contains("tts")) modal |= Modality.Audio;

            // Gemini 2.0 Flash: Text, images (experimental), and audio (coming soon) 
            if (id.Contains("gemini-2.0-flash"))
            {
                if (id.Contains("live")) modal |= Modality.Audio;
                if (id.Contains("image")) modal |= Modality.Image;
                if (id.Contains("video")) modal |= Modality.Video;
                if (id.Contains("audio")) modal |= Modality.Audio;
            }

            if (cap.HasFlag(ModelFeature.TextEmbedding)) modal |= Modality.TextEmbedding;
            if (cap.HasFlag(ModelFeature.ImageGeneration)) modal |= Modality.Image;
            if (cap.HasFlag(ModelFeature.VideoGeneration)) modal |= Modality.Video;
            if (cap.HasFlag(ModelFeature.TextGeneration)) modal |= Modality.Text;
            if (cap.HasFlag(ModelFeature.SoundFXGeneration)) modal |= Modality.Audio;
            if (cap.HasFlag(ModelFeature.SpeechGeneration)) modal |= Modality.Audio;
            if (cap.HasFlag(ModelFeature.SpeechRecognition)) modal |= Modality.Text;

            return modal;
        }

        internal static ModelFeature ResolveExtraCapability(Modality outputModal, string id)
        {
            ModelFeature cap = default;

            if (outputModal.HasFlag(Modality.Image)) cap |= ModelFeature.ImageGeneration;
            if (outputModal.HasFlag(Modality.Video)) cap |= ModelFeature.VideoGeneration;
            if (outputModal.HasFlag(Modality.Audio)) cap |= ModelFeature.SpeechGeneration;
            if (id.Contains("tts")) cap |= ModelFeature.SpeechGeneration;
            if (id.Contains("veo")) cap |= ModelFeature.VideoGeneration;

            return cap;
        }

        internal static ModelCatalogueEntry Resolve(ModelCatalogueEntry entry)
        {
            AIDevKitDebug.Mark($"Resolving Google model metadata for '{entry.Id}'...");

            // Missing Properties:
            // ✓ Type (Resolved inside GoogleModelData.cs)
            // ✓ IsTrainable (Resolved inside GoogleModelData.cs)
            // ✓ CreatedAt 
            // ✓ InputModality, OutputModality

            // entry.Name = entry.Id;

            // 출시일이 위의 modelReleaseDates에 포함되어 있는지 확인
            if (ReleaseDates.kReleaseDates.TryGetValue(entry.Id, out UnixTime releaseDate))
            {
                entry.CreatedAt = releaseDate;
            }
            else
            {
                // 출시일이 없으면 ResolveCreatedAt 메서드를 사용하여 날짜를 추출
                entry.CreatedAt ??= NaturalDateParser.ResolveTimeFromDescription(entry.Description);
                entry.CreatedAt ??= UnixTime.Now; // 그래도 출시일이 없으면 기본값으로 현재 날짜를 사용
            }

            entry.SetPrices(Prices.kPrices.GetBestMatch(entry.Id, (notFound)
                => AIDevKitDebug.LogWarning($"Model ID '{notFound}' not found in Google model prices.")));

            entry.InputModality = ResolveInputModality(entry.Capability, entry.Id, entry.Family);
            entry.OutputModality = ResolveOutputModality(entry.Capability, entry.Id);
            entry.Capability |= ResolveExtraCapability(entry.OutputModality, entry.Id);

            return entry;
        }

        #region Prices

        private static class Prices
        {
            internal static readonly Dictionary<string, ModelPrice[]> kPrices = new()
            {
                { "imagen-3", ModelPrice.PerImage(0.03) },
                { "veo-2.0-generate-001", ModelPrice.PerMinute(0.35 * 60) },
                { "gemini-2.5-flash-preview", new ModelPrice[] {
                    new (UsageType.InputToken, 0.00000015),
                    new (UsageType.OutputToken, 0.0000006),
                    new (UsageType.OutputTokenThinking, 0.0000035),
                }},
                { "gemini-2.5-pro-preview", new ModelPrice[] {
                    new (UsageType.InputToken, 0.0000025),
                    new (UsageType.OutputToken, 0.000015),
                    new (UsageType.CachedInputToken, 0.000000625),
                }},
                { "gemini-2.0-flash-lite", ModelPrice.PerInputOutput( 0.000000075, 0.0000003) },
                { "gemini-2.0-flash", new ModelPrice[] {
                    new (UsageType.InputToken, 0.0000001),
                    new (UsageType.OutputToken, 0.0000004),
                    new (UsageType.CachedInputToken, 0.000000025),
                }},
                { "gemini-1.5-flash-8b", new ModelPrice[] {
                    new (UsageType.InputToken, 0.000000075),
                    new (UsageType.OutputToken, 0.0000003),
                    new (UsageType.CachedInputToken, 0.00000002),
                }},
                { "gemini-1.5-flash", new ModelPrice[] {
                    new (UsageType.InputToken, 0.00000015),
                    new (UsageType.OutputToken, 0.0000006),
                    new (UsageType.CachedInputToken, 0.0000000375),
                }},
                { "gemini-1.5-pro", new ModelPrice[] {
                    new (UsageType.InputToken, 0.0000025),
                    new (UsageType.OutputToken, 0.00001),
                    new (UsageType.CachedInputToken, 0.000000625),
                }},
                { "gemma-3", ModelPrice.Free() },
                { "embedding", ModelPrice.Free() },
            };
        }

        #endregion Prices

        #region Release Dates

        private static class ReleaseDates
        {
            internal static readonly Dictionary<string, UnixTime> kReleaseDates = new()
            {
                { "models/embedding-001", new UnixTime(2023, 3, 30) }, // 출시일: 2023년 3월 (추정)
                { "models/text-bison-001", new UnixTime(2023, 5, 1) }, // 출시일: 2023년 5월 (추정)
                { "models/embedding-gecko-001", new UnixTime(2023, 5, 1) }, // 출시일: 2023년 5월 (추정)
                { "models/chat-bison-001", new UnixTime(2023, 7, 10) }, // 출시일: 2023년 7월 10일

                { "models/gemini-1.5-flash-001", new UnixTime(2024, 5, 14) }, // 출시일: 2024년 5월 14일
                { "models/gemini-1.5-flash-001-tuning", new UnixTime(2024, 5, 14) }, // 출시일: 2024년 5월 14일
                { "models/gemini-1.5-pro-001", new UnixTime(2024, 5, 24) }, // 출시일: 2024년 5월 24일

                { "models/gemini-1.0-pro-vision-latest", new UnixTime(2024, 2, 15) }, // 출시일: 2024년 2월 15일
                { "models/gemini-pro-vision", new UnixTime(2024, 2, 15) }, // 출시일: 2024년 2월 15일

                { "models/text-embedding-004", new UnixTime(2024, 1, 10) }, // 출시일: 2024년 1월 10일

                { "models/gemini-exp-1206", new UnixTime(2024, 12, 6) }, // 출시일: 2024년 12월 6일 (추정)
                { "models/gemini-2.0-flash-thinking-exp-1219", new UnixTime(2024, 12, 19) }, // 출시일: 2024년 12월 19일 (추정)
                { "models/gemini-2.0-flash-exp", new UnixTime(2024, 12, 13) }, // 출시일: 2024년 12월 13일

                { "models/aqa", new UnixTime(2024, 11, 15) }, // 출시일: 2024년 11월 15일 (추정, AQA 소개 블로그 기준)
                { "models/learnlm-1.5-pro-experimental", new UnixTime(2024, 11, 21) }, // 출시일: 2024년 11월 21일

                { "models/gemini-1.5-flash-8b-exp-0827", new UnixTime(2024, 8, 27) }, // 출시일: 2024년 8월 27일 (추정)

                { "models/gemini-1.5-pro-002", new UnixTime(2024, 9, 24) }, // 출시일: 2024년 9월 24일
                { "models/gemini-1.5-pro", new UnixTime(2024, 9, 24) }, // 출시일: 2024년 9월 24일
                { "models/gemini-1.5-pro-latest", new UnixTime(2024, 9, 24) }, // 출시일: 2024년 9월 24일
                { "models/gemini-1.5-flash", new UnixTime(2024, 9, 24) }, // 출시일: 2024년 9월 24일
                { "models/gemini-1.5-flash-002", new UnixTime(2024, 9, 24) }, // 출시일: 2024년 9월 24일
                { "models/gemini-1.5-flash-latest", new UnixTime(2024, 9, 24) }, // 출시일: 2024년 9월 24일
                { "models/gemini-1.5-flash-8b-exp-0924", new UnixTime(2024, 9, 24) }, // 출시일: 2024년 9월 24일 (추정)

                { "models/gemini-1.5-flash-8b", new UnixTime(2024, 10, 1) }, // 출시일: 2024년 10월 (추정)
                { "models/gemini-1.5-flash-8b-001", new UnixTime(2024, 10, 1) }, // 출시일: 2024년 10월 (추정)
                { "models/gemini-1.5-flash-8b-latest", new UnixTime(2024, 10, 1) }, // 출시일: 2024년 10월 (추정)

                { "models/imagen-3.0-generate-002", new UnixTime(2024, 12, 1) }, // 출시일: 2024년 12월 (추정)

                { "models/gemini-2.0-flash-thinking-exp-01-21", new UnixTime(2025, 1, 21) }, // 출시일: 2025년 1월 21일
                { "models/gemini-2.0-flash-thinking-exp", new UnixTime(2025, 1, 21) }, // 출시일: 2025년 1월 21일

                { "models/gemini-2.0-flash", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일
                { "models/gemini-2.0-flash-001", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일
                { "models/gemini-2.0-flash-exp-image-generation", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일 (추정)
                { "models/gemini-2.0-flash-lite-001", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일
                { "models/gemini-2.0-flash-lite", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일
                { "models/gemini-2.0-flash-lite-preview-02-05", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일
                { "models/gemini-2.0-flash-lite-preview", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일
                { "models/gemini-2.0-pro-exp", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일 (추정)
                { "models/gemini-2.0-pro-exp-02-05", new UnixTime(2025, 2, 5) }, // 출시일: 2025년 2월 5일 (추정)

                { "models/gemini-embedding-exp", new UnixTime(2025, 3, 7) }, // 출시일: 2025년 3월 7일 (같은 모델로 간주)
                { "models/gemini-embedding-exp-03-07", new UnixTime(2025, 3, 7) }, // 출시일: 2025년 3월 7일

                { "models/learnlm-2.0-flash-experimental", new UnixTime(2025, 3, 1) }, // 출시일: 2025년 3월 (추정)

                { "models/gemma-3-1b-it", new UnixTime(2025, 3, 12) }, // 출시일: 2025년 3월 12일
                { "models/gemma-3-4b-it", new UnixTime(2025, 3, 12) }, // 출시일: 2025년 3월 12일
                { "models/gemma-3-12b-it", new UnixTime(2025, 3, 12) }, // 출시일: 2025년 3월 12일
                { "models/gemma-3-27b-it", new UnixTime(2025, 3, 12) }, // 출시일: 2025년 3월 12일

                { "models/gemini-2.5-pro-exp-03-25", new UnixTime(2025, 3, 25) }, // 출시일: 2025년 3월 25일
                { "models/gemini-2.5-pro-preview-03-25", new UnixTime(2025, 3, 25) }, // 출시일: 2025년 3월 25일

                { "models/gemini-2.5-flash-preview-04-17", new UnixTime(2025, 4, 17) }, // 출시일: 2025년 4월 17일 (추정)
                { "models/gemini-2.0-flash-live-001",  new UnixTime(2025, 4, 9) },
                { "models/veo-2.0-generate-001",  new UnixTime(2025, 4, 9) },
            };
        }

        #endregion Release Dates
    }
}