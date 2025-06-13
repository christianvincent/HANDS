namespace Glitch9.AIDevKit.Client
{
    internal static class AIClientErrorFormatter
    {
        internal static string Format(string error)
        {
            if (string.IsNullOrWhiteSpace(error)) return "Unknown error has occurred.";

            if (error.Contains("Unrecognized request argument supplied: reasoning_effort"))
            {
                return "This model does not support the reasoning_effort parameter. Please use reasoning models (e.g. o-series), or do not set the reasoning_effort parameter.";
            }

            return error;
        }
    }
}