using System.Runtime.InteropServices;
namespace CRSim
{
    public partial class NativeMethods
    {
        /// <summary>
        /// 为调用进程分配一个新的控制台。
        /// </summary>
        /// <returns>如果函数成功，返回值为非零；如果失败，返回值为零。</returns>
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AllocConsole();
    }
}
