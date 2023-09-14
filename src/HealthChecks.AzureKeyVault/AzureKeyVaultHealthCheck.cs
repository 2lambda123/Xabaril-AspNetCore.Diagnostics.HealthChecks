using System.Collections.Concurrent;
using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureKeyVault;

public class AzureKeyVaultHealthCheck : IHealthCheck
{
    private readonly AzureKeyVaultOptions _options;
    private readonly Uri _keyVaultUri;
    private readonly TokenCredential _azureCredential;

    private static readonly ConcurrentDictionary<Uri, SecretClient> _secretClientsHolder = new();
    private static readonly ConcurrentDictionary<Uri, KeyClient> _keyClientsHolder = new();
    private static readonly ConcurrentDictionary<Uri, CertificateClient> _certificateClientsHolder = new();

    public AzureKeyVaultHealthCheck(Uri keyVaultUri, TokenCredential credential, AzureKeyVaultOptions options)
    {
        _keyVaultUri = Guard.ThrowIfNull(keyVaultUri);
        _azureCredential = Guard.ThrowIfNull(credential);
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/785
            if (_options._keys.Count == 0 && _options.Certificates.Count == 0 && _options._secrets.Count == 0)
            {
                return HealthCheckResult.Unhealthy("No keys, certificates or secrets configured.");
            }

            foreach (string secret in _options.Secrets)
            {
                var secretClient = CreateSecretClient();
                await secretClient.GetSecretAsync(secret, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            foreach (string key in _options.Keys)
            {
                var keyClient = CreateKeyClient();
                await keyClient.GetKeyAsync(key, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            foreach (var (key, checkExpired) in _options.Certificates)
            {
                var certificateClient = CreateCertificateClient();
                var certificate = await certificateClient.GetCertificateAsync(key, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (checkExpired && certificate.Value.Properties.ExpiresOn.HasValue)
                {
                    var expirationDate = certificate.Value.Properties.ExpiresOn.Value;

                    if (expirationDate < DateTime.UtcNow)
                    {
                        throw new Exception($"The certificate with key {key} has expired with date {expirationDate}");
                    }
                }
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private KeyClient CreateKeyClient()
    {
        if (!_keyClientsHolder.TryGetValue(_keyVaultUri, out var client))
        {
            client = new KeyClient(_keyVaultUri, _azureCredential);
            _keyClientsHolder.TryAdd(_keyVaultUri, client);
        }

        return client;
    }

    private SecretClient CreateSecretClient()
    {
        if (!_secretClientsHolder.TryGetValue(_keyVaultUri, out var client))
        {
            client = new SecretClient(_keyVaultUri, _azureCredential);
            _secretClientsHolder.TryAdd(_keyVaultUri, client);
        }

        return client;
    }

    private CertificateClient CreateCertificateClient()
    {
        if (!_certificateClientsHolder.TryGetValue(_keyVaultUri, out var client))
        {
            client = new CertificateClient(_keyVaultUri, _azureCredential);
            _certificateClientsHolder.TryAdd(_keyVaultUri, client);
        }

        return client;
    }
}
