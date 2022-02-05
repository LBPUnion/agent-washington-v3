﻿using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using LBPUnion.AgentWashington.Core.Logging;
using LBPUnion.HttpMonitor.Settings;

namespace LBPUnion.HttpMonitor;

public sealed class MonitorStatus
{
    private int _statusCode;
    private ServerStatus _status;
    private MonitorTarget _target;

    public ServerStatus ServerStatus => _status;
    public int StatusCode => _statusCode;
    
    internal MonitorStatus(MonitorTarget target)
    {
        _target = target;
    }

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
    
    internal void CheckStatus()
    {
#pragma warning disable CS0618
        var url = Url;
        
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
            _status = ServerStatus.Offline;
            _statusCode = (int) res.StatusCode;

            Logger.Log($"[{_target.Name}] Server responded with an error. {ex.Message} (status code {_statusCode})");
        }


        // Stop hijacking SSL certificate chain validation.
        if (_target.UseSsl && _target.IgnoreSslCertErrors)
        {
            ServicePointManager.ServerCertificateValidationCallback -= IgnoreCertChecks;
        }
#pragma warning restore CS0618
    }
    
    private bool IgnoreCertChecks(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslpolicyerrors)
    {
        return true;
    }
}

public enum ServerStatus
{
    Online,
    Offline,
    DnsError,
}