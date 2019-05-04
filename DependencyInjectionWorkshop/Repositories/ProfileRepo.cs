﻿using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Repositories
{
    public interface IProfile
    {
        string GetPassword(string accountId);
    }

    public class ProfileRepo : IProfile
    {
        public string GetPassword(string accountId)
        {
            var passwordFromDb = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }
}