using HealthChecks.Network.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Network;

public class ImapHealthCheck : IHealthCheck
{
    private readonly ImapHealthCheckOptions _options;

    public ImapHealthCheck(ImapHealthCheckOptions options)
    {
        _options = Guard.ThrowIfNull(options);

        Guard.ThrowIfNull(_options.Host, true);

        if (_options.Port == default)
        {
            throw new ArgumentNullException(nameof(_options.Port));
        }
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var imapConnection = new ImapConnection(_options);

            if (await imapConnection.ConnectAsync(cancellationToken).ConfigureAwait(false))
            {
                if (_options.AccountOptions.Login)
                {
                    return await ExecuteAuthenticatedUserActionsAsync(context, imapConnection, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Connection to server {_options.Host} has failed - SSL Enabled : {_options.ConnectionType}");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private async Task<HealthCheckResult> ExecuteAuthenticatedUserActionsAsync(HealthCheckContext context, ImapConnection imapConnection, CancellationToken cancellationToken)
    {
        var (User, Password) = _options.AccountOptions.Account;

        if (await imapConnection.AuthenticateAsync(User, Password, cancellationToken).ConfigureAwait(false))
        {
            if (_options.FolderOptions.CheckFolder
                && !await imapConnection.SelectFolderAsync(_options.FolderOptions.FolderName, cancellationToken).ConfigureAwait(false))
            {
                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Folder {_options.FolderOptions.FolderName} check failed.");
            }

            return HealthCheckResult.Healthy();
        }
        else
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Login on server {_options.Host} failed with configured user");
        }
    }
}
