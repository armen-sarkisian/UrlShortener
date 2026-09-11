using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Services;

/// <summary>
/// Числа для кодов берутся из последовательности SQL Server: она выдаёт значения
/// атомарно и вне транзакции, поэтому два параллельных запроса не получат один код
/// и откат вставки не приводит к повторной выдаче.
/// </summary>
public sealed class SqlServerCodeSequence(AppDbContext context) : ICodeSequence
{
    public async Task<long> NextAsync(CancellationToken cancellationToken = default)
    {
        // Именно DbCommand, а не SqlQueryRaw: EF оборачивает такой запрос в подзапрос,
        // а NEXT VALUE FOR внутри подзапроса SQL Server не допускает.
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
