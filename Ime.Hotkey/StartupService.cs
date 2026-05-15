using System.Diagnostics;
using System.Security;
using Microsoft.Win32;

namespace Ime.Hotkey;

/// <summary>
/// Manages Windows startup registration via HKCU Run registry key and Task Scheduler.
/// </summary>
internal static class StartupService
{
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ImeHotkey";
    private const string TaskName = "ImeHotkey_Logon";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        return key?.GetValue(AppName) is not null;
    }

    public static void SetEnabled(bool enable)
    {
        SetRegistryEnabled(enable);
        SetTaskSchedulerEnabled(enable);
    }

    private static void SetRegistryEnabled(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        if (key is null) return;

        if (enable)
        {
            var path = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(path))
                key.SetValue(AppName, $"\"{path}\"");
        }
        else
        {
            key.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }

    private static void SetTaskSchedulerEnabled(bool enable)
    {
        try
        {
            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath)) return;

                // /SC ONLOGON は管理者権限が必要なため、XML 経由で LogonTrigger を登録する
                var userId = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                var xml = $"""
                    <?xml version="1.0" encoding="UTF-16"?>
                    <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
                      <Triggers>
                        <LogonTrigger>
                          <Enabled>true</Enabled>
                          <UserId>{SecurityElement.Escape(userId)}</UserId>
                          <Delay>PT10S</Delay>
                        </LogonTrigger>
                      </Triggers>
                      <Principals>
                        <Principal id="Author">
                          <UserId>{SecurityElement.Escape(userId)}</UserId>
                          <LogonType>InteractiveToken</LogonType>
                          <RunLevel>LeastPrivilege</RunLevel>
                        </Principal>
                      </Principals>
                      <Settings>
                        <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
                        <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                        <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                        <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
                        <AllowStartOnDemand>true</AllowStartOnDemand>
                        <Enabled>true</Enabled>
                        <Hidden>false</Hidden>
                      </Settings>
                      <Actions Context="Author">
                        <Exec>
                          <Command>{SecurityElement.Escape(exePath)}</Command>
                        </Exec>
                      </Actions>
                    </Task>
                    """;

                var tmpPath = Path.Combine(Path.GetTempPath(), $"{TaskName}.xml");
                try
                {
                    File.WriteAllText(tmpPath, xml, System.Text.Encoding.Unicode);
                    RunSchtasks($"/Create /F /TN \"{TaskName}\" /XML \"{tmpPath}\"");
                }
                finally
                {
                    try { File.Delete(tmpPath); } catch { }
                }
            }
            else
            {
                RunSchtasks($"/Delete /F /TN \"{TaskName}\"");
            }
        }
        catch
        {
            // タスクスケジューラの操作失敗は無視（レジストリ側で起動可能）
        }
    }

    private static void RunSchtasks(string arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        process.Start();
        process.WaitForExit(10_000);
    }
}
