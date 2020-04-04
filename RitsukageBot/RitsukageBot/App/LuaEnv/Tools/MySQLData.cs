using System;
using MySql.Data.MySqlClient;

namespace Native.Csharp.App.LuaEnv.Tools
{
    class MySQLData
    {
        public static MySqlConnection NewConnection(string connectString) {
            return new MySqlConnection(connectString);
        }

        public static MySqlCommand NewCommand(string command, MySqlConnection connection)
        {
            return new MySqlCommand(command, connection);
        }
    }
}
