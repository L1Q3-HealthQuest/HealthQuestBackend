﻿using Dapper;
using HQB.WebApi.Models;
using HQB.WebApi.Interfaces;
using Microsoft.Data.SqlClient;

namespace HQB.WebApi.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly string _connectionString;

        public DoctorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Doctor";
            return await connection.QueryAsync<Doctor>(sql);
        }

        public async Task<Doctor?> GetDoctorByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Doctor WHERE ID = @Id";
            return await connection.QueryFirstOrDefaultAsync<Doctor>(sql, new { Id = id });
        }

        public async Task<int> AddDoctorAsync(Doctor doctor)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO Doctor (FirstName, LastName, Specialization)
                VALUES (@FirstName, @LastName, @Specialization)";
            return await connection.ExecuteAsync(sql, doctor);
        }

        public async Task<int> UpdateDoctorAsync(Doctor doctor)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE Doctor
                SET FirstName = @FirstName, LastName = @LastName, Specialization = @Specialization
                WHERE ID = @ID";
            return await connection.ExecuteAsync(sql, doctor);
        }

        public async Task<int> DeleteDoctorAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "DELETE FROM Doctor WHERE ID = @Id";
            return await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
