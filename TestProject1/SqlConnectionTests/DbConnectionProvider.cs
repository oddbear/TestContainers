﻿using Npgsql;
using System.Data.Common;

namespace TestProject1.SqlConnectionTests;

public sealed class DbConnectionProvider
{
    private readonly string _connectionString;

    public DbConnectionProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
