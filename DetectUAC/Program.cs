using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;

namespace DetectUAC
{
    class Program
    {
        private static Dictionary<uint, DateTime> suspiciousParents = new Dictionary<uint, DateTime>();
        private static readonly string[] TARGET_PARENTS = { "winver.exe", "ComputerDefaults.exe" };
        private static readonly string[] LEGITIMATE_CHILDREN = { "ComputerDefaults.exe", "winver.exe", "conhost.exe", "dllhost.exe" };

        static void Main(string[] args)
        {
            Console.Title = "UAC Bypass Detector v3.1 - Auto Kill";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     UAC BYPASS DETECTOR v3.1 - Auto Kill Mode             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            ManagementEventWatcher watcher = null;

            try
            {
                watcher = new ManagementEventWatcher(
                    new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'"));

                watcher.EventArrived += ProcessCreated;
                watcher.Start();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Detector đã sẵn sàng (Auto Kill Mode)!");
                Console.WriteLine("[+] Chạy example.exe để test...");
                Console.WriteLine("[+] Nhấn Enter để dừng detector...");
                Console.ResetColor();

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.ResetColor();
                Console.ReadKey();
            }
            finally
            {
                watcher?.Stop();
                watcher?.Dispose();
            }
        }

        private static void ProcessCreated(object sender, EventArrivedEventArgs e)
        {
            try
            {
                ManagementBaseObject target = (ManagementBaseObject)e.NewEvent["TargetInstance"];

                string processName = target["Name"]?.ToString() ?? "Unknown";
                uint pid = Convert.ToUInt32(target["ProcessID"]);
                uint parentPid = Convert.ToUInt32(target["ParentProcessID"]);
                string commandLine = target["CommandLine"]?.ToString() ?? "";

                // Ghi nhận parent
                if (Array.Exists(TARGET_PARENTS, p => p.Equals(processName, StringComparison.OrdinalIgnoreCase)))
                {
                    suspiciousParents[pid] = DateTime.Now;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"[PARENT] {processName} | PID: {pid}");
                    Console.ResetColor();
                }

                // Phát hiện UAC Bypass
                if (suspiciousParents.ContainsKey(parentPid))
                {
                    TimeSpan timeDiff = DateTime.Now - suspiciousParents[parentPid];

                    if (timeDiff.TotalSeconds <= 90)
                    {
                        bool isLegit = Array.Exists(LEGITIMATE_CHILDREN, c => c.Equals(processName, StringComparison.OrdinalIgnoreCase));

                        if (!isLegit)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n" + "═".PadRight(80, '═'));
                            Console.WriteLine("🚨  UAC BYPASS DETECTED → ĐANG KILL PROCESS VI PHẠM!");
                            Console.WriteLine("═".PadRight(80, '═'));
                            Console.WriteLine($"   Child Process : {processName}");
                            Console.WriteLine($"   Child PID     : {pid}");
                            Console.WriteLine($"   Parent        : {GetProcessNameById((int)parentPid)} (PID: {parentPid})");
                            Console.WriteLine($"   CommandLine   : {commandLine}");
                            Console.WriteLine("═".PadRight(80, '═') + "\n");
                            Console.ResetColor();

                            // ====================== TỰ ĐỘNG KILL ======================
                            KillProcess((int)pid);

                            LogToFile(pid, processName, parentPid, commandLine);
                        }
                    }
                }
            }
            catch { }
        }

        // Hàm kill process
        private static void KillProcess(int pid)
        {
            try
            {
                Process p = Process.GetProcessById(pid);
                p.Kill();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[+] ĐÃ KILL thành công process PID {pid}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[!] Không kill được PID {pid} → {ex.Message}");
                Console.ResetColor();
            }
        }

        private static string GetProcessNameById(int pid)
        {
            try { return Process.GetProcessById(pid).ProcessName + ".exe"; }
            catch { return "Unknown"; }
        }

        private static void LogToFile(uint childPid, string childName, uint parentPid, string cmd)
        {
            try
            {
                string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | UAC BYPASS DETECTED + KILLED | Child: {childName} (PID:{childPid}) | ParentPID:{parentPid} | CMD: {cmd}\n";
                System.IO.File.AppendAllText("UAC_Bypass_Detected.log", log);
            }
            catch { }
        }
    }
}