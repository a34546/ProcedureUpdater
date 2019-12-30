using System;
using System.Collections.Generic;
using System.Text;

namespace src
{
    public class ProcedureRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
        public string Sql { get; set; }

        public string BuildInsertSql()
        {
            return $"INSERT INTO `procedurerecord` (`Name`, `Version`, `Sql`) VALUES ('{Name}', '{Version}', '{Sql}');";
        }
    }
}
