using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using ZqUtils.Extensions;
/****************************
* [Author] 张强
* [Date] 2016-04-12
* [Describe] Sql帮助工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Sql帮助工具类
    /// </summary>
    public abstract class SqlHelper
    {
        #region 数据链接适配器
        /// <summary>
        /// 定义数据链接适配器变量
        /// </summary>
        public static SqlDataAdapter da;
        #endregion

        #region ExecuteNonQuery
        /// <summary>
        /// 执行sql命令
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">sql语句/参数化sql语句/存储过程名</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, int timeOut = 100, params SqlParameter[] commandParameters)
        {
            var cmd = new SqlCommand();
            if (timeOut > 0)
            {
                cmd.CommandTimeout = timeOut;
            }
            using (var conn = new SqlConnection(connectionString))
            {
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                return val;
            }
        }
        #endregion

        #region ExecuteReader
        /// <summary>
        ///  执行sql命令
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">sql语句/参数化sql语句/存储过程名</param>
        /// <param name="commandParameters">参数</param>
        /// <param name="timeOut">超时时长</param>
        /// <returns>SqlDataReader 对象</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, int timeOut = 100, params SqlParameter[] commandParameters)
        {
            var conn = new SqlConnection(connectionString);
            try
            {
                var cmd = new SqlCommand();
                if (timeOut > 0)
                {
                    cmd.CommandTimeout = timeOut;
                }
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return rdr;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "执行sql命令");
                conn.Close();
                throw;
            }
        }
        #endregion

        #region ExecuteDataset
        /// <summary>
        /// 执行Sql 命令
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">sql语句/参数化sql语句/存储过程名</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>DataSet 对象</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, int timeOut = 100, params SqlParameter[] commandParameters)
        {
            var conn = new SqlConnection(connectionString);
            try
            {
                var cmd = new SqlCommand();
                if (timeOut > 0)
                {
                    cmd.CommandTimeout = timeOut;
                }
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                da = new SqlDataAdapter(cmd);
                var ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "执行sql命令");
                conn.Close();
                return null;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region ExecuteDataTable
        /// <summary>
        /// 执行Sql 命令
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">sql语句/参数化sql语句/存储过程名</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>DataTable 对象</returns>
        public static DataTable ExecuteDataTable(string connectionString, CommandType commandType, string commandText, int timeOut = 100, params SqlParameter[] commandParameters)
        {
            var conn = new SqlConnection(connectionString);
            try
            {
                var cmd = new SqlCommand();
                if (timeOut > 0)
                {
                    cmd.CommandTimeout = timeOut;
                }
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "执行sql命令");
                conn.Close();
                return null;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region ExecuteScalar
        /// <summary>
        /// 执行Sql 命令
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">sql语句/参数化sql语句/存储过程名</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="commandParameters">参数</param>
        /// <returns>执行结果对象</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, int timeOut = 100, params SqlParameter[] commandParameters)
        {
            var cmd = new SqlCommand();
            if (timeOut > 0)
            {
                cmd.CommandTimeout = timeOut;
            }
            using (var conn = new SqlConnection(connectionString))
            {
                PrepareCommand(cmd, commandType, conn, commandText, commandParameters);
                object val = cmd.ExecuteScalar();
                return val;
            }
        }
        #endregion

        #region Private Method
        /// <summary>
        ///  设置一个等待执行的SqlCommand对象
        /// </summary>
        /// <param name="cmd">SqlCommand 对象，不允许空对象</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="conn">SqlConnection 对象，不允许空对象</param>
        /// <param name="commandText">Sql 语句</param>
        /// <param name="cmdParms">SqlParameters  对象,允许为空对象</param>
        private static void PrepareCommand(SqlCommand cmd, CommandType commandType, SqlConnection conn, string commandText, params SqlParameter[] cmdParms)
        {
            //打开连接
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            //设置SqlCommand对象
            cmd.Connection = conn;
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            if (cmdParms != null)
            {
                cmd.Parameters.AddRange(cmdParms);
            }
        }
        #endregion

        #region 防止sql语句注入攻击
        /// <summary>
        /// 防止sql语句的注入攻击
        /// </summary>
        /// <param name="s"></param>
        /// <returns>bool</returns>
        public static bool CheckSQL(string s)
        {
            var reg = @"^\w*[^or|exec|execute|insert|select|delete|update|alter|create|drop|count|and|where|in|like|chr|char|asc|mid|substring|master|truncate|declare|xp_cmdshell|restore|backup|net +user|\'|>|<|=|,]\w*$";
            return Regex.IsMatch(s, reg);
        }
        /// <summary>
        /// 防止sql注入
        /// </summary>
        /// <param name="s">源sql</param>
        /// <returns>string</returns>
        public static string ReplaceSQL(string s)
        {
            var sql = "insert |update |delete |exec |execute |' |=|create |drop |alter |column |dbo |sys ";
            var arr = sql.Split('|');
            for (int i = 0; i < arr.Length; i++)
            {
                s = s.Replace(arr[i], "");
            }
            return s;
        }
        #endregion

        #region Update DataTable
        /// <summary>
        /// 更新DataTable
        /// </summary>
        public static void UpdateDt(DataTable dt)
        {
            try
            {
                var cb = new SqlCommandBuilder(da);
                da.Update(dt);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "更新DataTable");
            }
        }
        #endregion

        #region Update DataSet
        /// <summary>
        /// 更新DataSet
        /// </summary>
        public static void UpdateDs(DataSet ds)
        {
            try
            {
                var cb = new SqlCommandBuilder(da);
                da.Update(ds);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "更新DataSet");
            }
        }
        #endregion

        #region SqlBulkCopy
        /// <summary>
        /// SqlBulkCopy批量插入
        /// </summary>
        /// <param name="dt">DataTable数据源</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="tableName">对应数据库表名</param>
        /// <param name="batchSize">每一批次中的行数</param>
        /// <param name="timeOut">超时之前操作完成所允许的秒数</param>
        /// <returns>bool</returns>
        public static bool SqlBulkCopy(DataTable dt, string connectionString, string tableName, int batchSize, int timeOut = 1800)
        {
            var result = true;
            try
            {
                if (dt?.Rows.Count > 0)
                {
                    using (var bulkCopy = new SqlBulkCopy(connectionString))
                    {
                        //每一批次中的行数
                        bulkCopy.BatchSize = batchSize;
                        //超时之前操作完成所允许的秒数
                        bulkCopy.BulkCopyTimeout = timeOut;
                        //将DataTable表名作为待导入库中的目标表名
                        bulkCopy.DestinationTableName = tableName;
                        //将数据集合和目标服务器库表中的字段对应 
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            //列映射定义数据源中的列和目标表中的列之间的关系
                            bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                        }
                        //将DataTable数据上传到数据表中
                        bulkCopy.WriteToServer(dt);
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "SqlBulkInsert");
                result = false;
            }
            return result;
        }

        /// <summary>
        /// SqlBulkCopy批量插入
        /// </summary>
        /// <param name="reader">IDataReader数据源</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="tableName">对应数据库表名</param>
        /// <param name="batchSize">每一批次中的行数</param>
        /// <param name="timeOut">超时之前操作完成所允许的秒数</param>
        /// <returns>bool</returns>
        public static bool SqlBulkCopy(IDataReader reader, string connectionString, string tableName, int batchSize, int timeOut = 1800)
        {
            var result = true;
            try
            {
                if (reader?.IsClosed == false)
                {
                    using (var bulkCopy = new SqlBulkCopy(connectionString))
                    {
                        //每一批次中的行数
                        bulkCopy.BatchSize = batchSize;
                        //超时之前操作完成所允许的秒数
                        bulkCopy.BulkCopyTimeout = timeOut;
                        //将DataTable表名作为待导入库中的目标表名
                        bulkCopy.DestinationTableName = tableName;
                        //将数据集合和目标服务器库表中的字段对应 
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            //列映射定义数据源中的列和目标表中的列之间的关系
                            bulkCopy.ColumnMappings.Add(reader.GetName(i), reader.GetName(i));
                        }
                        //将DataTable数据上传到数据表中
                        bulkCopy.WriteToServer(reader);
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "SqlBulkInsert");
                result = false;
            }
            return result;
        }

        /// <summary>
        /// SqlBulkCopy批量插入
        /// </summary>
        /// <param name="list">List数据源</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="tableName">对应数据库表名</param>
        /// <param name="batchSize">每一批次中的行数</param>
        /// <param name="timeOut">超时之前操作完成所允许的秒数</param>
        /// <returns>bool</returns>
        public static bool SqlBulkCopy<T>(List<T> list, string connectionString, string tableName, int batchSize, int timeOut = 1800)
        {
            var result = true;
            try
            {
                if (list?.Count > 0)
                {
                    SqlBulkCopy(list.ToDataTable(), connectionString, tableName, batchSize, timeOut);
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "SqlBulkInsert");
                result = false;
            }
            return result;
        }
        #endregion
    }
}
