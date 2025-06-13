using System;
using System.Collections.Generic;
using Glitch9.AIDevKit.Client;
using Glitch9.IO.Networking.RESTApi;

namespace Glitch9.AIDevKit
{
    internal class ChatCompletionStreamHandlerBuilder
    {
        private Action _onStart;
        private Action<string> _onError;
        private Action<ChatCompletion> _onDone;

        // single response
        private Action<string> _onReceiveText;
        private Action<string> _onReceiveRefusal;
        private Action<ToolCall[]> _onReceiveToolCalls;

        // multi response
        private Action<ChatDelta[]> _onReceiveDeltaChunk;

        internal void SetOnStart(Action onStart) => _onStart = onStart;
        internal void SetOnError(Action<string> onError) => _onError = onError;
        internal void SetOnDone(Action<ChatCompletion> onDone) => _onDone = onDone;

        internal void SetOnReceiveText(Action<string> onTextDelta) => _onReceiveText = onTextDelta;
        internal void SetOnReceiveRefusal(Action<string> onRefusal) => _onReceiveRefusal = onRefusal;
        internal void SetOnReceiveToolCalls(Action<ToolCall[]> onToolCalls) => _onReceiveToolCalls = onToolCalls;

        internal void SetOnReceiveDeltaChunk(Action<ChatDelta[]> onDeltaChunk) => _onReceiveDeltaChunk = onDeltaChunk;

        internal ChatCompletionStreamHandler Build()
        {
            if (_onReceiveDeltaChunk != null)
            {
                return new MultiResponseStreamHandler(
                    _onStart,
                    _onReceiveDeltaChunk,
                    _onError,
                    _onDone
                );
            }

            return new SingleResponseStreamHandler(
                _onStart,
                _onReceiveText,
                _onReceiveRefusal,
                _onReceiveToolCalls,
                _onError,
                _onDone
            );
        }
    }

    public interface IChatCompletionStreamHandler : ITextStreamHandler
    {
        IChatCompletionStreamHandler SetFactory(Func<string, IEnumerable<ChatCompletionChunk>> chunkFactory);
        IChatCompletionStreamHandler SetTask(GENResponseTask task);
        IChatCompletionStreamHandler SetOnDone(Action<ChatCompletion> onDone);
    }

    public abstract class ChatCompletionStreamHandler : IChatCompletionStreamHandler
    {
        public Action onStart;
        public Action<string> onError;
        public Action<ChatCompletion> onDone;
        protected GENResponseTask _task;
        protected Func<string, IEnumerable<ChatCompletionChunk>> _chunkFactory;
        protected ChatCompletionChunk _lastChunk;
        protected bool _isDone = false;

        // Required. Called from each API clients (e.g. OpenAI, Ollama, etc.)
        public IChatCompletionStreamHandler SetFactory(Func<string, IEnumerable<ChatCompletionChunk>> chunkFactory)
        {
            _chunkFactory = chunkFactory;
            return this;
        }

        // Optional.
        public IChatCompletionStreamHandler SetTask(GENResponseTask task)
        {
            _task = task;
            return this;
        }

        public IChatCompletionStreamHandler SetOnDone(Action<ChatCompletion> onDone)
        {
            this.onDone += onDone;
            return this;
        }

        public void StartStreaming() => onStart?.Invoke();
        public void OnReceiveStreamedData(string streamedText)
        {
            //GNDebug.Mark(streamData); 
            if (_chunkFactory == null) throw new ArgumentNullException(nameof(_chunkFactory));
            if (_isDone || string.IsNullOrEmpty(streamedText)) return;

            foreach (ChatCompletionChunk chunk in _chunkFactory(streamedText))
            {
                if (chunk == null) continue;

                if (chunk.isDone)
                {
                    FinishStreaming();
                    return;
                }

                if (chunk.isError)
                {
                    OnError(chunk.errorMessage);
                    return;
                }

                _lastChunk = chunk;
                OnReceiveChunk(chunk);
            }
        }

        protected abstract void OnReceiveChunk(ChatCompletionChunk chunk);
        protected abstract ChatCompletion CreateResult();

        public void FinishStreaming()
        {
            if (_isDone) return;
            _isDone = true;

            ChatCompletion lastDelta = _lastChunk?.delta;

            if (_task != null && _task.enableHistory)
            {
                lastDelta = CreateResult();
                if (_task.textProcessor != null) lastDelta.ProcessText(_task.textProcessor);
                PromptRecordFactory.Create(_task, lastDelta);
            }

            onDone?.Invoke(lastDelta);
        }

        public void OnError(string error)
        {
            if (_isDone) return;
            onError?.Invoke(AIClientErrorFormatter.Format(error));
        }

        public void OnProgress(float progress)
        {
            // do nothing
        }
    }
}