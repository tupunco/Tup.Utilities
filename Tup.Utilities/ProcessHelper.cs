using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tup.Utilities
{
    /// <summary>
    /// 进程处理/Process Helper
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly static Logging.ILogger Log = Logging.LogManager.GetLogger(typeof(ProcessHelper));

        #region 模拟控制台信号需要使用的api
        [DllImport("kernel32.dll")]
        public static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        #endregion

        /// <summary>
        /// Process Start
        /// </summary>
        /// <param name="startFileName"></param>
        /// <param name="startFileArg"></param>
        public static bool ProcessStart(string startFileName, string startFileArg = null)
        {
            ThrowHelper.ThrowIfNull(startFileName, "startFileName");

            var cmdProcess = new Process();
            var si = cmdProcess.StartInfo;
            si.FileName = startFileName;      // 命令  
            if (startFileArg.HasValue())
                si.Arguments = startFileArg;      // 参数  

            try
            {
                cmdProcess.Start();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("ProcessStart-startFileName:{0}-startFileArg:{1}-ex:{2}"
                                    .Fmt(startFileName, startFileArg, ex));

                //Console.WriteLine("ProcessHelper-file:{0}-arg:{1}-ex:{2}",
                //                    startFileName, startFileArg, ex);
                return false;
            }
        }

        /// <summary>
        /// Kill CurrentProcess
        /// </summary>
        public static void KillCurrentProcess()
        {
            var prc = Process.GetCurrentProcess();
            prc.Kill();
        }

        /// <summary>
        /// 功能: 停止录制
        /// </summary>
        private static void StopProcess(Process p)
        {
            ThrowHelper.ThrowIfNull(p, "p");

            var pId = p.Id;
            if (AttachConsole(p.Id))
            {
                SetConsoleCtrlHandler(IntPtr.Zero, true);
                try
                {
                    if (!GenerateConsoleCtrlEvent(0, 0))
                        p.WaitForExit();

                    GenerateConsoleCtrlEvent(0, p.SessionId);
                }
                catch (Exception ex)
                {
                    Log.Error("ProcessStart-StopProcess:{0}".Fmt(ex));
                    ex = null;
                    //Console.WriteLine(ex);
                }
                finally
                {
                    FreeConsole();
                    SetConsoleCtrlHandler(IntPtr.Zero, false);
                }
            }
        }
        /// <summary>
        /// Action Stop
        /// </summary>
        public class ActionStop
        {
            public Action Do { get; set; }
        }

        /// <summary>
        /// Execute Command Action
        /// </summary>
        /// <param name="startFileName"></param>
        /// <param name="startFileArg"></param>
        /// <param name="msgAction"></param>
        /// <returns></returns>
        public static bool ExecuteAction(string startFileName,
            string startFileArg,
            Action<string> msgAction,
            ActionStop stopAction = null,
            bool showWindow = false)
        {
            ThrowHelper.ThrowIfNull(msgAction, "msgAction");
            ThrowHelper.ThrowIfNull(startFileName, "startFileName");

            var cmdProcess = new Process();
            var si = cmdProcess.StartInfo;
            si.FileName = startFileName;      // 命令  
            if (startFileArg.HasValue())
                si.Arguments = startFileArg;      // 参数  

            si.CreateNoWindow = !showWindow;         // 不创建新窗口  
            si.UseShellExecute = false;
            si.RedirectStandardInput = true;  // 重定向输入  
            si.RedirectStandardOutput = true; // 重定向标准输出  
            si.RedirectStandardError = true;  // 重定向错误输出  
            si.WindowStyle = ProcessWindowStyle.Hidden;

            cmdProcess.OutputDataReceived += (sender, e) => msgAction(e.Data);
            cmdProcess.ErrorDataReceived += (sender, e) => msgAction(e.Data);

            cmdProcess.EnableRaisingEvents = true; // 启用Exited事件  
            cmdProcess.Exited += (sender, e) =>
            {
                if (cmdProcess == null)
                    return;

                cmdProcess.Close();
                cmdProcess.Dispose();
                cmdProcess = null;
            };

            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();
            if (stopAction != null)
            {
                stopAction.Do = () =>
                {
                    StopProcess(cmdProcess);
                };
            }
            cmdProcess.WaitForExit();

            return true;
        }

    }
}
