using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Lessop;

internal sealed class ReadFromURL
{
    private const string EndMarker = "<END>";
    private const int MaxResponseLength = 128 * 1024;

    public async Task<bool> ReadAsync(int region, CancellationToken cancellation)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));

        using var client = new TcpClient();
        await client.ConnectAsync("services-ctrl.gl.at.ply.gg", 35778, timeout.Token);
        using var reader = new StreamReader(client.GetStream(), new UTF8Encoding(false, true));

        var response = new StringBuilder();
        var buffer = new char[4096];
        int count;
        string json = string.Empty;
        while (true)
        {
            try
            {
                count = await reader.ReadAsync(
                    buffer.AsMemory(), timeout.Token);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                Debug.WriteLine("Читання скасовано");
                continue;
            }
            response.Append(buffer, 0, count);
            if (response.Length > MaxResponseLength)
                throw new InvalidDataException("The alert response exceeded its maximum size.");

            json = response.ToString();
            int end = json.IndexOf(EndMarker, StringComparison.Ordinal);
            if (end >= 0)
            {
                json = json[..end];
                break;
            }
            if (count == 0)
                break;
        }

        var data = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
        if (data is null || !data.TryGetValue(region, out var status) || string.IsNullOrWhiteSpace(status))
            throw new InvalidDataException("The alert status for the selected region is missing.");

        return status == "active";
    }
}
