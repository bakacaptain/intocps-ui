using System.Diagnostics;

namespace SimpleDiagram.Utils
{
    public class SimpleProcessSpawner
    {
        public static void Start(string processname, string args)
        {
            if (processname == string.Empty) return;
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = processname,
                    Arguments = args,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };
            proc.Start();
            proc.WaitForExit();
        }
    }
}