using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.AgentWashington.Core.Persistence;
using LBPUnion.HttpMonitor.Settings;

namespace LBPUnion.HttpMonitor;

public sealed class MonitorStatus
{
    private bool _hasChanged = false;
    private int _statusCode;
    private ServerStatus _status;
    private MonitorTarget _target;
    private DatabaseManager _database;

    public ServerStatus ServerStatus => _status;
    public int StatusCode => _statusCode;

    public string Name => _target.Name;
    
    internal MonitorStatus(DatabaseManager database, MonitorTarget target)
    {
        _database = database;
        _target = target;
    }

    public bool HasStatusChanged => _hasChanged;

    public string Url
    {
        get
        {
            var proto = _target.UseSsl ? "https://" : "http://";
            proto += _target.Host;
            if (_target.Port != 80 && _target.Port != 443)
            {
                proto += ":" + _target.Port.ToString();
            }

            proto += _target.Path;

            return proto;
        }
    }

    private void RestoreFromDatabase()
    {
        _database.OpenDatabase(db =>
        {
            var data = db.GetCollection<ServerStatusHistoryData>("statusHistoryData");

            var status = data.FindOne(x => x.Url == Url);
            if (status != null)
            {
                _status = status.Status;
                _statusCode = status.StatusCode;
            }
        });
    }

    private void UpdateDatabase()
    {
        _database.OpenDatabase(db =>
        {
            var data = db.GetCollection<ServerStatusHistoryData>("statusHistoryData");

            var status = data.FindOne(x => x.Url == Url);
            if (status != null)
            {
                status.Status = _status;
                status.StatusCode = _statusCode;
                data.Update(status);
            }
            else
            {
                status = new ServerStatusHistoryData()
                {
                    Url = this.Url,
                    Status = this._status,
                    StatusCode = _statusCode
                };

                data.Insert(status);
            }
        });
    }
    
    internal void CheckStatus(MonitorSettingsProvider settings)
    {
#pragma warning disable CS0618
        var url = Url;

        RestoreFromDatabase();
        
        var oldStatus = _status;
        var oldCode = _statusCode;
        
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl))
        {
            Logger.Log($"Couldn't parse URL: {url} - this means the monitor target is invalid.", LogLevel.Error);
            return;
        }

        // Start with a DNS lookup.
        try
        {
            Dns.GetHostAddresses(_target.Host);
        }
        catch (Exception ex)
        {
            Logger.Log($"[{_target.Name}]: DNS lookup error - {ex.Message}", LogLevel.Error);
            _status = ServerStatus.DnsError;
            UpdateStatusHistory(settings, oldStatus, oldCode);
            return;
        }
        
        // Hijack SSL cert chain validation if the monitor target is set to SSL and to ignore
        // invalid certificates. This allows us to stop C# from dying.
        if (_target.UseSsl && _target.IgnoreSslCertErrors)
        {
            ServicePointManager.ServerCertificateValidationCallback += IgnoreCertChecks;
        }
        
        var webRequest = WebRequest.Create(parsedUrl);

        try
        {
            var res = webRequest.GetResponse() as HttpWebResponse;
            _status = ServerStatus.Online;
            _statusCode = (int) res.StatusCode;
            Logger.Log($"[{_target.Name}] Server returned status code {_statusCode}.");
        }
        catch (WebException ex)
        {
            var res = ex.Response as HttpWebResponse;
            if (res != null)
            {
                _status = ServerStatus.Offline;
                _statusCode = (int) res.StatusCode;

                Logger.Log(
                    $"[{_target.Name}] Server responded with an error. {ex.Message} (status code {_statusCode})");
            }
            else
            {
                _statusCode = 0;
                _status = ServerStatus.Unknown;

                Logger.Log($"[{_target.Name}] Connection error: " + ex.Message, LogLevel.Error);
            }
        }


        // Stop hijacking SSL certificate chain validation.
        if (_target.UseSsl && _target.IgnoreSslCertErrors)
        {
            ServicePointManager.ServerCertificateValidationCallback -= IgnoreCertChecks;
        }
        
        UpdateStatusHistory(settings, oldStatus, oldCode);
        
#pragma warning restore CS0618

        UpdateDatabase();
    }

    private void UpdateStatusHistory(MonitorSettingsProvider settings, ServerStatus oldStatus, int oldCode)
    {
        _hasChanged = (oldStatus != _status || oldCode != _statusCode);
    }

    private bool IgnoreCertChecks(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslpolicyerrors)
    {
        return true;
    }

    private class ServerStatusHistoryData : IDatabaseObject
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public int StatusCode { get; set; }
        public ServerStatus Status { get; set; }
    }
}

public enum ServerStatus
{
    Online,
    Offline,
    DnsError,
    Unknown
}