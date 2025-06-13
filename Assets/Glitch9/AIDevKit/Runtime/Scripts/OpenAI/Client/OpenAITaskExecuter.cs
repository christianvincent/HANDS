using System;
using Cysharp.Threading.Tasks;
using Glitch9.AIDevKit.GENTasks;
using Glitch9.IO.Networking.RESTApi;
using UnityEngine;

namespace Glitch9.AIDevKit.OpenAI
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    internal class OpenAITaskExecuter : GENTaskExecuter
    {
#if UNITY_EDITOR
        static OpenAITaskExecuter()
        {
            GENTaskManager.RegisterTaskExecuter(Api.OpenAI, new OpenAITaskExecuter());
        }
#else 
        [UnityEngine.RuntimeInitializeOnLoadMethod]
        private static void ResisterTaskExecuter()
        {
            GENTaskManager.RegisterTaskExecuter(Api.OpenAI, new OpenAITaskExecuter());
        }
#endif

        internal override Api Api { get; } = Api.OpenAI;

        internal override async UniTask<ChatCompletion> GenerateResponseAsync(GENResponseTask task, Type jsonSchemaType)
        {
            ChatCompletionRequest req = task.CreateChatCompletionRequest(jsonSchemaType, false);
            return await req.ExecuteAsync();
        }

        internal override async UniTask StreamResponseAsync(GENResponseTask task, Type jsonSchemaType, IChatCompletionStreamHandler streamHandler)
        {
            ChatCompletionRequest req = task.CreateChatCompletionRequest(jsonSchemaType, true);
            await req.StreamAsync(streamHandler);
        }

        internal override async UniTask<GeneratedImage> GenerateImageAsync(GENImageTask task)
        {
            ImageCreationRequest.Builder builder = new ImageCreationRequest.Builder()
                .SetSender(task.sender)
                .SetIgnoreLogs(task._ignoreLogs)
                .SetModel(task.model.Id)
                .SetPrompt(task.prompt)
                .SetN(task.n)
                .SetOutputPath(task._outputPath)
                .SetCancellationToken(task.token);

            ImageSize? size = task.GetSize();
            ImageQuality? quality = task.GetQuality();
            ImageStyle? style = task.GetStyle();

            if (size != null) builder.SetSize(size.Value);
            if (quality != null) builder.SetQuality(quality.Value);
            if (style != null) builder.SetStyle(style.Value);

            GeneratedImage result = await builder.Build().ExecuteAsync();

            if (result != null)
            {
                size ??= OpenAIUtils.GetDefaultImageSize(task.model);
                quality ??= OpenAIUtils.GetDefaultImageQuality(task.model);
                Usage usage = OpenAIUtils.CreateImageUsage(size.Value, quality.Value, task.n);
                result.Usage = usage;
            }

            return result;
        }

        internal override async UniTask<GeneratedImage> GenerateInpaintAsync(GENInpaintTask task)
        {
            ImageEditRequest.Builder builder = new ImageEditRequest.Builder()
                .SetSender(task.sender)
                .SetIgnoreLogs(task._ignoreLogs)
                .SetImage(task.prompt.image)
                .SetModel(task.model.Id)
                .SetPrompt(task.prompt.text)
                .SetN(task.n)
                .SetCancellationToken(task.token);

            Texture2D mask = task.GetMask();
            ImageSize? size = task.GetSize();

            if (size != null) builder.SetSize(size.Value);
            if (mask != null) builder.SetMask(mask);

            builder.SetOutputPath(task._outputPath);

            GeneratedImage result = await builder.Build().ExecuteAsync();

            if (result != null)
            {
                size ??= OpenAIUtils.GetDefaultImageSize(task.model);
                ImageQuality quality = OpenAIUtils.GetDefaultImageQuality(task.model);
                Usage usage = OpenAIUtils.CreateImageUsage(size.Value, quality, task.n);
                result.Usage = usage;
            }

            return result;
        }

        // internal override async UniTask<GeneratedImage> GenerateImageVariationAsync(GENImageVariationTask task)
        // {
        //     ImageVariationRequest.Builder builder = new ImageVariationRequest.Builder()
        //         .SetSender(task.sender)
        //         .SetIgnoreLogs(task.ignoreLogs)
        //         .SetImage(task.prompt)
        //         .SetModel(task.model.Id)
        //         .SetN(task.n)
        //         .SetOutputPath(task.outputPath)
        //         .SetCancellationToken(task.token);

        //     ImageSize? size = task.GetSize();
        //     if (size != null) builder.SetSize(size.Value);

        //     GeneratedImage result = await builder.Build().ExecuteAsync();

        //     if (result != null)
        //     {
        //         size ??= OpenAIUtils.GetDefaultImageSize(task.model);
        //         ImageQuality quality = OpenAIUtils.GetDefaultImageQuality(task.model);
        //         Usage usage = OpenAIUtils.CreateImageUsage(size.Value, quality, task.n);
        //         result.Usage = usage;
        //     }

        //     return result;
        // }

        internal override async UniTask<GeneratedAudio> GenerateSpeechAsync(GENSpeechTask task)
        {
            SpeechRequest.Builder builder = new SpeechRequest.Builder()
                .SetSender(task.sender)
                .SetIgnoreLogs(task._ignoreLogs)
                .SetModel(task.model.Id)
                .SetPrompt(task.prompt)
                .SetVoice(task.voice)
                .SetOutputPath(task._outputPath)
                .SetResponseFormat(task.outputMimeType)
                .SetCancellationToken(task.token);

            if (task.speed != null) builder.SetSpeed(task.speed.Value);

            GeneratedAudio result = await builder.Build().ExecuteAsync();

            if (result != null && task.prompt != null)
            {
                Usage usage = Usage.PerCharacter(task.prompt.Length);
                result.Usage = usage;
            }

            return result;
        }

        internal override async UniTask<Transcript> GenerateTranscriptAsync(GENTranscriptTask task)
        {
            TranscriptionRequest.Builder builder = new TranscriptionRequest.Builder()
                .SetSender(task.sender)
                .SetIgnoreLogs(task._ignoreLogs)
                .SetModel(task.model.Id)
                .SetFile(task.prompt)
                .SetCancellationToken(task.token);

            if (task.language != null) builder.SetLanguage(task.language.Value);

            return await builder.Build().ExecuteAsync();
        }

        internal override async UniTask<Transcript> GenerateTranslationAsync(GENTranslationTask task)
        {
            TranslationRequest.Builder builder = new TranslationRequest.Builder()
                .SetSender(task.sender)
                .SetIgnoreLogs(task._ignoreLogs)
                .SetModel(task.model.Id)
                .SetFile(task.prompt)
                .SetCancellationToken(task.token);

            return Transcript.Translation(await builder.Build().ExecuteAsync());
        }

        internal override UniTask<SafetyRating[]> GenerateModerationAsync(GENModerationTask task)
        {
            ModerationRequest.Builder builder = new ModerationRequest.Builder()
                .SetSender(task.sender)
                .SetIgnoreLogs(task._ignoreLogs)
                .SetModel(task.model.Id)
                .SetPrompt(task.prompt)
                .SetSafetySettings(task.safetySettings)
                .SetCancellationToken(task.token);

            return builder.Build().ExecuteAsync();
        }

        internal override async UniTask<QueryResponse<IModelData>> ListModelsAsync(Query query = null)
        {
            QueryResponse<OpenAIModelData> res = await OpenAI.DefaultInstance.Models.ListAsync(query as CursorQuery);
            return res.ToSoftRef<OpenAIModelData, IModelData>();
        }

        internal override async UniTask<IModelData> RetrieveModelAsync(string modelId)
        {
            return await OpenAI.DefaultInstance.Models.RetrieveAsync(modelId);
        }
    }
}