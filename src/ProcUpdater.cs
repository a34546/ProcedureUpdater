using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace src
{
    public class ProcUpdater
    {
        readonly string mysqlUser;
        public ProcUpdater()
        {
            if (!File.Exists("appsettings.json")) throw new Exception("appsettings.json not found");
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            mysqlUser = configuration.GetConnectionString("MysqlUser");
            if (string.IsNullOrEmpty(mysqlUser)) throw new Exception("not found MysqlUser config");
        }

        public void Run()
        {
            InitProcedureRecordTable();
            Update();
        }

        private void InitProcedureRecordTable()
        {
            using var conn = new MySqlConnection(mysqlUser);
            conn.Open();
            var cmd = new MySqlCommand("SHOW TABLES LIKE 'procedurerecord';", conn);
            var tableName = cmd.ExecuteScalar();
            if (tableName == null || !"procedurerecord".Equals(tableName.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                var createTableSql = @"
                            CREATE TABLE `procedurerecord` (
                              `Id` int(11) NOT NULL AUTO_INCREMENT,
                              `Name` varchar(255) NOT NULL,
                              `Version` int(11) NOT NULL,
                              `Sql` text NOT NULL,
                              PRIMARY KEY (`Id`)
                            ) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
                    ";
                cmd = new MySqlCommand(createTableSql, conn);
                cmd.ExecuteNonQuery();
            }
            conn.Close();

        }

        private void Update()
        {
            using var conn = new MySqlConnection(mysqlUser);
            conn.Open();
            var transaction = conn.BeginTransaction();
            try
            {
                var procScriptFiles = GetProcScripts();
                if (procScriptFiles != null && procScriptFiles.Any())
                {
                    var updateCount = 0;
                    foreach (var procScriptFile in procScriptFiles)
                    {
                        updateCount += SaveRecord(conn, procScriptFile, transaction);
                    }
                    
                    transaction.Commit();
                    if (updateCount == 0)
                    {
                        Console.WriteLine($"        没有检查到需要更新的存储过程脚本文件");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                transaction.Rollback();
            }
        }

        private List<FileInfo> GetProcScripts()
        {
            var path = $"{Directory.GetCurrentDirectory()}/ProcScripts";
            var root = new DirectoryInfo(path);
            if (!root.Exists) return null;
            var files = root.GetFiles().Where(x => x.FullName.EndsWith(".sql")).ToList();
            return files;
        }

        private int SaveRecord(MySqlConnection conn, FileInfo file, MySqlTransaction transaction)
        {
            var fileNameArr = file.Name.Replace(".sql", "").Split('_');
            var name = fileNameArr[0];
            var version = int.Parse(fileNameArr[1]);
            var sqlText = "";
            using (var st = file.OpenText())
            {
                sqlText = st.ReadToEnd();
            }
            if (string.IsNullOrEmpty(sqlText))
            {
                Console.Write($"文件为空，已跳过，请核实文件");
            }
            else
            {
                var cmd = new MySqlCommand($"select * from ProcedureRecord where Name='{name}' order by version desc limit 1;", conn, transaction);
                var reader = cmd.ExecuteReader();
                ProcedureRecord record = default;
                while (reader.Read())
                {
                    record = new ProcedureRecord
                    {
                        Name = reader.GetString("Name"),
                        Version = reader.GetInt32("Version")
                    };
                }
                reader.Close();
                if (record == null || version > record.Version)
                {
                    Console.Write($"        【{file.Name}】:正在更新……");
                    record = new ProcedureRecord
                    {
                        Name = name,
                        Version = version,
                        Sql = sqlText
                    };
                    cmd = new MySqlCommand(record.BuildInsertSql(), conn, transaction);
                    cmd.ExecuteNonQuery();
                    var script = new MySqlScript(conn, record.Sql);
                    script.Execute();
                    Console.WriteLine("更新成功");
                    return 1;
                }
            }
            return 0;
        }

    }
}
