
namespace Arekntt.Core
{
    public interface ICoroutineRunner
    {
        void RunDelayed(System.Action action, float delay);
    }
}
