﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace RailwayTickets
{
    public class DatabaseManager
    {
        public NpgsqlConnection connection;
        public string connectionString;

        public DatabaseManager(string server, int port, string database, string userId, string password)
        {
            connectionString = $"Server={server};Port={port};Database={database};User Id={userId};Password={password};";
            connection = new NpgsqlConnection(connectionString);
        }
        public void OpenConnection()
        {
            connection.Open();
        }
        public void CloseConnection() 
        { 
            connection.Close(); 
        }
    }
}
