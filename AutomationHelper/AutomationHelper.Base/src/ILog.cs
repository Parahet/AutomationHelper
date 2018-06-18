namespace AutomationHelper.Base
{
    public interface ILog
    {
        void Info(string message);
        void Pass(string message);
        void Fail(string message);
        void Warn(string message);
        void AddScreenCapture();
    }
}
