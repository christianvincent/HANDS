using System;
using System.Collections.Generic;
using Glitch9.AIDevKit.Client;
using UnityEngine;
using UnityEngine.Events;

namespace Glitch9.AIDevKit
{
    public abstract class ChatCompletionStreamReceiver : MonoBehaviour, IChatCompletionStreamHandler
    {
        [SerializeField] private UnityEvent onStart;
        [SerializeField] private UnityEvent<string> onError;
        [SerializeField] private UnityEvent<ChatCompletion> onDone;

        protected GENResponseTask _task;
        protected Func<string, IEnumerable<ChatCompletionChunk>> _chunkFactory;
        protected ChatCompletionChunk _lastChunk;
        protected bool _isDone = false;
        protected Action<ToolCall[]> _onFunctionManagerCalls;

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
            this.onDone.AddListener((ChatCompletion completion) =>
            {
                onDone?.Invoke(completion);
            });
            return this;
        }

        public IChatCompletionStreamHandler SetFunctionManagerCalls(Action<ToolCall[]> onFunctionManagerCalls)
        {
            _onFunctionManagerCalls = onFunctionManagerCalls;
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