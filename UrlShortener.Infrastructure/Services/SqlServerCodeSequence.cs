using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Services;

/// <summary>
/// Numbers for codes come from a SQL Server sequence: it hands out values atomically and outside
/// the transaction, so two concurrent requests never receive the same code and a rolled back
/// insert does not cause a value to be issued twice.
/// </summary>
public sealed class SqlServerCodeSequence(AppDbContext context) : ICodeSequence
{
    public async Task<long> NextAsync(CancellationToken cancellationToken = default)
    {
        // A raw DbCommand rather than SqlQueryRaw: EF wraps such a query into a subquery,
        // and SQL Server does not allow NEXT VALUE FOR inside one.
        var connection = context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT NEXT VALUE FOR [{AppDbContext.CodeSequenceName}]";
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();

        var openedHere = connection.State != ConnectionState.Open;

        if (openedHere)
        {
            await context.Database.OpenConnectionAsync(cancellationToken);
        }

        try
        {
            var value = await command.ExecuteScalarAsync(cancellationToken);

            return Convert.ToInt64(value);
        }
        finally
        {
            if (openedHere)
            {
                await context.Database.CloseConnectionAsync();
            }
        }
    }
}
