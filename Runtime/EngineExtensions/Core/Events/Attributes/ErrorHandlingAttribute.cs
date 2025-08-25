using System;

namespace EngineExtensions.Core.Events.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    public class ErrorHandlingAttribute : Attribute {
        public ErrorPolicy Policy { get; }
        public ErrorHandlingAttribute(ErrorPolicy policy) {
            Policy = policy;
        }
    }
}