namespace EngineExtensions.Input {
    using UnityEngine;
    public interface IInputReader {
        bool GetButton(string actionPath);
        float GetAxis1D(string actionPath);
        Vector2 GetAxis2D(string actionPath);
    }
}