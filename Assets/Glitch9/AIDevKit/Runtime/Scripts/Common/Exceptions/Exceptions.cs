using System;
using Glitch9.AIDevKit.GENTasks;

namespace Glitch9.AIDevKit
{
    public class EmptyPromptException : Exception
    {
        public EmptyPromptException(Type promptType) : base($"Your prompt({promptType.Name}) is empty or null.") { }
    }

    public class BlockedPromptException : Exception
    {
        public PromptFeedback Feedback { get; }
        public BlockedPromptException(PromptFeedback feedback) : base(feedback.ToString()) => Feedback = feedback;
    }

    public class EmptyResponseException : Exception
    {
        public EmptyResponseException(Model model) : base($"Model {model.Id} returned null or empty result.") { }
        public EmptyResponseException(int endpointType) : base($"{EndpointType.GetName(endpointType)} task returned null or empty result.") { }
    }

    public class BrokenVoiceException : Exception
    {
        public BrokenVoiceException() : base("Currently selected voice is broken. This is a critical error. Please report this issue to the developer.") { }
    }

    public class BrokenModelException : Exception
    {
        public BrokenModelException() : base("Currently selected model is broken. This is a critical error. Please report this issue to the developer.") { }
    }

    public class InterruptedResponseException : Exception
    {
        public InterruptedResponseException(StopReason stopReason) : base(stopReason.GetMessage()) { }
    }

    public class BrokenResponseException : Exception
    {
        public BrokenResponseException(string message, Exception exception) : base(message, exception) { }
    }

    public class DeprecatedModelException : Exception
    {
        public DeprecatedModelException(Model model) : base($"Model {model.Id} is deprecated. Please use a different model.") { }
    }

    public class RateLimitExceededException : Exception
    {
        public RateLimitExceededException(string message = "Request rate limit exceeded.") : base(message) { }
    }

    public class NotSupportedFeatureException : NotSupportedException
    {
        public ModelFeature Capability { get; }
        public Model Model { get; }
        public NotSupportedFeatureException(Model model, ModelFeature cap) : base($"Model {model.Id} does not support {cap} feature. Please use a different model.")
        {
            Model = model;
            Capability = cap;
        }
    }

    /// <summary>
    /// Thrown when a GenAI provider does not support a specific feature.
    /// </summary>
    public class InvalidEndpointException : NotSupportedException
    {
        /// <summary>
        /// The name of the api provider that does not support the feature.
        /// </summary>
        public Api Api { get; }

        /// <summary>
        /// The name of the unsupported endpoint.
        /// Refer to the <see cref="GENTasks.EndpointType"/> for the list of supported endpoints.
        /// </summary>
        public int EndpointType { get; }

        public InvalidEndpointException(Api api, int endpointType) : base($"{api} does not support {GENTasks.EndpointType.GetName(endpointType)} endpoint.")
        {
            Api = api;
            EndpointType = endpointType;
        }
    }
}