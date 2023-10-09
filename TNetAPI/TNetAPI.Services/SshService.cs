using System.Text;
using Renci.SshNet;

namespace TNetAPI.Services;

public class SshService
{
    private readonly ShellStream _shellStream;
    private readonly SshClient _sshClient;

    public SshService(string host, string username, string password)
    {
        _sshClient = new SshClient(host, username, password);
        _sshClient.Connect();
        _shellStream = _shellStream ?? _sshClient.CreateShellStream("terminal", 80, 24, 800, 600, 4096);
        InitializeShell();
    }

    private void InitializeShell()
    {
        EnsureConnected();
        _shellStream.WriteLine("configure terminal");
        _shellStream.WriteLine("set cli pagination off");
        _shellStream.WriteLine("exit");
        _shellStream.WriteLine("clear screen");
        _shellStream.Flush();
    }

    public string ExecuteCommand(string command)
    {
        EnsureConnected();
        var cmd = _sshClient.CreateCommand(command);
        return cmd.Execute();
    }

    private void EnsureConnected()
    {
        if (!_sshClient.IsConnected) _sshClient.Connect();
    }

    public void Disconnect()
    {
        if (_sshClient.IsConnected) _sshClient.Disconnect();
    }


    public string ExecuteCustomCommand(string command)
    {
        EnsureConnected();


        _shellStream.WriteLine(command);
        _shellStream.Flush();

        Thread.Sleep(200);

        var outputBuilder = new StringBuilder();
        var lastDataReceivedTime = DateTime.Now;

        while (true)
        {
            if ((DateTime.Now - lastDataReceivedTime).TotalSeconds > 10) break;

            if (_shellStream.DataAvailable)
            {
                var result = _shellStream.Read();
                outputBuilder.Append(result);
            }
            else
            {
                Thread.Sleep(500);
            }
        }

        return ProcessOutput(outputBuilder.ToString());
    }

    private static string ProcessOutput(string output)
    {
        var clearScreenSequence = "\u001B[H\u001B[J"; // This is the escape sequence for clearing the screen
        var cursorMoveSequence = "\u001B[100B"; // Move cursor down by 100 lines

        // Split the output by the clearScreenSequence
        var parts = output.Split(new[] { clearScreenSequence }, StringSplitOptions.None);

        var processedOutput = parts.Length > 1 ? parts[^1] : output;

        // Remove cursorMoveSequence from the processed output
        processedOutput = processedOutput.Replace(cursorMoveSequence, "");

        return processedOutput;
    }
}