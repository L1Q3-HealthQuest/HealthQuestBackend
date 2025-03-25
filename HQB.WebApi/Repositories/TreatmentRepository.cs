using Dapper;
using HQB.WebApi.Models;
using HQB.WebApi.Interfaces;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace HQB.WebApi.Repositories;
public class TreatmentRepository : ITreatmentRepository
{
    private readonly string _connectionString;

    public TreatmentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<Treatment>> GetAllTreatmentsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT * FROM Treatment";
        return await connection.QueryAsync<Treatment>(sql);
    }

    public async Task<Treatment?> GetTreatmentByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT * FROM Treatment WHERE ID = @ID";
        return await connection.QueryFirstOrDefaultAsync<Treatment>(sql, new { ID = id });
    }

    public async Task<int> AddTreatmentAsync(Treatment treatment)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "INSERT INTO Treatment (ID, Name, Description, Price) VALUES (@ID, @Name, @Description, @Price)";
        return await connection.ExecuteAsync(sql, treatment);
    }

    public async Task<int> UpdateTreatmentAsync(Treatment treatment)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "UPDATE Treatment SET Name = @Name, Description = @Description, Price = @Price WHERE ID = @ID";
        return await connection.ExecuteAsync(sql, treatment);
    }

    public async Task<int> DeleteTreatmentAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "DELETE FROM Treatment WHERE ID = @ID";
        return await connection.ExecuteAsync(sql, new { ID = id });
    }
}
