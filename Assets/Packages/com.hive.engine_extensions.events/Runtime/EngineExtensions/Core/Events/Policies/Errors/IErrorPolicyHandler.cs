using System;

namespace EngineExtensions.Core.Events.Policies.Errors {
    public interface IErrorPolicyHandler {
        ErrorPolicyResult ExecuteHandle(Action executable);
    }
}