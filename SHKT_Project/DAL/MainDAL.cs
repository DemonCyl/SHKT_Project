using Dapper;
using log4net;
using SHKT_Project.Entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SHKT_Project.Common.SQLHelper;

namespace SHKT_Project.DAL
{
    public class MainDAL
    {

        private ConfigData config;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainDAL(ConfigData data)
        {
            this.config = data;
        }

        #region AssemblyType
        public List<AssemblyType> GetAllAssemblyType()
        {
            string sql = @"select * from AssemblyType";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.Query<AssemblyType>(sql).ToList();
                return re;
            }
        }

        public bool GetExistsType(string name)
        {
            string sql = @"select count(1) from AssemblyType where FAssemblyName = @name";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.QuerySingle<int>(sql, new { name = name }) > 0;
            }
        }

        public bool DeleteAssemblyItem(int id)
        {
            string sql = @"delete from AssemblyType where FInterID = @id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { id = id }) > 0;
            }
        }

        public bool SaveAssemblyItem(AssemblyType info)
        {
            string sql = @"INSERT INTO AssemblyType (FAssemblyName) VALUES (@Name) ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { Name = info.FAssemblyName }) > 0;
            }
        }

        public bool UpdateAssemblyItem(AssemblyType info)
        {
            string sql = @"UPDATE AssemblyType SET FAssemblyName = @Name WHERE FInterID = @Id ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { Name = info.FAssemblyName, Id = info.FInterID }) > 0;
            }
        }
        #endregion

        #region CheckCode
        public List<CheckCodeInfo> GetCheckCodeInfo(string gwId, int typeId)
        {
            string sql = $"select t.*,t1.FAssemblyName from CheckCodeInfo t left join AssemblyType t1 on t.FAssemblyID = t1.FInterID where 1=1 ";
            if (!string.IsNullOrEmpty(gwId))
            {
                var type = (int)Enum.Parse(typeof(GwType), gwId);
                sql += $" and t.FGWID = {type} ";
            }
            if (typeId > 0)
            {
                sql += $" and t.FAssemblyID = {typeId} ";
            }
            sql += " Order By t.FGWID,t.FAssemblyID ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.Query<CheckCodeInfo>(sql).ToList();
                return re;
            }
        }

        public bool GetExistsCheckCode(GwType gwID, int assemblyID)
        {
            string sql = @"select count(1) from CheckCodeInfo where FGWID = @gwID and FAssemblyID = @assemblyID";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.QuerySingle<int>(sql, new { gwID = gwID, assemblyID = assemblyID }) > 0;
            }
        }

        public List<CheckCodeInfoEntry> GetCheckCodeInfoEntry(int id)
        {
            string sql = @"select * from CheckCodeInfoEntry where FInterID = @id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.Query<CheckCodeInfoEntry>(sql, new { id = id }).ToList();
                return re;
            }
        }

        public bool DeleteCheckItem(int id)
        {
            string sql = @"delete from CheckCodeInfo where FInterID = @id";
            string sql1 = @"delete from CheckCodeInfoEntry where FInterID = @id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                conn.Execute(sql1, new { id = id });
                return conn.Execute(sql, new { id = id }) > 0;
            }
        }

        public bool DeleteCheckEntryItem(int id)
        {
            string sql1 = @"delete from CheckCodeInfoEntry where FEntryID = @id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql1, new { id = id }) > 0;
            }
        }

        public bool UpdateCheckItem(CheckCodeInfo info)
        {
            string sql = @"UPDATE CheckCodeInfo SET FBarCodeRule = @rule WHERE FInterID = @Id ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { rule = info.FBarCodeRule, Id = info.FInterID }) > 0;
            }
        }

        public bool UpdateCheckEntryItem(CheckCodeInfoEntry info)
        {
            string sql = @"UPDATE CheckCodeInfoEntry SET FCodeRule = @rule WHERE FEntryID = @Id ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { rule = info.FCodeRule, Id = info.FEntryID }) > 0;
            }
        }

        public bool AddCheckItem(CheckCodeInfo info)
        {
            string sql = @"INSERT INTO CheckCodeInfo (FGWID,FAssemblyID,FBarCodeRule,FDate) VALUES(@gwID,@assemblyID,@codeRule,GETDATE()) ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { gwID = info.FGWID, assemblyID = info.FAssemblyID, codeRule = info.FBarCodeRule, }) > 0;
            }
        }

        public bool AddCheckEntryItem(CheckCodeInfoEntry info)
        {
            string sql = @"INSERT INTO CheckCodeInfoEntry (FInterID,FCodeRule) VALUES(@Id,@codeRule) ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { Id = info.FInterID, codeRule = info.FCodeRule }) > 0;
            }
        }
        #endregion

        #region record
        public List<RecordInfo> GetRecordInfo(string gwId, int typeId, string code, string custCode)
        {
            string sql = $"select top 500 t.*,t1.FAssemblyName from RecordInfo t left join AssemblyType t1 on t.FAssemblyID = t1.FInterID where 1=1 ";
            if (!string.IsNullOrEmpty(gwId))
            {
                var type = (int)Enum.Parse(typeof(GwType), gwId);
                sql += $" and t.FGWID = {type} ";
            }
            if (typeId > 0)
            {
                sql += $" and t.FAssemblyID = {typeId} ";
            }
            if (!string.IsNullOrEmpty(code))
            {
                sql += $" and t.FBar like '%{code}%' ";
            }
            if (!string.IsNullOrEmpty(custCode))
            {
                sql += $" and t.FCustBar like '%{custCode}%' ";
            }
            sql += " Order By t.FDate DESC ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.Query<RecordInfo>(sql).ToList();
                return re;
            }
        }

        public List<RecordInfoEntry> GetRecordInfoEntry(long id)
        {
            string sql = @"select * from RecordInfoEntry where FInterID = @id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.Query<RecordInfoEntry>(sql, new { id = id }).ToList();
                return re;
            }
        }

        public bool UpdateRecordEntryItem(RecordInfoEntry info)
        {
            string sql = @"UPDATE RecordInfoEntry SET FCode = @rule WHERE FEntryID = @Id ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { rule = info.FCode, Id = info.FEntryID }) > 0;
            }
        }

        public bool DeleteRecordEntryItem(long id)
        {
            string sql1 = @"delete from RecordInfoEntry where FEntryID = @id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql1, new { id = id }) > 0;
            }
        }

        public bool AddRecordItem(RecordInfo info)
        {
            string sql = @"INSERT INTO RecordInfo (FGWID,FAssemblyID,FBar,FDate) VALUES(@gwID,@assemblyID,@codeRule,GETDATE()) ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { gwID = info.FGWID, assemblyID = info.FAssemblyID, codeRule = info.FBar, }) > 0;
            }
        }

        public bool AddRecordEntryItem(RecordInfoEntry info)
        {
            string sql = @"INSERT INTO RecordInfoEntry (FInterID,FCode) VALUES(@Id,@codeRule) ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { Id = info.FInterID, codeRule = info.FCode }) > 0;
            }
        }

        public bool SaveInfo(RecordInfo info, GwType type)
        {
            string sql = @" INSERT INTO RecordInfo (FGWID,FAssemblyID,FBar,FCustBar,FDate) values 
                             (@gwID,@assemblyID,@codeRule,@custBar,GETDATE());select SCOPE_IDENTITY();";

            string sql1 = @"INSERT INTO RecordInfoEntry (FInterID,FCode) VALUES(@Id,@codeRule) ";
            string sql2 = @"INSERT INTO RecordInfoEntry1 (FInterID,FValue) VALUES(@Id,@value) ";

            string update = @"UPDATE RecordInfo SET FCustBar = @cust WHERE FBar = @bar";
            string sql3 = @"select FBar from RecordInfo where FCustBar = @cust";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    if (type == GwType.OP70) // 绑定客户条码
                    {
                        if (!string.IsNullOrEmpty(info.FBar) && !string.IsNullOrEmpty(info.FCustBar))
                        {
                            SqlCommand up1 = new SqlCommand(update, conn, tran);
                            up1.Parameters.AddWithValue("@cust", info.FCustBar);
                            up1.Parameters.AddWithValue("@bar", info.FBar);

                            up1.ExecuteNonQuery();
                        }
                    }
                    else if (type == GwType.OP80)
                    {
                        if (!string.IsNullOrEmpty(info.FCustBar))
                        {
                            info.FBar = conn.QuerySingleOrDefault<string>(sql3, new { cust = info.FCustBar });
                        }
                    }

                    SqlCommand cmd = new SqlCommand(sql, conn, tran);
                    cmd.Parameters.AddWithValue("@gwID", info.FGWID);
                    cmd.Parameters.AddWithValue("@assemblyID", info.FAssemblyID);
                    cmd.Parameters.AddWithValue("@codeRule", info.FBar);
                    cmd.Parameters.AddWithValue("@custBar", info.FCustBar);

                    long id = Convert.ToInt64(cmd.ExecuteScalar());
                    // 零件条码
                    if (info.entries != null)
                    {
                        if (info.entries.Any())
                        {
                            foreach (var l in info.entries)
                            {
                                SqlCommand cmd1 = new SqlCommand(sql1, conn, tran);
                                cmd1.Parameters.AddWithValue("@Id", id);
                                cmd1.Parameters.AddWithValue("@codeRule", l.FCode);

                                cmd1.ExecuteNonQuery();
                            }
                        }
                    }
                    // 气检数据
                    if (info.entries1 != null)
                    {
                        if (info.entries1.Any())
                        {
                            foreach (var l in info.entries1)
                            {
                                SqlCommand cmd1 = new SqlCommand(sql2, conn, tran);
                                cmd1.Parameters.AddWithValue("@Id", id);
                                cmd1.Parameters.AddWithValue("@value", l.FVaule);

                                cmd1.ExecuteNonQuery();
                            }
                        }
                    }

                    

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    tran.Rollback();
                    return false;
                }
            }

            #endregion

        }
    }
}
